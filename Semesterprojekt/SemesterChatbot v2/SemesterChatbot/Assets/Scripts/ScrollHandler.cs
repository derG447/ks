using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class ScrollHandler : MonoBehaviour {

    private ScrollRect scroll_rect;

	// Use this for initialization
	void Start () {
      this.scroll_rect = gameObject.GetComponent<ScrollRect>();
      
	}
	
	// Update is called once per frame
	void Update () {
      if (Input.GetButtonDown("Vertical"))
      {
          this.scroll_rect.verticalNormalizedPosition = 0;
      }
	}
}
