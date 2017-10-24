using UnityEngine;
using System.Collections;

public class LaneChangeCarControl : MonoBehaviour
{
    //private properties (like p5.play's global variables) - inaccessible by other scripts)
    Rigidbody2D rb;
    AudioSource audioSource;
    GameObject gameManager;
    bool crashed;

    //public properties - these are typically set in the inspector and are accessible by other scripts
    [Header("Plays when the car crashes")] //I'm just using this to put comments in the inspector
    public AudioClip crashSound;
    [Header("Tuning variable for car turning")]
    public float horizontalSpeed;
    [Header("Tuning variable for braking")]
    public float brakeAmount;

    public float forwardSpeed;
    public float carAcceleration;

    public float laneWidth;

    public bool moving;
    public int score;

    //private variables
    int carDir;
    float angle;
    float startX;

    // Use this for initialization
    void Start()
    {
        //we need a reference to the game manager so we can tell it if the car crashed, so we search for it by name
        //Beware: Find() is slow! don't do it every frame.
        gameManager = GameObject.Find("GameManager");
        rb = GetComponent<Rigidbody2D>(); //we need an easy way to access the Rigidbody2D component on this GameObject
        audioSource = GetComponent<AudioSource>(); //and an easy way to access the AudioSource component
        crashed = false;
        moving = false;
        startX = transform.position.x; //keep track of the start position

        //having declared these variables above, I want to set their starting values
        carDir = 0;
        angle = 0;
    }

    // FixedUpdate is called before the physics system updates
    void FixedUpdate()
    {

        if (moving)
        {
            if (crashed == false)
            { //only accept input if the car hasn't crashed

                //create a variable for where we want to move the car, starting with its current position
                Vector2 newPosition = rb.position;

                //increase the speed by acceleration * the time elapsed
                forwardSpeed += Time.fixedDeltaTime * carAcceleration;  //Time.fixedDeltaTime is the number of seconds that will pass before the next fixedupdate

                //set the y component of the new velocity from forwardSpeed
                //using the speed * the time elapsed
                newPosition.y += forwardSpeed * Time.fixedDeltaTime;

                //we'll need to know how far the car has moved (if at all) from its previous lane
                float distanceFromLane = Mathf.Abs(transform.position.x - startX);

                if (carDir == 0)
                { //the car is not currently changing lanes
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    { //if they *just pressed* the left key
                        carDir = -1; //set the car direction to negative
                        startX = rb.position.x; //set the start position to the current position, before the turn
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    { //if they *just pressed* the right key
                        carDir = 1; //change the car dir to positive
                        startX = rb.position.x; //set the start position to the current position, before the turn
                    }
                }
                else
                { //if the car is currently turning
                    if (distanceFromLane >= laneWidth)
                    { //if they have reached the lane they are headed towards
                        carDir = 0; //change direction to 0
                    }
                }

                //move to the left or the right (or not at all) depending on carDir
                newPosition.x += carDir * horizontalSpeed * Time.fixedDeltaTime;

                //now that we've come up with the new position we want for the car, we tell the 
                //Rigidbody2D to go there. Note: we could just set transform.position to teleport the GameObject there,
                //but that wouldn't calculate collisions on the way. It's better to move physics objects using MovePosition()
                rb.MovePosition(newPosition);

                //change the angle of the car using the horizontal velocity
                //note: you can set a sprite's rotation directly with transform.localEulerAngles, but if you have a 
                //physics body on it, it's safer to call MoveRotation() on the body
                float newAngle = (laneWidth - distanceFromLane) * carDir * -15.0f;
                rb.MoveRotation(newAngle);

                //Using the forward speed and input axis to set the volume to full and increase the speed with the pitch
                audioSource.volume = 1;
                audioSource.pitch += forwardSpeed / 10000f;

            }
        }
    }

    //OnCollisionEnter2D() is called by the Unity engine under the following conditions:
    //1) this object has a Collider2D and a Rigidbody2D
    //2) it touched another object with (at least) a Collider2D
    //The system passes it a special Collision2D object with information about the collision 
    //(including which object we collided with)
    void OnCollisionEnter2D(Collision2D thisCollision)
    {
        if (thisCollision.collider.tag == "Enemy")
        {
            Debug.Log("hit " + thisCollision.collider.name);
            Destroy(thisCollision.gameObject);
            score ++;
        }

        if (thisCollision.collider.tag == "wall")
        {
            crashed = true;
        }

        if (crashed == true){
            return;
            GetComponent<SpriteRenderer>().color = Color.red; //Color.red is a nice shorthand for 'new Color(1.0f,0.0f,0.0f,1.0f)'
            audioSource.Stop(); //stop the engine sound
            audioSource.volume = 1.0f;
            audioSource.pitch = 1.0f; //When .pitch is 1.0 it plays the sound at normal pitch
            audioSource.PlayOneShot(crashSound); //PlayOneShot lets you play an AudioClip that isn't currently set as the 'clip' property on this AudioSource
            gameManager.SendMessage("Crash"); //calls the "Crash" function on *all* components of a gameObject
            crashed = true; //set this to make sure this function doesn't get called again*/
        }
    }
}