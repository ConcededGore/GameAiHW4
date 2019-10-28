using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MovementController))]
public class PlayerController : MonoBehaviour {

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

    [Header("Wander")]
    //public float wanderTargDist = 2;
    public float wanderTargRad = 2f;
    public float wanderShiftDelay = .2f;
    float wanderTick = 0;
    public Transform wanderCenter;

    [Header("Ignore this section")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public int mode = Modes.DYNAMICSEEK;
    public bool smartAvoid = false;

    [Header("Target Stuff")]
    public Transform target;

    public GameObject wanderTarget;

    private MovementController mc;

    //Can only edit these 2 values in the movement functions
    private Vector2 linearAcceleration;
    private Quaternion angularAcceleration;

    [Header("Smart Movement")]
    public int numRays = 8;
    public float rayDist = 1;
    public float repelForce = 2;
    public ContactFilter2D cf2;

    [Header("Path Following")]
    public PathBuilder pb;

    private LNode curr;
    private List<LNode> paths = new List<LNode>();
    private bool pathStarted = false;

    void Start() {
        linearAcceleration = new Vector2(0, 0);
        mc = GetComponent<MovementController>();

        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        minX = 1000;
        maxX = -1000;
        minY = 1000;
        maxY = -1000;

        for(int x = 0; x < walls.Length; x++)
        {
            if(walls[x].transform.position.x < minX)
            {
                minX = walls[x].transform.position.x;
            }
            if (walls[x].transform.position.x > maxX)
            {
                maxX = walls[x].transform.position.x;
            }
            if (walls[x].transform.position.y < minY)
            {
                minY = walls[x].transform.position.y;
            }
            if (walls[x].transform.position.y > maxY)
            {
                maxY = walls[x].transform.position.y;
            }
        }

        minX += 1f;
        maxX -= 1f;
        minY += 1f;
        maxY -= 1f;
    }

    void Update() {
        paths = pb.GetPaths();
        //Set Accelerations based on mode
            switch (mode) {
                case Modes.DYNAMICSEEK:
                    DynamicSeek();
                    break;
                case Modes.DYNAMICFLEE:
                    DynamicFlee();
                    break;
                case Modes.DYNAMICPURSUREARRIVE:
                    DynamicPursueWithDynamicArrive();
                    break;
                case Modes.DYNAMICEVADE:
                    DynamicEvade();
                    break;
                case Modes.DYNAMICALIGN:
                    DynamicAlign();
                    break;
                case Modes.DYNAMICFACE:
                    DynamicFace();
                    break;
                case Modes.DYNAMICWANDER:
                    DynamicWander();
                    break;
                case Modes.SMARTERWANDER:
                    SmarterWander();
                    break;
                case Modes.PATHFOLLOW:
                    if (!pathStarted) {
                        curr = paths[0];
                        target = curr.GetComponentInParent<Transform>();
                        pathStarted = true;
                    }
                    FollowPath();
                    break;
                default:
                    Debug.Log("Unknown Mode " + mode);
                    break;
            }

            if (smartAvoid) {
                AvoidObjects();
            }

            //Clamp Accelerations
            linearAcceleration = Vector3.ClampMagnitude(linearAcceleration, maxAcceleration);

            //Multiple by Time.deltaTime
            linearAcceleration *= Time.deltaTime;

            //Move using the accelerations
            mc.Move(linearAcceleration, 0);
            //Make sure the velocity isn't too fast
            mc.ClampVelocity(maxVelocity);
            //Angular stuff

            if (wanderTarget.activeSelf && (mode != Modes.DYNAMICWANDER && mode != Modes.SMARTERWANDER)) {
                wanderTarget.SetActive(false);
            }
    }

    void AvoidObjects() {
        //Draw Rays
        for (int i = 0; i < numRays; i++) {
            float angle = 360.0f * i / numRays * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Debug.DrawLine(transform.position, transform.position + (new Vector3(dir.x, dir.y, 0) * rayDist), new Color(1, 1, 0, 0.3f));
            RaycastHit2D[] hits = new RaycastHit2D[1];
            int numHits = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), dir, cf2, hits, rayDist);
            if (numHits > 0) {
                linearAcceleration += -dir * (1-(hits[0].distance / rayDist)) * repelForce;
            }
        }
        //public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance = Mathf.Infinity);
    }

    void DynamicSeek() {
        //Go max acceleration towards object, look at it
        if (target == null) {
            //Debug.Log("Target Not Set");
            return;
        }

        //Move towards position
        Vector2 positionDifference = target.position - transform.position;
        linearAcceleration = maxAcceleration * positionDifference.normalized;

        //Lynch said to use angular acceleration to look towards object
        DynamicFace();

        //Visualisation
        Debug.DrawLine(transform.position, target.position, Color.red);
    }

    void DynamicFlee() {
        DynamicSeek();
        linearAcceleration *= -1;
        //TODO: Invert the Dynamic Face? It should look in the opposite direction than
        //DynamicSeek tells it to
        DynamicFace(1);
    }

    void DynamicPursueWithDynamicArrive() {
        if (target == null || target.GetComponent<Rigidbody2D>() == null) {
            return;
        }
        //Move towards where the target is moving
        //PURSUE
        Vector3 targetVelocity = target.GetComponent<Rigidbody2D>().velocity;
        Vector3 targetPosition = target.position + (targetVelocity*pursueDistance);
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

        //Face towards it
        DynamicFace();

        //Visualization
        Debug.DrawLine(transform.position, targetPosition, Color.blue);
    }

    void DynamicEvade() {
        DynamicFlee();
    }

    void DynamicAlign() {
        DynamicFace();
    }

    void DynamicFace(int mode = 0) {
        //Look towards target
        //TODO: Look towards the player, but do it in a smoothed motion

        if (target == null)
        {
            return;
        }

        float xDif = target.position.x - transform.position.x;
        float yDif = target.position.y - transform.position.y;

        float hyp = Mathf.Sqrt((xDif * xDif) + (yDif * yDif));

        float targetAngle;
        float currentAngle = transform.localEulerAngles.z;

        targetAngle = Mathf.Acos(yDif / hyp);
        targetAngle = Mathf.Rad2Deg * targetAngle;



        float currentAngVel = GetComponent<Rigidbody2D>().angularVelocity;

        if(target.position.x > transform.position.x)
        {
            targetAngle = targetAngle * -1;
        }

        if(currentAngle > 180)
        {
            currentAngle = -1 * (360 - currentAngle);
        }

        //Debug.Log("target angle is " + targetAngle);
        //Debug.Log("current angle is " + currentAngle);

        if(mode == 1)
        {
            targetAngle = targetAngle - 180;
        }

        if (Mathf.Abs(currentAngle - targetAngle) < turnSatiationDist)
        {
            GetComponent<Rigidbody2D>().angularVelocity = 0;
        }
        else
        {
            if(currentAngle < targetAngle)
            {
                GetComponent<Rigidbody2D>().angularVelocity = currentAngVel + 15;
                if (GetComponent<Rigidbody2D>().angularVelocity > turnMaxSpeed)
                {
                    GetComponent<Rigidbody2D>().angularVelocity =  turnMaxSpeed;
                }
            }
            else
            {
                GetComponent<Rigidbody2D>().angularVelocity = currentAngVel - 15;
                if(GetComponent<Rigidbody2D>().angularVelocity < turnMaxSpeed)
                {
                    GetComponent<Rigidbody2D>().angularVelocity = -1 * turnMaxSpeed;
                }
            }


        }

    }

    void DynamicWander() {
        //TODO: Wander in an obtuse way as described in his slide-set

        //Some sort of visualization too
        target = wanderTarget.transform;

        if(!wanderTarget.activeSelf)
        {
            wanderTarget.SetActive(true);
        }

        if (Time.time - wanderTick >= wanderShiftDelay)
        {
            wanderTarget.transform.position = wanderCenter.position;

            float angle = Random.value * Mathf.PI;
            float xRand = 1;
            float yRand = 1;

            if(Random.value < .5)
            {
                xRand = -1;
            }

            if (Random.value < .5)
            {
                yRand = -1;
            }

            wanderTarget.transform.Translate(new Vector3(Mathf.Sin(angle) * wanderTargRad * xRand, Mathf.Cos(angle) * wanderTargRad * yRand, 0));

            wanderTick = Time.time;
        }
        DynamicSeek();

        mc.ClampVelocity(maxVelocity / 3);
    }

    void SmarterWander()
    {

        target = wanderTarget.transform;

        if (!wanderTarget.activeSelf)
        {
            wanderTarget.SetActive(true);
        }

        if (Time.time - wanderTick >= wanderShiftDelay)
        {
            bool found = false;
            for(int whileTick = 0; whileTick < 100; whileTick++)
            {
                //Debug.Log(whileTick);
                wanderTarget.transform.position = wanderCenter.position;

                float angle = Random.Range(0f,1f) * Mathf.PI;
                float xRand = 1;
                float yRand = 1;

                if (Random.value < .5)
                {
                    xRand = -1;
                }

                if (Random.value < .5)
                {
                    yRand = -1;
                }

                wanderTarget.transform.Translate(new Vector3(Mathf.Sin(angle) * wanderTargRad * xRand, Mathf.Cos(angle) * wanderTargRad * yRand, 0));
                if (wanderTarget.transform.position.x < maxX && wanderTarget.transform.position.x > minX && wanderTarget.transform.position.y < maxY && wanderTarget.transform.position.y > minY)
                {
                    //Debug.Log("Test Succc");
                    wanderTick = Time.time;
                    found = true;
                    break;
                }
            }
            if(!found)
            {
                //Debug.Log("moving to default targ");
                wanderTarget.transform.position = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
                wanderTick = Time.time + .8f;
            }
        }
        DynamicSeek();

        mc.ClampVelocity(maxVelocity / 3);
    }

    void FollowPath() {
        if (target == null && curr.num != 0) {
            target = curr.GetComponentInParent<Transform>();
        }
        Transform me = gameObject.GetComponent<Transform>();
        float dist = Mathf.Pow((target.position.x - me.position.x), 2) + Mathf.Pow((target.position.y - me.position.y), 2);
        //Debug.Log("(" + target.position.x + ", " + target.position.y + ", " + target.position.z + ") " + dist);
        if (dist < 1) {
            curr = curr.next;
            if (curr != null) {
                target = curr.GetComponentInParent<Transform>();
            }
        }
        DynamicSeek();
    }

    void OnMouseDown() {
        CameraFollow.target = transform;
        ModeSelector.UpdateText();
    }


}
