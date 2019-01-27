using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfContentRandomizer : MonoBehaviour {

	public float itemsAcross = 4;
	public float shelves = 4;
	public float shelfWidth = 3;
	public float ratio = 0.25f;

	public static GameObject[] items;
	public static bool loaded = false;

	// Start is called before the first frame update
	void Start() {

		// Load items, if not done already
		if(!loaded) {
			items = Resources.LoadAll<GameObject>("Shelf items");
			loaded = true;
		}

		// Set up to (itemsAcross * shelves) items, evenly spaced
		float spacing = shelfWidth / (itemsAcross - 1);
		Vector3 location = Vector3.zero;
		for(int i = 0; i < shelves; i++) {
			for(int j = 0; j < itemsAcross; j++) {
				int rand = Mathf.FloorToInt(Random.Range(0, 1 / ratio * items.Length));
				if(rand < items.Length) {
					location.z = 0.1f + i;
					location.y = -0.5f;
					location.x = -shelfWidth / 2 + spacing * j;

					GameObject item = Instantiate(items[rand]);
					item.transform.SetParent(transform);
					item.transform.localPosition = location;
					item.transform.rotation = transform.rotation;

					Vector3 rotation = Vector3.zero;
					rotation.z = Random.Range(-60, 60);
					rotation += item.transform.rotation.eulerAngles;
					item.transform.rotation = Quaternion.Euler(rotation);
				}
			}
		}
	}
}
