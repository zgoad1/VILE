using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: ☭☭☭

public class TessClaw : MonoBehaviour {

	public Material material;
	public TransformChain clawShoulder;
	public float minThickness = 0f;
	public float thicknessRandomness = 1f;
	public float wiggleRandomness = 0.5f;
	public TrailRenderer[] trails;
	public int armSegments = 2;
	public int fingerSegments = 2;
	public int numFingers = 3;

	private List<TransformChain> claw = new List<TransformChain>();
	private Mesh mesh;
	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triPoints = new List<int>();
	private Vector3 out1 = new Vector3(0, 1, 0);        // this determines the direction outward in which the vertices are drawn (x, y, or z)
	private Vector3 out2 = new Vector3(1, 0, 1);        // in this case the triangles go out in the x and y planes and remain steady on the z plane
	private Vector3[] triOffset = new Vector3[3];
	private GameObject clawRenderer;

	private void Reset() {
		//MeshRenderer mr = GetComponent<MeshRenderer>();
		//mr.material = Resources.Load<Material>("Lightning");
		//mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		//mr.receiveShadows = false;
		clawShoulder = GetComponentInChildren<TransformChain>();
		trails = GetComponentsInChildren<TrailRenderer>();
	}

	private void Start() {
		// create empty object for mesh renderer
		clawRenderer = new GameObject("Claw Renderer (" + gameObject.name + ")");
		MeshFilter mf = clawRenderer.AddComponent<MeshFilter>();
		mesh = new Mesh();
		mf.mesh = mesh;
		MeshRenderer mr = clawRenderer.AddComponent<MeshRenderer>();
		mr.material = material;
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.receiveShadows = false;

		AddClaw(clawShoulder, claw);

		UpdateVertices();

		// connect all arm segments
		for(int i = 0; i < armSegments; i++) {
			AddTriPoints(i, i + 1);
		}

		for(int i = 0; i < numFingers; i++) {
			int knuckle = armSegments + 1 + fingerSegments * i;
			// connect knuckle to wrist
			AddTriPoints(armSegments, knuckle);

			// connect knuckle to the rest of the finger segments
			for(int j = knuckle; j < knuckle + fingerSegments - 1; j++) {
				AddTriPoints(j, j + 1);
			}
		}

		UpdateMesh();
	}

	private void OnDestroy() {
		Destroy(clawRenderer);
	}

	void OnEnable() {
		if(clawRenderer != null) clawRenderer.SetActive(true);
	}

	void OnDisable() {
		if(clawRenderer != null) clawRenderer.SetActive(false);
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

			// An extremely roundabout way to shift all the elements left in the array
			// as if it were a 2D array (like how the declaration looks)
			for(int a = 0; a < verts.Length; a++) {
				verts[a] = (verts[a] + 2) % 3 + 3 * (a <= 2 ? t1 : t2);
			}
		}
	}

	// Update the outward directions in which to place the triangle vertices, based on
	// a TransformChain's transform's orientation
	void UpdateTriOffset(Transform transform) {
		out1 = transform.up;
		out2 = transform.right;
		triOffset[0] = out1;
		triOffset[1] = -out1 - out2;
		triOffset[2] = -out1 + out2;
	}

	// Update is called once per frame
	void Update() {
		UpdateVertices();
		UpdateMesh();
	}

	private void AddClaw(TransformChain clawPoint, List<TransformChain> claw) {
		if(clawPoint != null) {
			claw.Add(clawPoint);
			foreach(TransformChain c in clawPoint.next) {
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
		foreach(TransformChain p in claw) {
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

			UpdateTriOffset(p.transform);

			// add 3 vertices for this point
			vertices.Add(new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z) + triOffset[0] * (thickness + Random.Range(0f, thicknessRandomness)) + wiggle);
			vertices.Add(new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z) + triOffset[1] * (thickness + Random.Range(0f, thicknessRandomness)) + wiggle);
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

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		foreach(Vector3 v in vertices) 
			Gizmos.DrawSphere(v, 0.1f);
	}
}
