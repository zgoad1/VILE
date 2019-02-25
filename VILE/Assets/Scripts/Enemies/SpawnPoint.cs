using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BillboardRenderer))]	// only used for access to OnBecameVisible()
public class SpawnPoint : MonoBehaviour {

	public Color color = Color.red;
	public Mesh gfx;
	public List<Spawnable> spawnables = new List<Spawnable>();

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
				if(spawnables[i].toSpawn != null) {
					GameObject g = Instantiate(spawnables[i].toSpawn);
					g.transform.position = transform.position;
				}
				break;
			} else {
				currentSum += spawnables[i].likelihood;
			}
		}
		enabled = false;
		GetComponent<BillboardRenderer>().enabled = false;
	}

	/* Draw the SpawnPoint graphic
	 */
	private void OnDrawGizmos() {
		Gizmos.color = color;
		Gizmos.DrawMesh(gfx, transform.position);
	}

	/* Draw all the meshes and child meshes of each of the Spawnables
	 */
#if UNITY_EDITOR
	private void OnDrawGizmosSelected() {
		Transform cam = UnityEditor.SceneView.GetAllSceneCameras()[0].transform;
		Gizmos.color = Color.white;
		float spacing = 10;
		
		for(int i = 0; i < spawnables.Count; i++) {
			SkinnedMeshRenderer[] mrs;	// to contain the MeshRenderer of this spawnable and all MRs in its children
			MeshFilter[] mfs;					// ~ MeshFilters
			if(spawnables[i].toSpawn != null) {
				mrs = spawnables[i].toSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();
				mfs = spawnables[i].toSpawn.GetComponentsInChildren<MeshFilter>();
			} else {
				mrs = new SkinnedMeshRenderer[0];
				mfs = new MeshFilter[0];
			}
			List<Mesh> meshes = new List<Mesh>();
			foreach(SkinnedMeshRenderer r in mrs) {
				meshes.Add(r.sharedMesh);
			}
			foreach(MeshFilter f in mfs) {
				meshes.Add(f.sharedMesh);
			}
			if(meshes.Count > 0) {
				// Draw the actual meshes if the Spawnable has them
				foreach(Mesh m in meshes) {
					Gizmos.DrawWireMesh(m, transform.position + cam.up * 5 - spacing * cam.right * (spawnables.Count - 1) / 2f + cam.right * spacing * i);
				}
			} else {
				// Draw the SpawnPoint's mesh if the Spawnable is null or has no meshes
				Gizmos.DrawWireMesh(gfx, transform.position + cam.up * 5 - spacing * cam.right * (spawnables.Count - 1) / 2f + cam.right * spacing * i);
			}
		}
	}
#endif
}

class SpawnableComparer : IComparer<Spawnable> {
	public int Compare(Spawnable x, Spawnable y) {
		if(x.likelihood > y.likelihood) return 1;
		return -1;
	}
}