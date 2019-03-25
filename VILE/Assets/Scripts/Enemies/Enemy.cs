using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerTracker))]
public class Enemy : Controllable {

	public bool canBePossessed = true;

	protected static Player player;
	protected static Vector3 HpBarOffset = new Vector3(-100, 64, 0);
	[Tooltip("Damage to apply every frame in which the player is possessing this enemy.")]
	[SerializeField] protected float playerDamage = 0.2f;
	[Tooltip("VFX to play upon death.")]
	[SerializeField] protected GameObject deathObject;
	protected PlayerTracker tracker;
	protected bool checkedForPlayer = false;    // whether we've called CanSeePlayer() this frame (saves a raycast when calling CanSeePlayer() twice in a frame)
	protected bool sawPlayer = false;
	protected ParticleSystem stunSparks;    // needed to stop the effect if we're possessed
	protected bool haveStoppedStunSparks = true;
	protected TargetableRenderer tRenderer;

	// debug
	//public Vector3 playerPoint;

	public bool CanSeePlayer() {
		if(checkedForPlayer) return sawPlayer;
		checkedForPlayer = true;
		RaycastHit hit;
		if(Physics.Raycast(transform.position, GameController.playerTarget.position - transform.position, out hit, sightLength, GameController.defaultLayerMask)) {
			sawPlayer = hit.transform.gameObject.GetComponent<Player>() != null;
			//playerPoint = hit.point;
			return sawPlayer;
		}
		sawPlayer = false;
		return false;
	}

	//private void OnDrawGizmos() {
	//	Gizmos.color = Color.blue;
	//	Gizmos.DrawSphere(playerPoint, 0.5f);
	//}

	protected override void Reset() {
		base.Reset();

		if(renderer != null) {
			tRenderer = renderer.GetComponent<TargetableRenderer>();
			if(tRenderer == null) {
				tRenderer = renderer.gameObject.AddComponent<TargetableRenderer>();
			}
			tRenderer.parent = this;
		} else {
			Debug.LogWarning("You have an Enemy with a weird renderer (not mesh)");
		}

		player = FindObjectOfType<Player>();
		tracker = GetComponent<PlayerTracker>();

		defaultGracePeriod = 0f;    // no invincibility period
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
		hpBar = Instantiate(GameController.enemyHpBarPrefab).GetComponent<UIBar>();
		hpBar.character = this;
		hpBar.value = hp;
		hpBar.maxValue = maxHP;
		hpBar.gameObject.SetActive(false);
		hpBar.transform.SetParent(GameController.UICanvas.transform);
	}

	protected virtual void OnDisable() {
		onScreen.Remove(this);
		isOnScreen = false;
		Player.targets.Remove(this);
	}

	public override void SetTarget() {
		base.SetTarget();
		if(control == state.AI) target = FindObjectOfType<Player>();
		else target = player.target;
	}

	public override void Damage(float damage, float gracePeriod) {
		if(!invincible) {
			base.Damage(damage, gracePeriod);
			StopCoroutine("ShowHP");
			StartCoroutine("ShowHP");
			float powerFactor = damage / 120;
			float deathFactor = dead ? 5 : 1;
			if(powerFactor * deathFactor > 0.02f) {
				GameController.HitStop(Mathf.Min(0.8f, powerFactor * deathFactor));
			}
		}
	}

	public override void Stun() {
		if(!dead) {
			base.Stun();
			stunSparks = GameController.InstantiateFromPool(GameController.stunSparksPrefab, camLook).GetComponent<ParticleSystem>();
			haveStoppedStunSparks = false;
		}
	}

	public override void Knockback(Vector3 force) {
		base.Knockback(force);
		// Most enemies won't have a hurt animation, so just change hurtAnimPlaying back to false
		// after a couple seconds
		StartCoroutine("FinishKnockback");
	}

	protected IEnumerator FinishKnockback() {
		yield return new WaitForSeconds(2f);
		hurtAnimPlaying = false;
	}

	protected IEnumerator ShowHP() {
		hpBar.gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		if(this != GameController.player.target) hpBar.gameObject.SetActive(false);
	}

	public override void SetPlayer() {
		base.SetPlayer();
		StopStunSparks();
	}

	protected override void Die() {

		// Tell player.SetTarget() to ignore us
		canTarget = false;
		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		if(this == GameController.player.possessed && player.possessing) {
			// Kick the player out if they had us possessed
			GameController.player.Unpossess(false);
		} else if(this == GameController.player.target) {
			// Find a new target if the player was targeting us
			GameController.player.SetTarget();
		}

		// Create the death VFX
		GameController.InstantiateFromPool(deathObject, transform);
		base.Die();
		StopStunSparks();
		Destroy(gameObject);
	}

	private void StopStunSparks() {
		if(stunSparks != null && !haveStoppedStunSparks) {
			stunSparks.Stop();
			haveStoppedStunSparks = true;
		}
	}
}
