using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathBuilder : MonoBehaviour {

    public bool draw = true;
    
    private List<LNode> paths = new List<LNode>();
    private int numPaths = 0;

    private GameObject[] allNodes;

    // Start is called before the first frame update
    void Start() {
        allNodes = GameObject.FindGameObjectsWithTag("Node");
        numPaths = getNumPaths();

        for (int i = 0; i < numPaths; i++) {
            paths.Add(allNodes[0].GetComponent<LNode>());
        }

        for (int i = 0; i < allNodes.Length; i++) {
            GameObject curr = allNodes[i];
            if (curr.GetComponent<LNode>().num == 0) {
                paths[curr.GetComponent<LNode>().Path] = curr.GetComponent<LNode>();
            }
        }
        for (int i = 0; i < allNodes.Length; i++) {
            GameObject curr = allNodes[i];
            if (curr.GetComponent<LNode>().num != 0) {
                paths[curr.GetComponent<LNode>().Path].AddNode(allNodes[i].GetComponent<LNode>());
            }
        }

    }

    public int getNumPaths() {
        if (numPaths != 0) {
            return numPaths;
        } else if (allNodes.Length == 0) {
            return 0;
        }
        int retval = 0;
        int temp = 0;
        for (int i = 0; i < allNodes.Length; i++) {
            temp = allNodes[i].GetComponent<LNode>().Path;
            if (temp > retval) {
                retval = temp;
            }
        }
        return retval + 1;
    }

    public List<LNode> GetPaths() {
        return paths;
    }

}
