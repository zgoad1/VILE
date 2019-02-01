using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Ideas:
 * - variety of different types. each has one of these effects
 * - powers up enemies
 *		- hp up
 *		- decrease cooldowns (TIME)
 *		- revive dead enemies (PUPPETEER)
 *		- makes all enemies explode (THERMAL)
 * - debuffs player
 *		- screen flash (LIGHT)
 *		- levitate player (can't attack, can sprint) (KINETIC)
 *		- slows down time (applies to all) (TIME)
 *		- explosions at player footsteps (emissive decal that starts glowing then explodes) (THERMAL)
 *	- buffs player
 *		- st up (ELECTRIC)
 */

public class ArcChalice : Enemy {
	[SerializeField] protected Transform middle;
	public float areaOfEffect = 30;
	[Tooltip("Speed of rotations around y-axis")]
	public float rotationsPerSec = 0;
	[Tooltip("Effect duration in seconds")]
	public float effectDuration = 3;
	
	protected Vector3 newEuler = Vector3.zero;

	protected override void Reset() {
		base.Reset();
		middle = transform.Find("Middle");
		onGround = true;
	}

	protected virtual void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, areaOfEffect);
	}

	protected override void AIUpdate() {
		base.AIUpdate();

		// applying effect when player is near
		if(distanceFromPlayerSquared < areaOfEffect * areaOfEffect) {
			if(!attacking && CanAttack(atk1Cost)) {
				Attack1();
			}
		}

		// rotation
		newEuler.y += rotationsPerSec;//* Time.deltaTime;
		middle.transform.localRotation = Quaternion.Euler(newEuler);
		//Debug.Log("Rotating " + rotationsPerSec + " to " + middle.transform.localEulerAngles.y);
	}

	protected override void Attack1() {
		base.Attack1();
		ApplyEffect();
	}

	public override void Stun() {
		base.Stun();
		StopCoroutine("EffectCR");
		attacking = false;
		anim.SetBool("attacking", false);
	}

	protected virtual void ApplyEffect() {
		StartCoroutine("EffectCR");
	}

	protected virtual IEnumerator EffectCR() {
		anim.SetBool("attacking", true);
		yield return new WaitForSeconds(effectDuration);
		anim.SetBool("attacking", false);
	}

	// debug
	public void AnimFunc_Debug() {
		Debug.Log("IM PLAEYHN TH;E ANIMEMMANTINSHEN MOMMIEEEEE");
	}
}
