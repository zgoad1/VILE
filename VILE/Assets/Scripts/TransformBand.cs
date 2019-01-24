using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**Creates a stretchy "band" of transforms
 * 
 */
 

public class TransformBand : TransformChain {

	private int length = 1;
	private TransformBand leaf;

	// Start is called before the first frame update
	void Start() {
		if(root) {
			TransformBand tb = this;
			while(tb.next.Length > 0) {
				length++;
				tb = (TransformBand)tb.next[0];
			}
			leaf = tb;
		}
	}

	// Update is called once per frame
	void Update() {
		if(root) {
			float distance = Vector3.Distance(transform.position, leaf.transform.position);
			TransformBand tb = (TransformBand)next[0];
			for(int i = 1; i < length; i++) {
				tb.transform.position = Vector3.Lerp(transform.position, leaf.transform.position, (float)i / (length - 1));
				if(tb.next.Length > 0) tb = (TransformBand)tb.next[0];
			}
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, 0.2f);
	}
}
