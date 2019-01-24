using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerTracker))]
public class Enemy : Controllable {
	
	protected static Player player;
	protected static Vector3 HpBarOffset = new Vector3(-100, 64, 0);
	protected float playerDamage = 0.2f;    // damage to apply every frame in which the player
											// is possessing this enemy
	[SerializeField] protected GameObject deathObject;  // an object that does VFX for death (explosions, etc)
	protected PlayerTracker tracker;
	protected bool checkedForPlayer = false;    // whether we've called CanSeePlayer() this frame (saves a raycast when calling CanSeePlayer() twice in a frame)
	protected bool sawPlayer = false;

	[Tooltip("How far this enemy can see")]
	public float sightLength = 200;

	public bool CanSeePlayer() {
		if(checkedForPlayer) return sawPlayer;
		checkedForPlayer = true;
		RaycastHit hit;
		if(Physics.Raycast(transform.position, GameController.playerTarget.position - transform.position, out hit, sightLength, GameController.defaultLayerMask)) {
			sawPlayer = hit.transform.gameObject.GetComponent<Player>() != null;
			return sawPlayer;
		}
		sawPlayer = false;
		return false;
	}

	protected override void Reset() {
		base.Reset();

		if(renderer != null) {
			TargetableRenderer cr;
			cr = renderer.GetComponent<TargetableRenderer>();
			if(cr == null) {
				cr = renderer.gameObject.AddComponent<TargetableRenderer>();
			}
			cr.parent = this;
		} else {
			Debug.LogWarning("You have an Enemy with a weird renderer (not mesh)");
		}

		player = FindObjectOfType<Player>();
		tracker = GetComponent<PlayerTracker>();

		defaultGracePeriod = 0f;	// no invincibility period
	}

	protected override void PlayerUpdate() {
		base.PlayerUpdate();
		target = GameController.player.target;
		EnemyPlayerUpdate();
	}

	protected virtual void EnemyPlayerUpdate() {
		SetScreenCoords();
		hpBar.gameObject.SetActive(true);
		//Damage(playerDamage);
		hp -= playerDamage * 60 * Time.deltaTime;
		GameController.player.stamina += playerDamage / 2f;
	}

	protected override void Update() {
		base.Update();
		checkedForPlayer = false;

		#region Set HPBar position
		Vector3 newScreenCoords = screenCoords;
		newScreenCoords.x -= 0.5f;
		newScreenCoords.y -= 0.5f;
		newScreenCoords.x *= Screen.width * (GameController.UICanvas.rect.width / Screen.width);
		newScreenCoords.y *= Screen.height * (GameController.UICanvas.rect.height / Screen.height);
		hpBar.transform.localPosition = newScreenCoords + HpBarOffset;
		#endregion
	}

	protected override void Start() {
		base.Start();
		SetTarget();
		hpBar = Instantiate(GameController.enemyHpBarObject).GetComponent<UIBar>();
		hpBar.character = this;
		hpBar.maxValue = maxHP;
		hpBar.gameObject.SetActive(false);
		hpBar.transform.SetParent(GameController.UICanvas.transform);
	}

	protected override void SetTarget() {
		base.SetTarget();
		if(control == state.AI) target = FindObjectOfType<Player>();
		else target = player.target;
	}

	public override void Damage(float damage) {
		if(!invincible) {
			StopCoroutine("ShowHP");
			StartCoroutine("ShowHP");
		}
		base.Damage(damage);
	}

	protected IEnumerator ShowHP() {
		hpBar.gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		if(this != GameController.player.target) hpBar.gameObject.SetActive(false);
	}

	protected override void Die() {
		if(this == GameController.player.possessed && player.possessing) GameController.player.Unpossess(false);
		// Create the death VFX
		GameController.InstantiateFromPool(deathObject, transform);
		base.Die();
		Destroy(gameObject);
	}
}
