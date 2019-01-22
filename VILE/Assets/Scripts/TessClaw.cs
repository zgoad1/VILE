using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: optimize

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TessClaw : MonoBehaviour {

	public LerpFollow clawShoulder;
	public float minThickness = 0f;
	public float thicknessRandomness = 1f;
	public float wiggleRandomness = 0.5f;
	public TrailRenderer[] trails;

	private List<LerpFollow> claw = new List<LerpFollow>();
	private Mesh mesh;
	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triPoints = new List<int>();
	private Vector3 out1 = new Vector3(0, 1, 0);        // this determines the direction outward in which the vertices are drawn (x, y, or z)
	private Vector3 out2 = new Vector3(1, 0, 1);        // in this case the triangles go out in the x and y planes and remain steady on the z plane
	private Vector3[] triOffset;

	private void Reset() {
		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material = Resources.Load<Material>("Lightning");
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.receiveShadows = false;
		clawShoulder = GetComponentInChildren<LerpFollow>();
		trails = GetComponentsInChildren<TrailRenderer>();
	}

	private void Start() {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		AddClaw(clawShoulder, claw);

		triOffset = new Vector3[] {
			out1,
			-out1 - out2,
			-out1 + out2
		};

		UpdateVertices();

		int armSegments = 2;
		int fingerSegments = 2;
		int numFingers = 3;

		// connect all arm segments
		for(int i = 0; i < armSegments; i++) {
			AddTriPoints(i, i + 1);
		}
		for(int i = 0; i < numFingers; i++) {
			int knuckle = armSegments + 1 + fingerSegments * i;
			// connect finger to knuckle
			AddTriPoints(armSegments, knuckle);

			// connect knuckle to the rest of the finger segments
			for(int j = knuckle; j < knuckle + fingerSegments - 1; j++) {
				AddTriPoints(j, j + 1);
			}
		}

		UpdateMesh();
	}

	void AddTriPoints(int t1, int t2) {
		int[] verts = new int[] {
			t1 * 3, t1 * 3 + 1, t1 * 3 + 2,
			t2 * 3, t2 * 3 + 1, t2 * 3 + 2
		};
		for(int i = 0; i < 3; i++) {
			// |_
			triPoints.Add(verts[0]);
			triPoints.Add(verts[3]);
			triPoints.Add(verts[2]);
			// _
			// \|
			triPoints.Add(verts[2]);
			triPoints.Add(verts[3]);
			triPoints.Add(verts[5]);

			// This should effectively shift all the elements left in the array, but do so
			// as if it were a 2D array (like how the declaration looks)
			for(int a = 0; a < verts.Length; a++) {
				verts[a] = (verts[a] + 2) % 3 + 3 * (a <= 2 ? t1 : t2);
			}
		}
	}

	// Update is called once per frame
	void Update() {
		transform.position = Vector3.zero;
		UpdateVertices();
		UpdateMesh();
	}

														// TODO: analyze the order in which vertices are added here
	private void AddClaw(LerpFollow clawPoint, List<LerpFollow> claw) {
		if(clawPoint != null) {
			claw.Add(clawPoint);
			foreach(LerpFollow c in clawPoint.next) {
				AddClaw(c, claw);
			}
		}
	}

	void UpdateVertices() {
		vertices.Clear();
		float ithicknessRandomness = thicknessRandomness;
		float ithickness = minThickness / 2;
		float thickness = ithickness;
		Vector3 iwiggle = new Vector3(Random.Range(-wiggleRandomness, wiggleRandomness), Random.Range(-wiggleRandomness, wiggleRandomness), Random.Range(-wiggleRandomness, wiggleRandomness));
		Vector3 wiggle = iwiggle;
		foreach(LerpFollow p in claw) {
			// make ends of claws end in points
			if(p.root || p.next.Length == 0) {
				thickness = 0;
				thicknessRandomness = 0;
				wiggle = Vector3.zero;
			} else {
				thickness = ithickness;
				thicknessRandomness = ithicknessRandomness;
				wiggle = iwiggle;
			}

			// add 3 vertices for this point
			vertices.Add(new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z) + triOffset[0] * (thickness + Random.Range(0f, thicknessRandomness)) + wiggle);// - out1 * thickness - out1 * Random.Range(0f, thicknessRandomness) + wiggle);
			vertices.Add(new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z) + triOffset[1] * (thickness + Random.Range(0f, thicknessRandomness)) + wiggle);// + out1 * thickness + out1 * Random.Range(0f, thicknessRandomness) + wiggle);
			vertices.Add(new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z) + triOffset[2] * (thickness + Random.Range(0f, thicknessRandomness)) + wiggle);

			iwiggle = new Vector3(Random.Range(-wiggleRandomness, wiggleRandomness), Random.Range(-wiggleRandomness, wiggleRandomness), Random.Range(-wiggleRandomness, wiggleRandomness));
		}
		thickness = ithickness;
		thicknessRandomness = ithicknessRandomness;
		wiggle = iwiggle;
	}

	void UpdateMesh() {
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triPoints, 0);
		mesh.RecalculateBounds();
	}
}
