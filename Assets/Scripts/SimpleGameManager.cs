using UnityEngine;
using System.Collections;
//SIMPLE LANE CHANGING GAME EXAMPLE
//Matt Parker and Bennett Foddy
//NYU Game Center, 2016-17
public class SimpleGameManager : MonoBehaviour {

	//public properties: these are typically set in the Inspector, and are accessible by other scripts
	public TextMesh textMesh;
	[Header("Store the prefab to create the car")] //I'm just using these headers to put comments in the inspector
	public GameObject carPrefab;	//Reference to the prefab we'll use to make the player car
	public GameObject enemyPrefab;	//Reference to the prefab we'll use to make the enemy car
	public Vector3 playerStartPoint;	//start position for the player
	public Vector3[] spawnPositions;	//spawn positions for enemy cars
	public Transform backgroundContainer;	//backgrounds gameObjects
	public GameObject player;	//player gameObjects

	//private properties: these are set in code, like P5.Play's global variables,
	//and cannot be accessed by other scripts

	Camera mainCamera;	//this variable will hold a reference to the Camera component on the main Camera object

	float timer = 0;	//track how much time has ellapsed

	//Game states of the game
	const int MODE_START = 0;
	const int MODE_GAME = 1;
	const int MODE_CRASH = 2;

	int gameMode; //variable to track current gameMode

	// Use this for initialization
	void Start () {
        //GameObject playerPrefab = GameObject.Find("PlayerPrefab");
        //LaneChangeCarControl LaneChangeCarControl = playerPrefab.GetComponent<LaneChangeCarControl>();
        gameMode = MODE_START; //start the game in start mode
		mainCamera = Camera.main; //get a reference to the main camera in the scene
		ResetGame(); //Call the ResetGame to setup the scene at the beginning

	}
	
	// Update is called once per frame
	void Update () {
		if(gameMode == MODE_START){	//Are we waiting to start?
			if(Input.GetMouseButton(0)){ //if the mouse button was pressed
				gameMode = MODE_GAME; //change the gameMode to play
				player.GetComponent<LaneChangeCarControl>().moving = true; //let the player know to moving
				InvokeRepeating("SpawnEnemyCar", 3f, 2f); //start spawning enemies, starting in 3 seconds and then after every 2 seconds
			}
		} else if(gameMode == MODE_GAME){ //Are we playing the game?
			timer += Time.deltaTime; //add to our timer
			textMesh.text = "Time: \n" + (int)timer; //update the display on the timer
			KeepBackgroundsOnScreen();
		} else if(gameMode == MODE_CRASH){ //Did we crash?
			textMesh.text = "Final\nScore: \n" + (int)timer; //if we crashed, display the final score
		}
	}

	void KeepBackgroundsOnScreen(){

		foreach(Transform background in backgroundContainer){ //go through all the backgroundContainer's child transforms
			//if the camera is too far from the background sprite to see it, then it should jump up to the top of the stack
			SpriteRenderer backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>(); //get a reference to the SpriteRenderer component
			float spriteHeight = backgroundSpriteRenderer.bounds.size.y; //the height of the sprite in world units
			float distanceFromCamera = mainCamera.transform.position.y - background.position.y;
			float cameraHeight = mainCamera.orthographicSize; //vertical height of the orthographic camera in world units
			if (distanceFromCamera > cameraHeight * 2f) {
				Vector3 newPosition = background.position; //we can't modify this directly so we store it in a variable
				newPosition.y += spriteHeight * backgroundContainer.childCount; //since we have 3 children in backgroundContainer, this will move up 3*spriteHeight

				//now we can copy the modified value back to the transform
				background.position = newPosition;
			}
		}

	}

	void SpawnEnemyCar(){
		int i = Random.Range(0, spawnPositions.Length); //a number between 0 and the number of spawn positions (exclusive)

		Vector3 spawnPosition = spawnPositions[i]; //get the actual position we stored in the inspector

		GameObject car =  Instantiate (enemyPrefab, playerStartPoint, Quaternion.identity) as GameObject;

		car.transform.position = new Vector3(
			spawnPosition.x,
			spawnPosition.y + player.transform.position.y,
			spawnPosition.z);
	}
		
	//This custom function is triggered by the CarControl script, using SendMessage. 
	//It gets called after the player hits a wall, and it resets the game after a delay
	public void Crash(){
		CancelInvoke(); //Cancel the invoke that is spawning enemies
		//'Invoke' will cause the 'ResetGame' function to be called after 1 second.
		Invoke ("ResetGame", 1.0f); 
		gameMode = MODE_CRASH; //change the gameMode to Crash!
	}

	//Reset the game after a delay after the crash
	void ResetGame(){
		timer = 0; //reset the timer

        GameObject playerClone = Instantiate(player) as GameObject;
        Destroy(playerClone);

       // Destroy(player); //remove the old player car
	
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); //find all the cars in the scene using their tag

		foreach(GameObject enemy in enemies){ //loop through the enemies
            GameObject enemyClone = Instantiate(enemy) as GameObject;
            Destroy(enemy); //destroy the enemy sprite
		}

		//reset the positions of the background sprites, spacing them out so they sit on top of each other.
		int backgroundNum = 0;
		foreach (Transform background in backgroundContainer) { 
			SpriteRenderer backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>(); //get a reference to the SpriteRenderer component
			float spriteHeight = backgroundSpriteRenderer.bounds.size.y; //the height of the sprite in world units
			background.position = new Vector3 (0, spriteHeight * backgroundNum ,0); 
			backgroundNum += 1;
		}

		player = Instantiate (carPrefab, playerStartPoint, Quaternion.identity) as GameObject; //create a new player object

		mainCamera.GetComponent<FollowObject>().followObject = player.transform; //have the camera follow the player

		gameMode = MODE_START; //change the gameMode 
		textMesh.text = "Click\nTo\nStart"; //change the text to start instructions. '\n' signifies a new line.
	}
}
