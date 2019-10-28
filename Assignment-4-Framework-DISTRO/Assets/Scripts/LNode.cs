using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LNode : MonoBehaviour {
    public LNode next = null;
    public bool head = false;
    public int num = 0;
    public int Path = 0;
    public Transform t;

    private Vector2 pos;
    private int tailLength = 0;

    private void Start() {
        pos = this.GetComponentInParent<Transform>().position;
    }

    private void OnDrawGizmos() {
        if (next != null) {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(pos, next.pos);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, .25f);
    }

    public void AddNode(LNode n) {
        if (next == null) {
            next = n;
        } else if (n.num < next.num) {
            n.next = next;
            next = n;
        } else {
            next.AddNode(n);
        }
        tailLength++;
    }

    public void DeleteNode(int target) {
        deleteNodeRecursive(target, 0);
    }

    private void deleteNodeRecursive(int n, int curr) {
        if (n == curr) {
            next = next.next;
        } else {
            deleteNodeRecursive(n, curr + 1);
        }
    }

    public LNode GetNode(int n) {
        if (num == n) {
            return this;
        }
        return next.GetNode(n);
    }

    public int GetTailLength() {
        return tailLength;
    }

}
