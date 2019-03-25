using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PodSpawner : MonoBehaviour {

	public float radius = 200;
	public Transform podLocation;
	public GameObject podPrefab;

	private EnemyPod pod;

	private void Start() {
		CreateNewPod();
	}

	private void Update() {
		if(pod != null) {
			float squareDistance = (GameController.player.transform.position - transform.position).sqrMagnitude;
			if(squareDistance < radius * radius) {
				pod.enabled = true;
				pod = null;
				GetPod(Random.Range(30f, 120f));
			}
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.DrawWireSphere(transform.position, radius);
	}

	private void CreateNewPod() {
		pod = GameController.InstantiateFromPool(podPrefab, podLocation).GetComponent<EnemyPod>();
		pod.enabled = false;
	}

	private IEnumerator GetPod(float time) {
		yield return new WaitForSeconds(time);
		CreateNewPod();
	}
}
