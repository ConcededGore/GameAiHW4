using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArrowsMove : MonoBehaviour
{

    private Rigidbody rb;
    public float speedForce;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        rb.AddForce(input * speedForce);
    }
}
