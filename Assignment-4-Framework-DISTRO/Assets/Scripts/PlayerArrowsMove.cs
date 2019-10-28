using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArrowsMove : MonoBehaviour
{

    private Rigidbody2D rb;
    public float speedForce;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.AddForce(input * speedForce);
    }
}
