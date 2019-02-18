using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FencerPartnerFinder : MonoBehaviour {

	public Fencer parent;

	private void Reset() {
		parent = GetComponentInParent<Fencer>();
	}
}
