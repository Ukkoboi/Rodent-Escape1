using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControls : MonoBehaviour
{
    public float movementspeed;
    public float turningspeed;
    public float detectRange;
    public float stoppingRange;
    public float switchTargetRange;
    public float switchDistance;

    public float AIDelay;

    public string stringState;
    public AudioSource audioSource;

    private Rigidbody rb;
    private float AIt;

    private GameObject targetObject;
    private Vector3 target;

    private int obstacleMask;

    private enum State { forward, left, right, back, stop };
    private State state;
    private State nextState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        AIt = 0f;
        obstacleMask = LayerMask.GetMask("Obstacle");
        state = State.forward;
        nextState = State.forward;
    }

    void FixedUpdate()
    {

        Vector3 currentRotation = rb.rotation.eulerAngles;
        rb.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);

        if (Vector3.Distance(transform.position, target) < switchTargetRange)
        {
            float randomx = Random.Range(-switchDistance, switchDistance);
            float randomz = Random.Range(-switchDistance, switchDistance);

            target += new Vector3(randomx, 0f, randomz);
        }

        if (targetObject != null)
        {
            if (Vector3.Distance(transform.position, targetObject.transform.position) < detectRange)
            {
                if (!Physics.Linecast(transform.position, targetObject.transform.position, obstacleMask))
                {
                    target = targetObject.transform.position;

                }
            }
        }
        else
        {
            targetObject = GameObject.FindGameObjectWithTag("Player");
        }

        Debug.DrawLine(target, target + new Vector3(0f, 5f, 0f), Color.green);

        float angle = Vector3.SignedAngle(transform.forward, target - transform.position, Vector3.up);

        if (AIt < 0)
        {
            state = nextState;
            AIt = AIDelay;
        }
        else
        {
            AIt -= Time.deltaTime;
        }

        if (state == State.forward)
        {
            stringState = "forward";
            if (angle < 0)
            {
                Turning(-1f);
            }
            else if (angle > 0)
            {
                Turning(1f);
            }

            if (Mathf.Abs(angle) < 90)
            {
                Move(1f);
            }
        }
        else if (state == State.left)
        {
            stringState = "left";
            Turning(-1f);
            Move(1f);
        }
        else if (state == State.right)
        {
            stringState = "right";
            Turning(1f);
            Move(1f);
        }
        else if (state == State.back)
        {
            stringState = "back";
            Move(-1f);
            nextState = State.forward;
        }
        else if (state == State.stop)
        {
            stringState = "stop";
            Move(0f);
            nextState = State.forward;
        }

    }

    private void Move(float input)
    {
        Vector3 movement = transform.forward * input * movementspeed;
        rb.velocity = movement;
    }

    private void Turning(float input)
    {
        Vector3 turning = Vector3.up * input * turningspeed;
        rb.angularVelocity = turning;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!other.gameObject.CompareTag("Obstacle") && !other.gameObject.CompareTag("Wall"))
        {
            return;
        }

        RaycastHit leftHit;
        RaycastHit rightHit;

        float leftLength = 0f;
        float rightLength = 0f;

        if (Physics.Raycast(transform.position, transform.forward + transform.right * -1, out leftHit, Mathf.Infinity, obstacleMask))
        {
            leftLength = leftHit.distance;
        }
        if (Physics.Raycast(transform.position, transform.forward + transform.right, out rightHit, Mathf.Infinity, obstacleMask))
        {
            rightLength = rightHit.distance;
        }
        if (leftLength > rightLength)
        {
            state = State.left;
            target = leftHit.point;
        }
        else
        {
            state = State.right;
            target = rightHit.point;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        nextState = State.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Obstacle") && !collision.gameObject.CompareTag("Wall"))
        {
            return;
        }

        state = State.back;
    }
}
