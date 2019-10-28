using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour {

    public float drag = 1;

    private Rigidbody rb;

    private Vector3 acceleration = new Vector3(0,0,0);
    private Vector3 lastVelocity = new Vector3(0,0,0);

    void Start() {
        rb = GetComponent<Rigidbody>();
        if (GetComponent<SwarmHead>() != null) {
            rb.detectCollisions = false;
        }
    }

    public void Move(Vector3 linearAcceleration, float angularAcceleration) {
        acceleration = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = rb.velocity;
        rb.AddForce(linearAcceleration);
    }

    public void ClampVelocity(float maxVelocity) {
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }

}
