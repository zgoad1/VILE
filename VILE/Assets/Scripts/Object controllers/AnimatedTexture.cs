using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedTexture : MonoBehaviour {
	public int _uvTieX = 1;
	public int _uvTieY = 1;
	public int _fps = 10;
	public int materialIndex = 0;
	public bool randomSequence = false;

	private Vector2 _size;
	private Renderer _myRenderer;
	private int _lastIndex = -1;
	private int maxIndex = 0;

	void Start() {
		_size = new Vector2(1.0f / _uvTieX, 1.0f / _uvTieY);
		_myRenderer = GetComponent<Renderer>();
		if(_myRenderer == null)
			enabled = false;
		maxIndex = _uvTieX * _uvTieY;
	}
	// Update is called once per frame
	void Update() {
		// Calculate index
		int index;
		if(randomSequence) {
			do {
				index = Mathf.FloorToInt(Random.Range(0, maxIndex));
			} while(index == _lastIndex);
		} else {
			index = (int)(Time.timeSinceLevelLoad * _fps) % (_uvTieX * _uvTieY);
		}
		if(index != _lastIndex) {
			// split into horizontal and vertical index
			int uIndex = index % _uvTieX;
			int vIndex = index / _uvTieY;

			// build offset
			// v coordinate is the bottom of the image in opengl so we need to invert.
			Vector2 offset = new Vector2(uIndex * _size.x, 1.0f - _size.y - vIndex * _size.y);

			_myRenderer.materials[materialIndex].SetTextureOffset("_MainTex", offset);
			_myRenderer.materials[materialIndex].SetTextureScale("_MainTex", _size);

			_lastIndex = index;
		}
	}
}