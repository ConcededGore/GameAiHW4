using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmHead : MonoBehaviour {

    [Header("Path Following")]
    public PathBuilder pb;
    public int pathNum = 0;
    private LNode curr;
    private List<LNode> paths = new List<LNode>();
    private bool pathStarted = false;
    public bool needsPath = true;

    [Header("Movement")]
    public float maxVelocity = 8.0f;
    public float maxAcceleration = 0.2f;
    public float maxAngularAcceleration = 0.5f;

    public float arriveSlowRadius = 3.0f;
    public float pursueDistance;

    [Header("Turning")]
    public float turnSatiationDist = 5;
    public float turnSpeedTick = 15;
    public float turnMaxSpeed = 60;

    [Header("Target Stuff")]
    public Transform target;

    private MovementController mc;
    private Vector3 linearAcceleration;

    // Start is called before the first frame update
    void Start() {
        linearAcceleration = new Vector3(0, 0, 0);
        mc = GetComponent<MovementController>();
        if(needsPath)
            paths = pb.GetPaths();
    }

    // Update is called once per frame
    void Update() {
        if (!pathStarted && needsPath) {
            if (pathNum >= paths.Count) {
                Debug.Log("ERROR: Broodmother unable to find assigned path #" + pathNum);
            }
            curr = paths[pathNum];
            target = curr.GetComponentInParent<Transform>();
            pathStarted = true;
        }
        if(needsPath)
            FollowPath();

        //Clamp Accelerations
        linearAcceleration = Vector3.ClampMagnitude(linearAcceleration, maxAcceleration);

        //Multiple by Time.deltaTime
        linearAcceleration *= Time.deltaTime;

        //Move using the accelerations
        if(needsPath)
            mc.Move(linearAcceleration, 0);
    }

    void FollowPath() {
        if (target == null && curr.num != 0) {
            target = curr.GetComponentInParent<Transform>();
        }
        Transform me = gameObject.GetComponent<Transform>();
        float dist = Mathf.Pow((target.position.x - me.position.x), 2) + Mathf.Pow((target.position.y - me.position.y), 2);
        //Debug.Log("(" + target.position.x + ", " + target.position.y + ", " + target.position.z + ") " + dist);
        if (dist < 1) {
            if (curr.next == null) {
                return;
            }
            curr = curr.next;
            if (curr != null) {
                target = curr.GetComponentInParent<Transform>();
            }
        }
        DynamicSeek();
    }

    void DynamicSeek() {
        //Go max acceleration towards object, look at it
        if (target == null) {
            Debug.Log("Target Not Set");
            return;
        }

        //Move towards position
        Vector3 positionDifference = target.position - transform.position;
        linearAcceleration = maxAcceleration * positionDifference.normalized;

        //Lynch said to use angular acceleration to look towards object
        //DynamicFace();

        //Visualisation
        Debug.DrawLine(transform.position, target.position, Color.red);
    }

    void DynamicFace(int mode = 0) {
        //Look towards target
        //TODO: Look towards the player, but do it in a smoothed motion

        if (target == null) {
            return;
        }

        float xDif = target.position.x - transform.position.x;
        float yDif = target.position.y - transform.position.y;

        float hyp = Mathf.Sqrt((xDif * xDif) + (yDif * yDif));

        float targetAngle;
        float currentAngle = transform.localEulerAngles.z;

        targetAngle = Mathf.Acos(yDif / hyp);
        targetAngle = Mathf.Rad2Deg * targetAngle;



        float currentAngVel = GetComponent<Rigidbody>().angularVelocity.magnitude; // I made a change here that I dont want to forget in case it breaks

        if (target.position.x > transform.position.x) {
            targetAngle = targetAngle * -1;
        }

        if (currentAngle > 180) {
            currentAngle = -1 * (360 - currentAngle);
        }

        //Debug.Log("target angle is " + targetAngle);
        //Debug.Log("current angle is " + currentAngle);

        if (mode == 1) {
            targetAngle = targetAngle - 180;
        }

        if (GetComponent<Rigidbody>().angularVelocity.x != 0 || GetComponent<Rigidbody>().angularVelocity.y != 0) {
            Debug.Log("Ruh roh raggy, rou rot a rug!");
        }

        if (Mathf.Abs(currentAngle - targetAngle) < turnSatiationDist) {
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,0);
        }
        else {
            if (currentAngle < targetAngle) {
                GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, currentAngVel + 15);
                if (GetComponent<Rigidbody>().angularVelocity.z > turnMaxSpeed) {
                    GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0, turnMaxSpeed);
                }
            }
            else {
                GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0, currentAngVel - 15);
                if (GetComponent<Rigidbody>().angularVelocity.z < turnMaxSpeed) {
                    GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0, -1 * turnMaxSpeed);
                }
            }


        }

    }

}
