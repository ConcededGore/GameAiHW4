using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMove : MonoBehaviour {

    public float moveSpeed = 5.0f;
    public bool horizontal;

    private const float horizontalBorder = 12.0f, verticalBorder = 7.0f;

    private Rigidbody2D rb2d;

    private bool downright = false;

    void Start() {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (horizontal) {
            if (downright) {
                rb2d.AddForce(new Vector3(moveSpeed * Time.deltaTime, 0, 0));
                if (transform.position.x > horizontalBorder) downright = false;
            } else {
                rb2d.AddForce(new Vector3(-moveSpeed * Time.deltaTime, 0, 0));
                if (transform.position.x < -horizontalBorder) downright = true;
            }
        } else {
            if (downright) {
                rb2d.AddForce(new Vector3(0, -moveSpeed * Time.deltaTime, 0));
                if (transform.position.y < -verticalBorder) downright = false;
            } else {
                rb2d.AddForce(new Vector3(0, moveSpeed * Time.deltaTime, 0));
                if (transform.position.y > verticalBorder) downright = true;
            }
        }
    }

    void OnMouseDown() {
        if (CameraFollow.target != null) {
            CameraFollow.target.GetComponent<PlayerController>().target = transform;
        }
    }
}
