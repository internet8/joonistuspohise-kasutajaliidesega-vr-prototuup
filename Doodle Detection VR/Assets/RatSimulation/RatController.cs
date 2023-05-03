using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatController : MonoBehaviour
{
    public float defaultHeight = 0.178f;
    public float speed = 0.1f;
    public float force = 0.1f;
    public float rotationSpeed = 0.1f;
    public Vector3 velocity = new Vector3(0, 0, 0);
    public Vector3 acceleration = new Vector3(0, 0, 0);
    float timeToChangeDir = 0;
    float dirTime = 2;

    // seek
    public float maxVelocity = 1;
    public float detectionRad = 3f;
    public GameObject player;
    Vector3 desiredVel = new Vector3(0, 0, 0);

    // evade
    public float evadeRadius = 5;
    public float jumpBackcircleRadius = 1;
    public float jumpBackHeightFactor = 0.3f;
    float jumpBackRadius;
    bool evading = false;

    // wander
    public float wanderCircleDist = 5;
    public float wanterCircleRad = 5f;
    public float wanderMaxDisplacement = 0.5f;
    float wanderCircleAngle = 0;

    void Start()
    {
        jumpBackRadius = evadeRadius - jumpBackcircleRadius;
        maxVelocity += Random.Range(-maxVelocity / 3f, maxVelocity / 3f);
    }

    void Update()
    {
        // wander
        acceleration = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        acceleration = Vector3.ClampMagnitude(acceleration, force);

        // seek
        if (Vector3.Distance(transform.position, player.transform.position) < detectionRad && !evading)
        {
            desiredVel = Vector3.Normalize(new Vector3(player.transform.position.x, 0, player.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)) * speed;
            acceleration = Vector3.Normalize(desiredVel - velocity);
            acceleration = Vector3.ClampMagnitude(acceleration, force);
        }

        // evade
        if (Vector3.Distance(transform.position, player.transform.position) < evadeRadius)
        {
            evading = true;
            desiredVel = Vector3.Normalize(new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(player.transform.position.x, 0, player.transform.position.z)) * speed;
            acceleration = Vector3.Normalize(desiredVel - velocity);
            acceleration = Vector3.ClampMagnitude(acceleration, force * 3);
        } else if (evading)
        {
            evading = false;
        }

        // moving the rat object
        if (evading)
        {
            float jumpHeight = 0;
            float distFromEvadingArea = Mathf.Abs(Vector3.Distance(transform.position, player.transform.position) - jumpBackRadius);
            if (distFromEvadingArea < jumpBackcircleRadius / 2)
            {
                jumpHeight = distFromEvadingArea;
            } else if (distFromEvadingArea < jumpBackcircleRadius)
            {
                jumpHeight = jumpBackcircleRadius - distFromEvadingArea;
            }
            transform.position = new Vector3(transform.position.x, jumpHeight * jumpBackHeightFactor, transform.position.z);
        } else
        {
            transform.position = new Vector3(transform.position.x, defaultHeight, transform.position.z);
        }
        velocity += acceleration * Time.deltaTime * 75;
        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
        transform.position = transform.position + velocity * Time.deltaTime;

        // rotating the rat object
        Quaternion lookRot = Vector3.Distance(transform.position, player.transform.position) < detectionRad ? Quaternion.LookRotation(new Vector3(player.transform.position.x, 0, player.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)) : Quaternion.LookRotation(velocity);
        //transform.rotation = new Quaternion(transform.rotation.x, lookRot.y, transform.rotation.z, transform.rotation.w); ;
        //transform.eulerAngles += new Vector3(0, 90, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Mathf.Clamp01(rotationSpeed * Time.maximumDeltaTime));

        //if (Time.time > timeToChangeDir)
        //{
        //    timeToChangeDir = Time.time + Random.Range(1f, 3f);
        //    direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        //    direction = Vector3.Normalize(direction);
        //}

        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity, out hit, 1))
        {
            Debug.DrawRay(transform.position, velocity * hit.distance, Color.yellow);
            if (hit.transform.tag == "Wall")
            {
                velocity = velocity * -1;
            }
        }
    }
}
