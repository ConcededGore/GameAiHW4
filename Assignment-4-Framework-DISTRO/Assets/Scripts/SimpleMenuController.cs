using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleMenuController : MonoBehaviour
{
    public void GoToScene() {
        //0 is menu
        //1 is game
        SceneManager.LoadScene(1);
    }
}
