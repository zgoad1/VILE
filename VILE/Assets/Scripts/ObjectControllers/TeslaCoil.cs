using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaCoil : Recharger {

	[Tooltip("Amount of stamina this Recharger can give before it dies")]
	public float hp = 100;
	public GameObject deathObject;


	protected override void GiveStamina(float st) {
		float ist = GameController.player.stamina;
		base.GiveStamina(st);
		float stDiff = GameController.player.stamina - ist;
		hp -= stDiff * Time.deltaTime * 60;
		if(hp <= 0) {
			GameController.InstantiateFromPool(deathObject, transform.parent);
			Destroy(transform.parent.gameObject);
		}
	}
}
