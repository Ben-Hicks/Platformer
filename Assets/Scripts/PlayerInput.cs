using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Player))]
public class PlayerInput : MonoBehaviour {

	public XMLManager xmlmanager;
	Player player;

	// Use this for initialization
	void Start () {
		player = GetComponent<Player> ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 directionalInput = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		player.SetDirectionalInput (directionalInput);
	
		if (Input.GetButtonDown ("Jump")) {
			player.OnJumpInputDown ();
		}

		if (Input.GetButtonUp ("Jump")) {
			player.OnJumpInputUp ();
		}

		if (Input.GetButtonDown("Roll")){
			player.OnRollInputDown();
		}

		if (Input.GetButtonDown ("Start")) {
			Application.Quit ();
		}

		if (Input.GetButtonDown ("Save")) {
			Debug.Log ("Saving");
			xmlmanager.SaveBlocks ();
		}
	}
}
