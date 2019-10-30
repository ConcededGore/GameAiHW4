using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarmer : MonoBehaviour {

    [Header("Flocking")]
    public SwarmHead sh;
    public float threshold = 5;

    private float strength = 0;
    public Transform target;

    [Header("Turning")]
    public float turnSatiationDist = 5;
    public float turnSpeedTick = 15;
    public float turnMaxSpeed = 60;

    [Header("Movement")]
    public float maxVelocity = 8.0f;
    public float maxAcceleration = 80f;
    public float maxAngularAcceleration = 0.5f;

    public float arriveSlowRadius = 3.0f;
    public float pursueDistance;

    private Vector3 linearAcceleration;
    private Quaternion angularAcceleration;

    private MovementController mc;


    private int collisionMode = 0;
    public int numRays = 8;
    public float rayDist = 1;
    public float repelForce = 2;
    public ContactFilter2D cf2;
    // Start is called before the first frame update
    void Start() {
        linearAcceleration = new Vector3(0, 0, 0);
        mc = GetComponent<MovementController>();
    }

    // Update is called once per frame
    void Update() {

        if(Input.GetKeyDown("c"))
        {
            collisionMode = 0;
        }
        else if(Input.GetKeyDown("s"))
        {
            collisionMode = 1;
        }

        Flock();
        CollisionAvoid();
        //Clamp Accelerations
        linearAcceleration = Vector3.ClampMagnitude(linearAcceleration, maxAcceleration);

        //Multiple by Time.deltaTime
        linearAcceleration *= Time.deltaTime;

        //Move using the accelerations
        mc.Move(linearAcceleration, 0);
    }

    float GetDist(Transform a, Transform b) {
        return (a.position - b.position).magnitude;
    }

    List<GameObject> GetLarvaInRange() {
        GameObject[] allLarva = GameObject.FindGameObjectsWithTag("Hunter");
        List<GameObject> retval = new List<GameObject>();
        for (int i = 0; i < allLarva.Length; i++) {
            if (GetDist(allLarva[i].GetComponent<Transform>(), GetComponent<Transform>()) < 5) {
                retval.Add(allLarva[i]);
            }
        }
        return retval;
    }

    void Flock() {
        List<GameObject> larva = GetLarvaInRange();

        // Seperation (This is what will be replaced by collision avoidance)
        List<Vector3> forces = new List<Vector3>();
        Vector3 Seperation = new Vector3(0,0,0);

        for (int i = 0; i < larva.Count; i++) {
            if (threshold == 0) {
                Debug.Log("ERROR: Threshold being 0 will result in division by 0");
                return;
            }
            strength = (maxAcceleration) * (threshold - GetDist(larva[i].GetComponent<Transform>(), GetComponent<Transform>())) / threshold;
            forces.Add((GetComponent<Transform>().position - larva[i].GetComponent<Transform>().position).normalized * strength);
        }
        for (int i = 0; i < forces.Count; i++) {
            Seperation += forces[i];
        }

        // Align
        List<Vector3> vels = new List<Vector3>();
        Vector3 sum = new Vector3(0, 0, 0);
        for (int i = 0; i < larva.Count; i++) {
            vels.Add(larva[i].GetComponent<Rigidbody>().velocity);
        }
        for (int i = 0; i < larva.Count; i++) {
            vels[i] = vels[i].normalized;
        }
        for (int i = 0; i < larva.Count; i++) {
            sum += vels[i];
        }
        if (vels.Count == 0) {
            Debug.Log("No Targets");
        } else {
            sum /= vels.Count;
            AlignWithFlock(sum, -1);
        }

        // Cohesion
        target = sh.GetComponent<Transform>();
        DynamicPursueWithDynamicArrive();
        //Debug.Log(linearAcceleration + " " + linearAcceleration.magnitude + " " + Seperation);
        linearAcceleration += Seperation;

    }

    void CollisionAvoid()
    {
        if(collisionMode == 0)
        {
            //RaycastHit[] collectedHits = new RaycastHit[numRays];
            for (int i = 0; i < numRays; i++)
            {
                float angle = (transform.localEulerAngles.y + 45 + (90 * i / numRays)) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Debug.DrawLine(transform.position, transform.position + (new Vector3(dir.x, 0, dir.y) * rayDist), new Color(1, 1, 0, 0.3f));
                RaycastHit hit;
                bool ifHit = Physics.Raycast(transform.position, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)), out hit, rayDist);
                if(ifHit)
                {
                    if (Mathf.Cos(transform.localEulerAngles.y) > Vector3.Dot(transform.localEulerAngles, hit.transform.localEulerAngles))
                    {
                        //float angle = (transform.localEulerAngles.y - 45 + (90 * i / numRays)) * Mathf.Deg2Rad;
                        Vector3 dir2 = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                        linearAcceleration += -dir2 * (1 - (hit.distance / rayDist)) * repelForce;
                    }
                }
            }
        }
        else if(collisionMode == 1)
        {
            RaycastHit[] collectedHits = new RaycastHit[numRays];
            for (int i = 0; i < 4 * numRays; i++)
            {
                float angle = (360f* ( i / numRays)) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Debug.DrawLine(transform.position, transform.position + (new Vector3(dir.x, 0, dir.y) * rayDist), new Color(1, 1, 0, 0.3f));
                RaycastHit hit;
                bool ifHit = Physics.Raycast(transform.position, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)), out hit, rayDist);
                if (ifHit)
                {
                    collectedHits[i] = hit;
                }
            }
        }
    }

    void DynamicPursueWithDynamicArrive() {
        if (target == null || target.GetComponent<Rigidbody>() == null) {
            Debug.Log("Test");
            return;
        }
        //Move towards where the target is moving
        //PURSUE
        Vector3 targetVelocity = target.GetComponent<Rigidbody>().velocity;
        Vector3 targetPosition = target.position + (targetVelocity * pursueDistance);
        Vector2 positionDifference = targetPosition - transform.position;
        linearAcceleration = maxAcceleration * positionDifference.normalized;

        //Slow down by clamping velocity as getting closer
        //ARRIVE
        float remainingDist = Vector3.Distance(targetPosition, transform.position);
        if (remainingDist < arriveSlowRadius) {
            float slowAmount = 1.0f - ((arriveSlowRadius - remainingDist) / arriveSlowRadius);
            //Debug.Log(slowAmount);
            mc.ClampVelocity(slowAmount * maxVelocity);
        }

        //Visualization
        Debug.DrawLine(transform.position, targetPosition, Color.blue);
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

        if (GetComponent<Rigidbody>().angularVelocity.x != 0 || GetComponent<Rigidbody>().angularVelocity.z != 0) {
            Debug.Log("Ruh roh raggy, rou rot a rug!");
        }

        if (Mathf.Abs(currentAngle - targetAngle) < turnSatiationDist) {
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
        }
        else {
            if (currentAngle < targetAngle) {
                GetComponent<Rigidbody>().angularVelocity = new Vector3(0, currentAngVel + 15, 0);
                if (GetComponent<Rigidbody>().angularVelocity.y > turnMaxSpeed) {
                    GetComponent<Rigidbody>().angularVelocity = new Vector3(0, turnMaxSpeed, 0);
                }
            }
            else {
                GetComponent<Rigidbody>().angularVelocity = new Vector3(0, currentAngVel - 15, 0);
                if (GetComponent<Rigidbody>().angularVelocity.y < turnMaxSpeed) {
                    GetComponent<Rigidbody>().angularVelocity = new Vector3(0, -1 * turnMaxSpeed, 0);
                }
            }


        }

    }

    void AlignWithFlock(Vector3 targ, int mode = 0) {
        //Look towards target

        float xDif = targ.x - transform.position.x;
        float yDif = targ.y - transform.position.y;

        float hyp = Mathf.Sqrt((xDif * xDif) + (yDif * yDif));

        float targetAngle;
        float currentAngle = transform.localEulerAngles.y;

        targetAngle = Mathf.Acos(yDif / hyp);
        targetAngle = Mathf.Rad2Deg * targetAngle;



        float currentAngVel = GetComponent<Rigidbody>().angularVelocity.magnitude;

        if (targ.x > transform.position.x) {
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

        if (Mathf.Abs(currentAngle - targetAngle) < turnSatiationDist) {
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,0);
        }
        else {
            if (currentAngle < targetAngle) {
                GetComponent<Rigidbody>().angularVelocity = new Vector3(0, currentAngVel + 15,0);
                if (GetComponent<Rigidbody>().angularVelocity.magnitude > turnMaxSpeed) {
                    GetComponent<Rigidbody>().angularVelocity = new Vector3(0, turnMaxSpeed,0);
                }
            }
            else {
                GetComponent<Rigidbody>().angularVelocity = new Vector3(0, currentAngVel - 15,0);
                if (GetComponent<Rigidbody>().angularVelocity.magnitude < turnMaxSpeed) {
                    GetComponent<Rigidbody>().angularVelocity = new Vector3(0, -1 * turnMaxSpeed,0);
                }
            }


        }

    }


}
