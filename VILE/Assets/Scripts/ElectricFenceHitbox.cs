using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Hitbox for the electric fences used by Fencers.
 * 
 * Works as a normal hitbox with these exceptions:
 * - Causes the player to stop sprinting
 * - Applies knockback in the direction outward from the hitbox on the player's side
 * 
 * 
 * Determining knockback direction:
 * 
 * (P) - player location
 * [rectangle] - this hitbox
 * 
 * Player to left | Player to right
 *        ----    |    ----
 *        |  |    |    |  |
 *        |  |    |    |  |
 *        |  |    |    |  |
 *  (P) ← |  |    |    |  | → (P)
 *        |  |    |    |  |
 *        |  |    |    |  |
 *        |  |    |    |  |
 *        ----    |    ----
 */

public class ElectricFenceHitbox : AttackHitbox {

	private Vector3 hitboxCenter, playerHit;

	protected override void ApplyEffects(Controllable character) {

		playerHit = character.transform.position;

		// Make player stop sprinting
		if(character is Player && ((Player)character).isLightning) {
			((Player)character).OnHitElectricFence();
		}

		// Damage character as normal
		character.Damage(power, gracePeriod);

		// Get knockback direction & apply knockback
		if(knockbackPower > 0) {
			Vector3 knockbackDirection;
			bool playerToRight;
			float knockbackMultiplier = 1f;

			if(character.calculatedVelocity == Vector3.zero) {
				// The player isn't moving; determine knockback direction via position
				hitboxCenter = transform.position + transform.parent.localScale.z * transform.forward / 2f;
				float dotProduct = Vector3.Dot(transform.right, character.transform.position - hitboxCenter);
				playerToRight = dotProduct > 0;
				//Debug.Log("(1) Dot: " + dotProduct);
			} else {
				// The player is moving; determine knockback direction via velocity
				knockbackMultiplier = 2f;
				float dotProduct = Vector3.Dot(transform.right, character.calculatedVelocity);
				playerToRight = dotProduct < 0;
				//Debug.Log("(2) Dot: " + dotProduct);
			}

			if(playerToRight) {
				knockbackDirection = transform.right;
			} else {
				knockbackDirection = -transform.right;
			}

			// Apply knockback
			character.Knockback(knockbackMultiplier * knockbackPower * knockbackDirection);
		}
	}

	// debug
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(hitboxCenter, 0.35f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(playerHit, 0.35f);
	}
}
