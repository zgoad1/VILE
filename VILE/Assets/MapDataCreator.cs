using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapDataCreator : MonoBehaviour {

	private MainMenu menu;



	private void Start() {
		menu = FindObjectOfType<MainMenu>();
	}

	public void GetInput() {
		// Prompt user with input fields
	}

	public void CreateMapData() {
		menu.SaveMapData(MapGenerator.GetRandomSeed());
		menu.GeneratePanels(MainMenu.saveFile);
	}
}
