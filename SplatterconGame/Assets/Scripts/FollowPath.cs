using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    [SerializeField]
    List<GameObject> nodes;
    GameObject currentNode;
    GameObject previousNode;
    public int nodeIndex;
    public bool atTargetNode;
    public Vector3 vehiclePosition;
    public Vector3 acceleration;
    public Vector3 velocity;

    // Use this for initialization
    void Start()
    {
        vehiclePosition = transform.position;
        nodeIndex = 0;
        currentNode = nodes[nodeIndex];
        previousNode = nodes[nodes.Count - 1];
        atTargetNode = false;
    }

    // Update is called once per frame
    void Update()
    {
        PathFollow(nodes);
        TargetReached();

        //Force movement
        //acceleration is the sum of the forces, need to clamp and divide
        vehiclePosition += velocity * Time.deltaTime;
        
      
        transform.position = vehiclePosition;
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
                nodeIndex = 0;
            }
            currentNode = path[nodeIndex];
            atTargetNode = false;
        }
    }

    void TargetReached()
    {
        Vector3 distance = currentNode.transform.position - transform.position;
        distance = new Vector3(distance.x, distance.y, 0);
        if (distance.magnitude <= 0.25f)
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

    

}
