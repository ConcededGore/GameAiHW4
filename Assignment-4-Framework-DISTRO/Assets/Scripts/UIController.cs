using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("q"))
        {
            SceneManager.LoadScene("UI scene");
        }
    }

    public void LoadOne()
    {
        SceneManager.LoadScene("Field 1");
    }

    public void LoadTwo()
    {
        SceneManager.LoadScene("Field");
    }

    public void LoadThree()
    {
        SceneManager.LoadScene("Forest");
    }
}
