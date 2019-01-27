using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTestAnim : GroundTest {
	protected override void Land() {
		//Debug.Log("landing");
		parent.anim.SetTrigger("land");
	}
	protected override void Leave() {
		parent.anim.SetTrigger("offGround");
	}
}
