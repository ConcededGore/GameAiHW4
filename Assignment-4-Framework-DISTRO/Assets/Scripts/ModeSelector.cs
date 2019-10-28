using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModeSelector : MonoBehaviour {
    public TextMeshProUGUI modeText;
    public static TextMeshProUGUI _modeText;
    
    void Start() {
        _modeText = modeText;
    }

    void Update() {
        if (CameraFollow.target == null) {
            modeText.text = "No Player Selected";
            return;
        }

        int newMode = -1;
        bool changeSmarter = false;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            newMode = Modes.DYNAMICSEEK;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            newMode = Modes.DYNAMICFLEE;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            newMode = Modes.DYNAMICPURSUREARRIVE;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            newMode = Modes.DYNAMICEVADE;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            newMode = Modes.DYNAMICALIGN;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            newMode = Modes.DYNAMICFACE;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            newMode = Modes.DYNAMICWANDER;
        } else if (Input.GetKeyDown(KeyCode.Alpha7)) {
            newMode = Modes.DYNAMICWANDER;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            newMode = Modes.SMARTERWANDER;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) 
        {
            changeSmarter = true;
        }

        if (newMode != -1) {
            CameraFollow.target.GetComponent<PlayerController>().mode = newMode;
            UpdateText();
        }
        if (changeSmarter) {
            CameraFollow.target.GetComponent<PlayerController>().smartAvoid = !CameraFollow.target.GetComponent<PlayerController>().smartAvoid;
            UpdateText();
        }

    }

    public static void UpdateText() {
        if (CameraFollow.target == null) return;
        if (CameraFollow.target.GetComponent<PlayerController>().smartAvoid) {
            _modeText.text = "Smart(er) ";
        } else {
            _modeText.text = "";
        }
        switch (CameraFollow.target.GetComponent<PlayerController>().mode) {
            case Modes.DYNAMICSEEK:
                _modeText.text += "Dynamic Seek";
                break;
            case Modes.DYNAMICFLEE:
                _modeText.text += "Dynamic Flee";
                break;
            case Modes.DYNAMICPURSUREARRIVE:
                _modeText.text += "Dynamic Pursue+Arrive";
                break;
            case Modes.DYNAMICEVADE:
                _modeText.text += "Dynamic Evade";
                break;
            case Modes.DYNAMICALIGN:
                _modeText.text += "Dynamic Align";
                break;
            case Modes.DYNAMICFACE:
                _modeText.text += "Dynamic Face";
                break;
            case Modes.DYNAMICWANDER:
                _modeText.text += "Dynamic Wander";
                break;
            case Modes.SMARTERWANDER:
                _modeText.text += "Smarter Dynamic Wander";
                break;
            default:
                _modeText.text += "Unknown Mode";
                break;
        }
    }
}
