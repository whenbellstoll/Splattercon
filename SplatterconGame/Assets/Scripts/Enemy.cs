using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D _rb;
    private float _maxSpeed = 5.0f;
    private float _maxHealth = 100;
    private float _health;
    private Vector2 _destination = Vector2.zero;
    protected HealthBar _healthBar;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = _maxHealth;
        _healthBar = GetComponent<HealthBar>();
        _healthBar.SetMaxHealth(_maxHealth);
        _healthBar.UpdateHealth(_health);
    }

    // Update is called once per frame
    void Update()
    {
        BasicAI();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _destination = GetNearestBooth();
        }
    }

    public void BasicAI()
    {
        if(_destination == Vector2.zero)
        {
            _destination = GetNearestBooth();
        }
        else
        {
            Vector2 netForce = Vector2.zero;
            netForce += Seek(_destination);
            _rb.AddForce(netForce);
        }
    }

    public Vector2 GetNearestBooth()
    {
        GameObject booths = GameObject.Find("PlacedObjects");
        float closestDist = float.MaxValue;
        Vector2 closestBooth = Vector2.zero;
        for(int i = 0; i < booths.transform.childCount; i++)
        {
            Vector2 boothpos = booths.transform.GetChild(i).position;
            if(Vector2.SqrMagnitude((Vector2)transform.position - boothpos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude((Vector2)transform.position - boothpos);
                closestBooth = boothpos;
            }
        }
        if(closestBooth == Vector2.zero)
        {
            closestBooth = transform.position;
        }

        Debug.Log(closestBooth);
        return closestBooth;
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
}
