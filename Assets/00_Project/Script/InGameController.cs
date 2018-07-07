using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InGameController : MonoBehaviour {

	[SerializeField] private Character cha;
	[SerializeField] private Camera cam;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		//CameraPosReset();
	}

	private void FixedUpdate()
	{
		CameraPosReset();
	}

	void CameraPosReset()
	{
		Vector3 pos = cha.transform.position;

		

		//cam.transform.DOMove(new Vector3(pos.x, pos.y + 6.5f, pos.z - 11.5f),0.1f);
	}
}
