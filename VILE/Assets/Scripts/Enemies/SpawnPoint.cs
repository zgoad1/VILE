using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BillboardRenderer))]	// only used for access to OnBecameVisible()
public class SpawnPoint : MonoBehaviour {

	public Color color = Color.red;
	public Mesh gfx;
	public List<Spawnable> spawnables;

	private float sum = 0;

	private void Reset() {
		gfx = Resources.Load<Mesh>("DiamondPrism");
	}

	private void OnBecameVisible() {
		SpawnObject();
	}

	void SpawnObject() {
		sum = 0;
		foreach(Spawnable s in spawnables) {
			sum += s.likelihood;
		}
		SpawnableComparer comp = new SpawnableComparer();
		spawnables.Sort(comp);
		float rand = Random.Range(0f, sum);
		float currentSum = 0;
		for(int i = 0; i < spawnables.Count; i++) {
			if(rand >= currentSum && rand < currentSum + spawnables[i].likelihood) {
				GameObject g = Instantiate(spawnables[i].toSpawn);
				g.transform.position = transform.position;
				break;
			} else {
				currentSum += spawnables[i].likelihood;
			}
		}
		enabled = false;
		GetComponent<BillboardRenderer>().enabled = false;
	}

	private void OnDrawGizmos() {
		Gizmos.color = color;
		Gizmos.DrawMesh(gfx, transform.position);
	}

	private void OnDrawGizmosSelected() {
		Transform cam = SceneView.GetAllSceneCameras()[0].transform;
		Gizmos.color = Color.white;
		float spacing = 10;

		for(int i = 0; i < spawnables.Count; i++) {
			SkinnedMeshRenderer[] mrs;
			MeshFilter[] mfs;
			if(spawnables[i].toSpawn != null) {
				mrs = spawnables[i].toSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();
				mfs = spawnables[i].toSpawn.GetComponentsInChildren<MeshFilter>();
			} else {
				mrs = null;
				mfs = null;
			}
			List<Mesh> meshes = new List<Mesh>();
			foreach(SkinnedMeshRenderer r in mrs) {
				meshes.Add(r.sharedMesh);
			}
			foreach(MeshFilter f in mfs) {
				meshes.Add(f.sharedMesh);
			}
			if(meshes.Count > 0) {
				foreach(Mesh m in meshes) {
					Gizmos.DrawWireMesh(m, transform.position + cam.up * 5 - spacing * cam.right * (spawnables.Count - 1) / 2f + cam.right * spacing * i);
				}
			} else {
				Gizmos.DrawWireMesh(gfx, transform.position + cam.up * 5 - spacing * cam.right * (spawnables.Count - 1) / 2f + cam.right * spacing * i);
			}
		}
	}
}

class SpawnableComparer : IComparer<Spawnable> {
	public int Compare(Spawnable x, Spawnable y) {
		if(x.likelihood > y.likelihood) return 1;
		return -1;
	}
}