using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void FinalNodeCallback(GameObject obj);

public class FollowPath : MonoBehaviour
{
    [SerializeField]
    List<GameObject> nodes;
    GameObject currentNode;
    GameObject previousNode;
    private int nodeIndex = -1;
    private bool atTargetNode;
    private Vector3 vehiclePosition;
    private Vector3 acceleration;
    private Vector3 velocity;

    private float _pauseTimer = 0;

    [SerializeField]
    private GameObject _deathParticles;

    public FinalNodeCallback FinalNodeReached;

    // Use this for initialization
    void Start()
    {
        vehiclePosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (nodeIndex != -1)
        {
            if (_pauseTimer > 0)
            {
                _pauseTimer -= Time.deltaTime;
            }
            else
            {
                PathFollow(nodes);
                TargetReached();
            }
        }

        //Force movement
        //acceleration is the sum of the forces, need to clamp and divide
        vehiclePosition += velocity * Time.deltaTime;
        
      
        transform.position = vehiclePosition;
    }

    public void SetNodes(List<GameObject> newnodes)
    {
        nodes = newnodes;
        nodeIndex = 0;
        currentNode = nodes[nodeIndex];
        previousNode = nodes[nodes.Count - 1];
        atTargetNode = false;
    }

    void PathFollow(List<GameObject> path)
    {
        if (!atTargetNode)
        {
            Seek(currentNode);     

        }
        else
        {
            previousNode = currentNode;
            nodeIndex++;
            if (nodeIndex >= path.Count)
            {
                FinalNodeReached?.Invoke(gameObject);
                nodeIndex = -1;
            }
            else
            {
                currentNode = path[nodeIndex];
                atTargetNode = false;
                _pauseTimer = 0.5f;
                velocity = Vector3.zero;
            }
        }
    }

    void TargetReached()
    {
        Vector3 distance = currentNode.transform.position - transform.position;
        distance = new Vector3(distance.x, distance.y, 0);
        if (distance.sqrMagnitude <= 0.5f * 0.5f)
        {
            atTargetNode = true;
        }
    }

    public void Seek(GameObject target)
    {
        Seek(target.transform.position);
    }

    public void Seek(Vector3 targetLocation)
    {

        // Step 1:  Calculate desired velocity
        // Vector "pointing" from myself (Vehicle) to my target
        Vector3 desiredVelocity = targetLocation - vehiclePosition;


        // Another way of "setting" magnitude of DV
        // Normalize and * maxSpeed
        desiredVelocity.Normalize();
        if( velocity.normalized != desiredVelocity )
        {
            velocity = desiredVelocity * 3;
        }
    }

    public void Death()
    {
        Instantiate(_deathParticles, transform.position, _deathParticles.transform.rotation);
    }
}
