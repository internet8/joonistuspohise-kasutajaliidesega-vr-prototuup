using UnityEngine;
using System.Collections;

public class Lamp_ctrl : MonoBehaviour {

	private Animator anim;
	private CharacterController controller;
	private bool battle_state;
	public float speed = 6.0f;
	public float runSpeed = 1.0f;
	public float turnSpeed = 60.0f;
	public float gravity = 20.0f;
	public int goblin_class; // 1-warrior 2-shamon 3-archer
	private Vector3 moveDirection = Vector3.zero;



	// Use this for initialization
	void Start () {
		
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController> ();
		
		
	}


	// Update is called once per frame
	void Update () {
		
		
		if (Input.GetKey("2")) //idle ->>walk 
		{
			anim.SetInteger("battle", 1);
			battle_state = true;
			
		}
		if (Input.GetKey("1")) 			//idle ->>walk root moove
		{
			anim.SetInteger("battle", 0);
			battle_state = false;
		}
		if (Input.GetKey ("up")) {		 //moving
			if (battle_state == false)
			{
				anim.SetInteger ("moving", 1);//walk root moove
				runSpeed = 1.0f;
			} else 
			{
				anim.SetInteger ("moving", 2);//walk 
				runSpeed = 2f;
			}
			
			
		} else {
			anim.SetInteger ("moving", 0);
		}
		
		
		
		
		if (Input.GetKeyUp("b")) // back view
		{
			anim.SetInteger("moving", 3);
		} 
		if (Input.GetKeyDown("r")) //reverence
		{
			anim.SetInteger("moving", 4);
		}
		

		
		
		if(controller.isGrounded)
		{
			moveDirection=transform.forward * Input.GetAxis ("Vertical") * speed * runSpeed;
			
		}
		float turn = Input.GetAxis("Horizontal");
		transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
		controller.Move(moveDirection * Time.deltaTime);
		moveDirection.y -= gravity * Time.deltaTime;
	}
}