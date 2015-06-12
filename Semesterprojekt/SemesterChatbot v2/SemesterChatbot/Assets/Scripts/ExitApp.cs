using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExitApp : MonoBehaviour {

	public Button MyButton;

	// Use this for initialization
	void Start () {
		MyButton.onClick.AddListener (() => {clickclick ();});
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void clickclick (){
		Application.Quit ();
	}
}
