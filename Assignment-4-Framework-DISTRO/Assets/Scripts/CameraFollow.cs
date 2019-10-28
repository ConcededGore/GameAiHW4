using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    //This is set from indi
    public static Transform target = null;
    public float dampTime = 0.5f;

    private float startingZ;

    private Vector3 velocity;

    void Start() {
        startingZ = transform.position.z;
    }

    //Follow the target
    void Update() {
        if (target == null) {
            return;
        }
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, dampTime);
    }
}
