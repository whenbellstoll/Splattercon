using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D _rb;
    private float _maxSpeed = 5.0f;
    private float _maxHealth = 100;
    private float _health;
    private float _viewRange = 3.0f;
    private float _widthMult = 0.8f;
    private Vector2 _destination = Vector2.zero;
    protected HealthBar _healthBar;
    private GameManager _gm;

    private float _pauseTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _health = _maxHealth;
        _healthBar = GetComponent<HealthBar>();
        _healthBar.SetMaxHealth(_maxHealth);
        _healthBar.UpdateHealth(_health);
    }

    // Update is called once per frame
    void Update()
    {
        if(_pauseTimer > 0)
            _pauseTimer -= Time.deltaTime;
        else
            SeekAttendeeAI();
    }

    public void SeekAttendeeAI()
    {
        _destination = _gm.GetNearestAttendee(transform.position);
        Vector3 destination = _destination;
        //Check for obstacle
        if (CheckObstacle(new Vector2(_destination.x, _destination.y)))
        {
            //Set destination to closest way to player that avoids obstacles
            destination = (Vector2)transform.position + AvoidObstacle(new Vector2(_destination.x, _destination.y));
        }

        //Seek destination
        Vector3 netForce = Seek(destination);
        if (Vector2.SqrMagnitude((Vector2)transform.position - _destination) < 0.1f * 0.1f)
        {
            _rb.velocity = Vector2.zero;
            netForce = Vector2.zero;
        }


        //Debug.DrawLine(transform.position, transform.position + netForce, Color.red);
        _rb.AddForce(netForce);
    }

    public void BasicAI()
    {
        if (_destination == Vector2.zero)
        {
            _destination = _gm.GetNearestBooth(transform.position);
        }
        else
        {
            Vector2 netForce = Vector2.zero;
            netForce += Seek(_destination);
            _rb.AddForce(netForce);
        }
    }

    public void AvoidAI()
    {
        Vector3 destination = _destination;
        //Check for obstacle
        if (CheckObstacle(new Vector2(_destination.x, _destination.y)))
        {
            //Set destination to closest way to player that avoids obstacles
            destination = (Vector2)transform.position + AvoidObstacle(new Vector2(_destination.x, _destination.y));
        }

        //Seek destination
        Vector3 netForce = Seek(destination);
        if (Vector2.SqrMagnitude((Vector2)transform.position - _destination) < 0.1f * 0.1f)
        {
            _rb.velocity = Vector2.zero;
            netForce = Vector2.zero;
        }


        //Debug.DrawLine(transform.position, transform.position + netForce, Color.red);
        _rb.AddForce(netForce);
    }


    /// <summary>
    /// Steers the body towards a desired velocity
    /// </summary>
    /// <param name="desiredVelocity">Desired Velocity</param>
    /// <returns>Force to steer the body</returns>
    public Vector2 Steer(Vector2 desiredVelocity)
    {
        //Set up desired velocity
        desiredVelocity = desiredVelocity.normalized;
        desiredVelocity *= _maxSpeed;

        //Calc steering force
        Vector2 steeringForce = desiredVelocity - _rb.velocity;

        //Return force
        return steeringForce;
    }

    /// <summary>
    /// Seeks a target destination
    /// </summary>
    /// <param name="target">Target to seek</param>
    /// <returns>Force to seek the target</returns>
    public Vector2 Seek(Vector2 target)
    {
        Vector3 desiredVelocity = target - (Vector2)transform.position;
        return Steer(desiredVelocity);
    }

    /// <summary>
    /// Changes the speed of the enemy for an amount of time
    /// </summary>
    /// <param name="speed">float value to change _maxSpeed</param>
    /// <param name="time">float value for how long speed will be changed</param>
    public void ChangeSpeed(float speed, float time)
    {
        time -= Time.deltaTime;
        if(time > 0)
        {
           _maxSpeed = speed;
        }
        else
        {
            speed = 5.0f;
        }
        
    }

    /// <summary>
    /// Checks if there is an obstical in the enemy's path
    /// </summary>
    /// <returns>If enemy's path is interuptted</returns>
    protected bool CheckObstacle(Vector2 target)
    {
        RaycastHit2D[] hits;
        Vector2 detectPosition = transform.position;
        Vector2 targetDir = target - (Vector2)transform.position;
        targetDir.Normalize();

        Debug.DrawRay(detectPosition, Quaternion.AngleAxis(0, Vector3.forward) * targetDir * _viewRange, Color.red);
        //Check right
        hits = UnityEngine.Physics2D.CircleCastAll(detectPosition, _widthMult, targetDir, _viewRange);
        foreach (RaycastHit2D hit in hits)
        {
            //Make sure hit was not from their own hitbox
            if (hit.collider.gameObject != gameObject && hit.collider.tag != "Attendee" && hit.collider.tag != "Enemy")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Find direction to avoid obstacle
    /// </summary>
    /// <returns>Direction to avoid obstacle</returns>
    protected Vector2 AvoidObstacle(Vector2 target)
    {
        //Debug.Log("Avoiding Obstacle");
        Vector2 dir = Vector3.zero;
        bool found = false;
        bool noHit = false;

        Vector2 detectPosition = transform.position;
        Vector2 targetDir = target - (Vector2)transform.position;
        targetDir.Normalize();
        RaycastHit2D[] hits;

        //Check 90 degrees for a path to avoid obstacle
        for (int i = 0; i <= 90; i += 6)
        {
            noHit = true;
            //Check right side for path
            hits = Physics2D.CircleCastAll(detectPosition, _widthMult, Quaternion.AngleAxis(i, Vector3.forward) * targetDir, _viewRange * 1.5f);
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && hit.collider.tag != "Attendee" && hit.collider.tag != "Enemy")
                    {
                        noHit = false;
                    }
                }
            }
            if (noHit)
            {
                //Set direction if path is found
                dir = Quaternion.AngleAxis(i, Vector3.forward) * targetDir;
                Debug.DrawLine(transform.position, (Vector2)transform.position + dir * _viewRange * 1.5f, Color.yellow);
                found = true;
            }

            noHit = true;
            hits = Physics2D.CircleCastAll(detectPosition, _widthMult, Quaternion.AngleAxis(-i, Vector3.forward) * targetDir, _viewRange * 1.5f);
            //Check left side for path
            if (hits.Length > 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && hit.collider.tag != "Attendee" && hit.collider.tag != "Enemy")
                    {
                        noHit = false;
                    }
                }
            }
            if (noHit)
            {
                //Set direction if path is found
                dir = Quaternion.AngleAxis(-i, Vector3.forward) * targetDir;
                Debug.DrawLine(transform.position, (Vector2)transform.position + dir * _viewRange * 1.5f, Color.yellow);
                found = true;
            }

            if (found)
            {
                return dir;
            }
        }

        return -targetDir;
    }

    //Kill attendee on coming in contact with them
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Attendee" && _pauseTimer <= 0)
        {
            Destroy(collision.gameObject);
            _pauseTimer = 1.0f;
            _rb.velocity = Vector2.zero;
        }
    }
}
