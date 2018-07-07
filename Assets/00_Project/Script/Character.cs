using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;

public class Character : MonoBehaviour {

	public enum MoveState
	{
		None,
		Forward,
		Back,
		Left,
		Rigth,
	}

	MoveState currentMoveState;

	public float speed = 5f;    //속도

	[SerializeField] Transform charactorModel;
	[SerializeField] Animator anim;
	[SerializeField] private Rigidbody rigidbody;

	List<bool> moveStateOnList = new List<bool>();

	AnimatorStateInfo animState;

	bool jumpOn;
	float jumpDelay;

	[SerializeField] private ChaCamController camPrantObj;

	public Vector3 jumpTest;

	// Use this for initialization
	void Start () {

		MoveStateOnListSet();
		animState = anim.GetCurrentAnimatorStateInfo(0);
	}

	void MoveStateOnListSet()
	{
		moveStateOnList = new List<bool>();

		foreach (MoveState sample in Enum.GetValues(typeof(MoveState)))
		{
			moveStateOnList.Add(false);
		}
	}

	// Update is called once per frame
	void Update () {

		JumpDelayCheck();
		InputCheck();
	}

	private void JumpDelayCheck()
	{
		if (jumpOn == false)
			return;
		
		bool isPlaying = isAnimPlaying("jump");
		
		if (isPlaying == false)
		{
			currentMoveState = MoveState.None;
			jumpOn = false;
		}
	}

	private void InputCheck()
	{
		for (int i = 0; i < moveStateOnList.Count; i++)
		{
			moveStateOnList[i] = false;
		}
		
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			MoveOn(MoveState.Forward);
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			MoveOn(MoveState.Back);
		}
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			MoveOn(MoveState.Left);
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			MoveOn(MoveState.Rigth);
		}
		if (Input.GetKey(KeyCode.Space))
		{
			JumpOn();
		}
		if (Input.GetKey(KeyCode.Alpha1))
		{
			HappyOn();
		}
		if (!moveStateOnList.Any(value => value))
		{
			MoveOn(MoveState.None);
		}
	}

	private void MoveOn(MoveState moveState)
	{
		if (isAnimPlaying("happy"))
			return;
		
		moveStateOnList[(int)moveState] = true;
		
		if (currentMoveState != moveState && !jumpOn)
		{
			if (isAnimPlaying("move") && moveState == MoveState.None)
			{
				anim.SetBool("MoveOn", false);
			}
			else if (isAnimPlaying("move") == ChaMoveOn())
			{
				anim.SetBool("MoveOn", true);
				anim.Play("move");
			}
		}
		
		currentMoveState = moveState;
		
		if (currentMoveState == MoveState.None)
			return;
		
		Lotate(moveState);

		Vector3 moveVector = Vector3.zero;

		switch (moveState)
		{
			case MoveState.Forward :	moveVector = Vector3.forward; break;
			case MoveState.Back:		moveVector = Vector3.back; break;
			case MoveState.Left:		moveVector = Vector3.left; break;
			case MoveState.Rigth:		moveVector = Vector3.right; break;
		}
		
		transform.Translate(moveVector * speed * Time.deltaTime);
	}

	private void Lotate(MoveState moveState)
	{
		if (ChaMoveOn())
		{
			Vector3 camParantRotValue = camPrantObj.transform.localRotation.eulerAngles;
			transform.DOLocalRotate(camParantRotValue, 0.1f);
		}

		float angle = 0;

		switch (moveState)
		{
			case MoveState.Forward: angle = 0; break;
			case MoveState.Back: angle = 180; break;
			case MoveState.Left: angle = 270; break;
			case MoveState.Rigth: angle = 90; break;
		}

		charactorModel.transform.DOLocalRotate(new Vector3(0, angle, 0), 0.3f);
	}

	private bool ChaMoveOn()
	{
		return currentMoveState == MoveState.Forward ||
				currentMoveState == MoveState.Back ||
				currentMoveState == MoveState.Left ||
				currentMoveState == MoveState.Rigth;
	}

	public void JumpOn()
	{
		if (jumpOn)
			return;

		jumpOn = true;
		
		anim.Play("jump");
		
		float camVRotalue = transform.localEulerAngles.y % 360;

		if (camVRotalue < 0)
			camVRotalue = 360 + camVRotalue;
		
		float xValue = 0;
		float zValue = 0;

		if (0 <= camVRotalue && camVRotalue <= 180)
		{
			zValue = -90 + camVRotalue;
			xValue = -(90 - Mathf.Abs(zValue));
		}
		else
		{
			zValue = 90 - (90 + camVRotalue);
			xValue = -(90 - Mathf.Abs(zValue));
		}
		
		rigidbody.AddForce(new Vector3(0,90,0) * 2);
	}

	public void HappyOn()
	{
		if (isAnimPlaying("stay") || isAnimPlaying("move"))
		{
			anim.Play("happy");
		}
	}

	private bool isAnimPlaying(string name)
	{
		return anim.GetCurrentAnimatorStateInfo(0).IsName(name);
	}
	
}
