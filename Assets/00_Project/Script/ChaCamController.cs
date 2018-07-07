using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChaCamController : MonoBehaviour {

	bool mouseRightClickOn;
	Vector3 mouseStartPos;
	float startCamYValue;
	float changeValue;

	[SerializeField] private Camera cam;
	[SerializeField] private Transform target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		TargetPosCheck();
		MouseInputCheck();
	}
	
	private void TargetPosCheck()
	{
		transform.DOLocalMove(target.transform.localPosition, 0.5f);
	}

	private void MouseInputCheck()
	{
		//마우스 오른쪽
		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			mouseRightClickOn = true;
			startCamYValue = transform.localRotation.eulerAngles.y;
			mouseStartPos = Input.mousePosition;
		}
		else if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			mouseRightClickOn = false;
		}
		else if (Input.GetKey(KeyCode.Mouse1))
		{
			Vector3 currentPos = Input.mousePosition;

			float newChangeValue = (currentPos.x - mouseStartPos.x) / 10;

			if (newChangeValue != changeValue)
			{
				changeValue = newChangeValue;
				transform.localRotation = Quaternion.Euler(0, startCamYValue + changeValue, 0);
			}

			transform.localRotation = Quaternion.Euler(0, startCamYValue + newChangeValue, 0);
		}


		//마우스 휠 클릭
		if (Input.GetKey(KeyCode.Mouse2))
		{
			Debug.LogWarning("Mouse2");
		}

		//마우스 휠 아래로 내림
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			CameraFieldOfViewReset(3);
		}

		//마우스 휠 위로 올림
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			CameraFieldOfViewReset(-3);
		}

	}

	public void StartRotValueReset()
	{
		startCamYValue = 0;
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		mouseStartPos = Input.mousePosition;
	}

	private void CameraFieldOfViewReset(int value)
	{
		float newValue = cam.fieldOfView + value;

		newValue = Mathf.Clamp(newValue, 20, 80);

		cam.fieldOfView = newValue;
	}


}
