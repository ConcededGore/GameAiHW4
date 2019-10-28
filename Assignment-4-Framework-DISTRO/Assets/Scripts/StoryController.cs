using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryController : MonoBehaviour {

    public TextMeshProUGUI title, text, timerText;

    public Transform randomBound1, randomBound2;
    public Transform spawn, grandmasHouse;
    public PlayerController hunter, wolf, red;
    public Transform house;

    private int currentPhase = 0;
    private int timer = 0;

    /*Default Player Movement Values (for reference)
     * Max Velocity - 5
     * Linear Acceleration - 300
     * Angular Acceleration - 0.1
     * Slow Radius - 3
     * Pursue Distance - 0.3
     * Turning Stuff - 5, 15, 60
     * Wandering Stuff - 1.75, 0.4
     * Smart Avoid - On
     * Movement- 16, 3, 400
     */
    
    /* **In the context of this scene, despawning means just disabling, no prefabs needed
     * **All characters have the smarter mode enabled
     * 
     * 1.- Hunter appears at random pos, camera follows, smarter wander
     * --Wait x Seconds--
     * 2.- Wolf appears at random pos, camera follows, smarter wander
     * --Wait x Seconds--
     * 3.- Hunter Pursues Wolf, Wolf runs away
     * --Wait until collision, then despawn them both
     * 4.- Red appears and follows path to grandmas house (make sure path is visible through something other than gizmos, draw a png path overlay or use the linerenderer)
     * **Path must be zigzagged a good amount
     * **Red is moving at like 1/2 speed everyone else
     * --Wait x Seconds--
     * 5.- Wolf appears and pursues red
     * --Wait until collision, stop them both--
     * 6.- Show little dialogue on UI, have them both stopped still
     * --Wait x Seconds--
     * 7.- Red Continues Path Finding, the wolf goes directly to grandmas house, slows on arrival
     * **wolf is movin at .7 speed
     * --Wait until wolf collides with house
     * 8.- Hunter Appears at random position, also goes to house, slows on arrival
     * **hunter moving at full speed
     * 9.- Red hears screams, and follows them to the house, where she is also eaten
     * --Wait 10 Seconds--
     * Go back to menu
     */

    void Start() {
        hunter.smartAvoid = true;
        hunter.gameObject.SetActive(false);
        red.smartAvoid = true;
        red.gameObject.SetActive(false);
        wolf.smartAvoid = true;
        wolf.gameObject.SetActive(false);
        currentPhase = 1;
        ChangePhase(1);
        StartCoroutine(TimerController());
    }

    Vector3 GenerateRandomPos() {
        float randomX = randomBound2.position.x - randomBound1.position.x,
                    randomY = randomBound2.position.y - randomBound1.position.y;
        randomX = randomBound1.position.x + (Random.Range(0.0f, 1.0f) * randomX);
        randomY = randomBound1.position.y + (Random.Range(0.0f, 1.0f) * randomY);
        return new Vector3(randomX, randomY, 0);
    }

    public void HunterCollidesWithWolf() {
        timer = 0;
    }

    public void ChangePhase(int num) {
        switch (num) {
            case 1:
                hunter.transform.position = GenerateRandomPos();
                hunter.gameObject.SetActive(true);
                hunter.mode = Modes.SMARTERWANDER;
                timer = 10;
                CameraFollow.target = hunter.transform;
                title.text = "Hunter Appears";
                text.text = "The hunter silently moves through the forest looking for the wolf...";
                break;
            case 2:
                wolf.transform.position = GenerateRandomPos();
                wolf.gameObject.SetActive(true);
                wolf.mode = Modes.SMARTERWANDER;
                timer = 10;
                CameraFollow.target = wolf.transform;
                title.text = "Wolf Appears";
                text.text = "The wolf creeps slowly through the forest, attempting to steer clear of wherever the hunter may be...";
                break;
            case 3:
                wolf.target = hunter.transform;
                hunter.target = wolf.transform;
                wolf.mode = Modes.DYNAMICEVADE;
                hunter.mode = Modes.DYNAMICSEEK;
                timer = -1;
                CameraFollow.target = hunter.transform;
                title.text = "The Chase";
                text.text = "The hunter sees the wolf and gives chase!";
                StartCoroutine(CollisionWatch(hunter.transform, wolf.transform));
                break;
            case 4:
                wolf.gameObject.SetActive(false);
                hunter.gameObject.SetActive(false);
                timer = 0;
                break;
            case 5:
                red.gameObject.SetActive(true);
                red.mode = Modes.PATHFOLLOW;
                wolf.transform.position = GenerateRandomPos();
                wolf.gameObject.SetActive(true);
                wolf.target = red.transform;
                wolf.mode = Modes.DYNAMICPURSUREARRIVE;
                CameraFollow.target = red.transform;
                timer = -1;
                title.text = "The Pursuit";
                text.text = "The wolf smells Red and chases her down!";
                StartCoroutine(CollisionWatch(red.transform, wolf.transform));
                break;
            case 6:
                wolf.target = null;
                red.target = null;
                red.mode = Modes.DYNAMICSEEK;
                title.text = "The Conversation";
                text.text = "The Wolf: Where are you going little girl?\n Red: Just my helpess grandma's house!\n The Wolf: Interesting... okay, be on your way!";
                timer = 10;
                break;
            case 7:
                red.mode = Modes.PATHFOLLOW;
                red.maxVelocity *= .8f;
                title.text = "The trap";
                text.text = "";
                timer = -1;
                wolf.mode = Modes.DYNAMICSEEK;
                wolf.target = house;
                wolf.maxVelocity = wolf.maxVelocity * .7f;
                StartCoroutine(CollisionWatch(wolf.transform, house));
                break;
            case 8:
                //Red should still be path following
                wolf.gameObject.SetActive(false);
                hunter.gameObject.SetActive(true);
                hunter.transform.position = GenerateRandomPos();
                hunter.mode = Modes.DYNAMICSEEK;
                hunter.target = house;
                StartCoroutine(CollisionWatch(hunter.transform, house));
                break;
            case 9:
                hunter.gameObject.SetActive(false);
                red.target = house;
                red.mode = Modes.DYNAMICSEEK;
                StartCoroutine(CollisionWatch(red.transform, house));
                break;
            case 10:
                red.gameObject.SetActive(false);
                title.text = "An Unfortunate End";
                text.text = "And then Red walked into the house where the wolf sat, still hungry after his two meals, and was eaten.";
                timer = 10;
                break;
            default:
                Debug.Log("Unkown phase");
                SceneManager.LoadScene(0);
                break;
        }
    }

    private IEnumerator TimerController() {
        while (true) {
            yield return new WaitForSeconds(1);
            if (timer == -1) {
                timerText.text = "Waiting...";
            } else if (timer == 0) {
                currentPhase++;
                ChangePhase(currentPhase);
            } else {
                timer--;
                timerText.text = "" + timer;
            }
        }
    }

    private IEnumerator CollisionWatch(Transform obj1, Transform obj2)
    {
        bool found = false;
        while(!found)
        {
            yield return null;
            //print(Vector3.Distance(obj1.position, obj2.position));
            if (Vector3.Distance(obj1.position, obj2.position) <= 1.3f)
            {
                //print(Vector3.Distance(obj1.position, obj2.position));
                currentPhase++;
                ChangePhase(currentPhase);
                found = true;
            }
            
        }
    }
}
