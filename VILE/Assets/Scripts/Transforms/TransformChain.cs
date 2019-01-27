using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformChain : MonoBehaviour {
	public bool root;
	public TransformChain[] next = new TransformChain[0];
}
