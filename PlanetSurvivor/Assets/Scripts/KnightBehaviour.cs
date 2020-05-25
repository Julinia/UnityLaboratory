 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class KnightBehaviour : MonoBehaviour {

	private Animator animator;
	private GameObject planet;

	private bool attackWindowActive;
	private bool pullWindowActive;

	private float health = 100;
	public bool hasHerb;
	public bool hasCrystal;
	public bool hasPowder;

	private bool hasKit;
	private int hatchetLifes;
	private bool hasBomb;
	// Use this for initialization


	void Start () {
		animator = GetComponent<Animator>();
		planet = GameObject.Find("Planet");

		// constantly decrementing health with a given interval
		InvokeRepeating("DecrementHealth", 0, 0.8f);
	}

	public void DecrementHealth(){
		health--;
		if (health <= 0) {
			// gameover
			print("Survived time:" + Time.timeSinceLevelLoad);
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateInterface();
		PlayerControls();
		if(health > 100)
			{
			health = 100;
			}
		print(health);
	}

	// synchronize all script properties and flags with UI elements
	void UpdateInterface(){
		GameObject.Find("Slider").GetComponent<Slider>().value = health;
		GameObject.Find("Fill").GetComponent<Image>().color =
			Color.Lerp(Color.red, Color.green, health / 100f);
		foreach (GameObject herb in GameObject.FindGameObjectsWithTag("Herb")){
			herb.GetComponent<Image>().color = hasHerb ? Color.white : Color.black;
		}
		foreach (GameObject herb in GameObject.FindGameObjectsWithTag("Crystal")){
			herb.GetComponent<Image>().color = hasCrystal ? Color.white : Color.black;
		}
		foreach (GameObject powder in GameObject.FindGameObjectsWithTag("Powder")){
			powder.GetComponent<Image>().color = hasPowder ? Color.white : Color.black;
		}
		GameObject.Find("Hatchet").GetComponent<Image>().color = hatchetLifes > 0 ? Color.white : Color.black;
		GameObject.Find("Kit").GetComponent<Image>().color = hasKit ? Color.white : Color.black;
		GameObject.Find("Bomb").GetComponent<Image>().color = hasBomb ? Color.white : Color.black;
	}

	// key presses and actions
	void PlayerControls(){		
		if (Input.GetKey(KeyCode.A)) {
			Move(-1);
		}
		if (Input.GetKey(KeyCode.D)) {
			Move(1);
		}
		// setting animator properties to cause animation transitions!!
		if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) ){
			animator.SetBool("IsRunning", false);
		}
		if (Input.GetKeyDown(KeyCode.F)){
			animator.SetTrigger("Attack");
		}

		// crafting logic
		if (Input.GetKeyDown(KeyCode.E)){
			if (hatchetLifes > 0){
				UseHatchet();
			}else {
				TryCraftHatchet();
			}
		}
		if (Input.GetKeyDown(KeyCode.Q)){
			if (hasKit){
				UseKit();
			}else {
				TryCraftKit();
			}
		}
		if (Input.GetKeyDown(KeyCode.R)){
			animator.SetTrigger("Pull");
		}
		if (Input.GetKeyDown(KeyCode.X)){
			if (hasBomb){
				UseBomb();
			}else {
				TryCraftBomb();
			}
		}
	}

	public void UseBomb(){
        ParticleSystem exp = GetComponent<ParticleSystem>();
        exp.Play();
        Destroy(gameObject, exp.main.duration);
		hasBomb = false;
	}
	
	public void TryCraftBomb(){
			if (hasCrystal && hasHerb && hasPowder) {
			hasBomb = true;
			hasHerb = false;
			hasCrystal = false;
			hasPowder = false;

		}
	}

	public void UseKit(){
		health += 20;
		hasKit = false;
	}

	public void TryCraftKit(){
		if (hasHerb) {
			hasKit = true;
			hasHerb = false;
		}
	}

	public void UseHatchet(){
		animator.SetTrigger("Smash");
		if (hatchetLifes >= 0) hatchetLifes--;
	}
	public void TryCraftHatchet(){
		if (hasCrystal && hasHerb) {
			hatchetLifes = 10;
			hasCrystal = false;
			hasHerb = false;
		}
	}

	// rotates the player and launches running animation, localScale.x mirrors the sprite in the right direction
	void Move(int leftRight){
			animator.SetBool("IsRunning", true);
			gameObject.transform.parent.localScale = new Vector3(leftRight * 1, 1, 1);
			planet.transform.Rotate(new Vector3(0, 0, leftRight * 1.2f));
	}

	// animation windows / animtion marks, used from animation events to tell when the actual ATTACK/PULL frames are playing
	private void SetAttackWindowValue(int value){
		attackWindowActive = value == 1;
	}

	private void SetPullWindowValue(int value){
		pullWindowActive = value == 1;
	}

	// used from animation event, the effect of the ground shake -- shake all opposide side enemies out of the planet
	private void ShakeGround(){
		planet.GetComponent<Animator>().Play("GroundShake", 0, 0);
		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")){
			Vector3 outOfPlanetDirection = (enemy.transform.position - planet.transform.position).normalized;
			if (outOfPlanetDirection.y <= -0.35f){
				enemy.transform.position += outOfPlanetDirection * 1.0f;
			}
		}

	}


	// unity sends this signal when COLLIDERS ARE OVERLAPPING!!!
	// we catch it and apply logic depending on the incoming object
	private void OnTriggerStay2D(Collider2D other) {
		if (other.tag == "Enemy" && pullWindowActive) {
			other.GetComponent<EnemyBehaviour>().PullOut();
		}
		if (other.tag == "HerbRes" && pullWindowActive) {
			hasHerb = true;
			Destroy(other.gameObject);
		}
		// if (other.tag == "PowderRes" && pullWindowActive) {
		// 	hasPowder = true;
		// 	Destroy(other.gameObject);
		// }
		if (other.tag == "CrystalRes" && attackWindowActive) { 
			Animator otherAnimator = other.GetComponent<Animator>();
			otherAnimator.Play("Blink");
			if (other.GetComponent<Crystal>().hitsToDestroy <= 0){
				hasCrystal = true;
				Destroy(other.gameObject);
			}
		}
	}


	
}
