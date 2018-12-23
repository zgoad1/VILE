using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FaceText {
	public Sprite face;
    [TextArea(10, 10)] public string text;    // TextArea doesn't work for some unknown reason
	public bool useCutsceneDbox = false;
}
