using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour {

    public PathBuilder pb;
    public GameObject Line;

    private int numPaths = 0;
    private List<LNode> prePaths = new List<LNode>();
    private bool started = false;

    private void Start() {
        prePaths = pb.GetPaths();
    }

    void Update() {
        if (pb.draw && !started) {
            started = StartUp();
        }
    }

    private bool StartUp() {
        if (numPaths == 0) {
            numPaths = pb.getNumPaths();
            return false;
        } else {
            for (int i = 0; i < numPaths; i++) {
                GameObject tempGO = Instantiate(Line);
                LineRenderer curr = tempGO.GetComponent<LineRenderer>();
                curr.positionCount = prePaths[i].GetTailLength() + 1;
                LNode temp = prePaths[i];
                while (true) {
                    //Debug.Log(temp.num + " " + temp.GetComponentInParent<Transform>().position.x + " " + temp.GetComponentInParent<Transform>().position.y);
                    if (temp.next == null) {
                        temp = prePaths[i];
                        break;
                    } else {
                        temp = temp.next;
                    }
                }
                int count = 0;
                do {
                    //Debug.Log("i: " + i + " count: " + count);
                    curr.SetPosition(count, temp.GetComponentInParent<Transform>().position);
                    count++;
                    temp = temp.next;
                } while (temp != null);
            }

        }
        return true;
    }

}
