using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapPanel : MonoBehaviour {

	public TextMeshProUGUI mapInfo, scoreInfo, timeInfo;
	public static int numAttributes = 7;    // number of string attributes

	private string seed, width, height, score, scoreDate, time, timeDate;
	private int index;
	private MainMenu _menu;
	private MainMenu menu {
		get {
			if(_menu == null) {
				_menu = FindObjectOfType<MainMenu>();
			}
			return _menu;
		}
	}



	private void Reset() {
		GetInfoAttributes();
	}

	private void GetInfoAttributes() {
		TextMeshProUGUI[] infos = GetComponentsInChildren<TextMeshProUGUI>();
		try {
			mapInfo = infos[0];
			scoreInfo = infos[1];
			timeInfo = infos[2];
		} catch {
			Debug.LogError("Messed up panel prefab");
		}
	}

	public void SetValues(string[] map) {
		if(mapInfo == null) GetInfoAttributes();
		try {
			seed = map[0];
			width = map[1];
			height = map[2];
			score = map[3];
			scoreDate = map[4];
			time = map[5];
			timeDate = map[6];

			mapInfo.text =
				"<u>Seed</u>\n" +
				seed + "\n" +
				"<u>Dimensions</u>\n" +
				width + " x " + height;
			scoreInfo.text =
				"<u>Best Score</u>\n" +
				score + "\n" +
				"<u>Date Achieved</u>\n" +
				scoreDate;
			timeInfo.text =
				"<u>Best time</u>\n" +
				time + "\n" +
				"<u>Date Achieved</u>\n" +
				timeDate;

		} catch {
			Debug.LogError("Insufficient array provided to MapPanel constructor");
		}
	}

	public void Delete() {
		menu.DeleteMapData(index);
	}

	public void LoadLevel() {
		MapGenerator mapgen = FindObjectOfType<MapGenerator>();
		mapgen.seed = seed;
		mapgen.gridSize = new Vector2(int.Parse(width), int.Parse(height));
		GameController.SceneChange("Intro");
		foreach(Button b in FindObjectsOfType<Button>()) {
			b.interactable = false;
		}
	}
}
