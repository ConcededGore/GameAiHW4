using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour {

    public int numTrees = 0;
    public float radius = 0.5f;
    public GameObject tree;
    public float boundryX = 14.5f;
    public float boundryY = 9;

    // Start is called before the first frame update
    void Start() {
        GenerateTrees();
    }

    // Returns the distance between vectors a and b
    private float getDist(Vector3 a, Vector3 b) {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    // Returns a vector populated with boundryX +1 if posIn clips a wall, or
    // Returns the position of the player/target/tree its clipping, or
    // Returns boundryx +2 if spawn is valid
    private Vector3 CheckSpawnValidity(Vector2 posIn) {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
        GameObject[] trees = GameObject.FindGameObjectsWithTag("tree");
        Transform t;
                
        if (posIn.x + radius > boundryX || -posIn.x - radius < -boundryX) {
            return new Vector3(boundryX + 1, boundryX + 1, 0);
        }
        if (posIn.y + radius > boundryY || -posIn.y - radius < -boundryY) {
            return new Vector3(boundryX + 1, boundryX + 1, 0);
        }

        // Checks if posIn is in a player
        for (int i = 0; i < players.Length; i++) {
            t = players[i].GetComponent<Transform>();
            if (getDist(new Vector3(posIn.x, posIn.y, 0), t.position) < radius + t.localScale.x) {
                return new Vector3(t.position.x, t.position.y, t.localScale.x);
            }
        }

        // Checks if posIn is in a target
        for (int i = 0; i < targets.Length; i++) {
            t = targets[i].GetComponent<Transform>();
            if (getDist(new Vector3(posIn.x, posIn.y, 0), t.position) < radius + t.localScale.x) {
                return new Vector3(t.position.x, t.position.y, t.localScale.x);
            }
        }

        // Checks if posIn is in another tree
        for (int i = 0; i < trees.Length; i++) {
            t = trees[i].GetComponent<Transform>();
            if (getDist(new Vector3(posIn.x, posIn.y, 0), t.position) < radius + t.localScale.x) {
                return new Vector3(t.position.x, t.position.y, t.localScale.x);
            }
        }

        return new Vector3(boundryX + 2, boundryX + 2, 0);
    }

    // Generates a valid position to spawn the tree
    private Vector2 genPos() {
        Vector3 retval = new Vector3(Random.Range(-boundryX + (radius / 2), boundryX - (radius / 2)), Random.Range(-boundryY + (radius / 2), boundryY - (radius / 2)), 0);
        Vector2 offset;
        Vector3 valid = CheckSpawnValidity(retval);
        float validRadius = 0;
        int iterations = 0;

        while (true) {
            // This means that the tree is either trying to spawn in a wall or it has somehow spawned in an object 10 times in a row
            // and I'm not even sure if that first one is possible in the current set up and the second is astronomically unlikely,
            // I'm just hiding the trees that can't find a spawn because I don't want to deal with editing the array
            if (valid.x == boundryX + 1 || iterations >= 10) {
                return new Vector2(boundryX + 10, boundryY + 10);
            }
            
            if (valid.x == boundryX + 2) { // Spawn is valid
                return new Vector2(retval.x, retval.y);
            }
            else { // Spawn is invalid
                validRadius = valid.z;
                valid.z = 0;
                offset = valid - retval;
                retval += valid;
                retval.Normalize();
                retval *= (radius + validRadius);
                iterations++;
                valid = CheckSpawnValidity(retval);
            }
        }
    }

    private void GenerateTrees() {
        for (int i = 0; i < numTrees; i++) {
            Instantiate(tree, genPos(), Quaternion.identity);
        }
    }

    public void RegenerateTrees() {
        GameObject[] trees = GameObject.FindGameObjectsWithTag("tree");
        Transform temp;
        Vector2 tempVec;

        for (int i = 0; i < numTrees; i++) {
            temp = trees[i].GetComponent<Transform>();
            tempVec = genPos();
            temp.position = new Vector3(tempVec.x, tempVec.y, 0);
        }
    }

}
