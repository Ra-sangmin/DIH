using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
		
	}

	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClickMenu()
	{
		Debug.LogWarning("Menu");
	}

	public void OnClickStart()
	{
		SceneManager.LoadScene("InGame");
		//SceneManager.LoadScene("TestScene_V3_0608");
		Debug.LogWarning("Start");
	}

}
