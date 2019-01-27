using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTestFlashEye : GroundTest {
	protected override void Land() {
		// if we hit the ground relatively quickly
		if(!parent.onGround && parent.yMove.y < -0.5f) {
			// shake strength inversely proportional to distance from player
			GameController.camControl.ScreenShake(1f - parent.distanceFromPlayer / 150f);
		}
		base.Land();
	}
}
