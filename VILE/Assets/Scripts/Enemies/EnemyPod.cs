using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPod : Enemy {

	protected override void AIUpdate() {
		base.AIUpdate();
		velocity = Vector3.Lerp(velocity, Vector3.zero, accel);
	}

	public override void Knockback(Vector3 force) {
		velocity += force;
	}

	private void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if(other.gameObject.layer == GameController.solidLayer) {
			GameController.camControl.ScreenShake(1f - distanceFromPlayer / 150f);
			anim.SetTrigger("land");
			StartCoroutine("Open");
		} else if(player != null) {
			player.Damage(8);
			player.Knockback((player.transform.position - transform.position).normalized * 5);
		}
	}

	private IEnumerator Open() {
		yield return new WaitForSeconds(2f);
		anim.SetTrigger("open");
	}
}
