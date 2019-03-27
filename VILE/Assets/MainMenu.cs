using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class MainMenu : MonoBehaviour {

	public static string _saveFile;
	public static string saveFile {
		get {
			if(_saveFile == null) {
				_saveFile = Application.persistentDataPath + "/mapdata.txt";
			}
			return _saveFile;
		}
	}
	public GameObject contentPane, mapPanelPrefab;

	private GameObject[] mapPanels;



	// Start is called before the first frame update
	void Start() {
		GeneratePanels(saveFile);
	}

	/**Use the below methods to generate the UI panels for each map.
	 */
	public void GeneratePanels(string file) {
		List<string[]> mapData = GetMapData(file);
		CreateMapPanels(mapData);
	}

	private StreamReader GetNewFileReader(string file) {
		EnsureFileExists(file);
		return new StreamReader(file, true);
	}

	private StreamWriter GetNewFileWriter(string file) {
		EnsureFileExists(file);
		return new StreamWriter(file, true);
	}

	private void EnsureFileExists(string file) {
		if(!File.Exists(file)) {
			File.Create(file).Close();
			Debug.Log("Creating new data file");
		}
	}

	/**Destroy any existing Map Panel objects, and create new ones from the given
	 * map data.
	 */
	public void CreateMapPanels(List<string[]> mapData) {

		if(mapPanels != null) {
			foreach(GameObject mp in mapPanels) {
				Destroy(mp);
			}
		}
		
		mapPanels = new GameObject[mapData.Count];
		for(int i = 0; i < mapData.Count; i++) {
			GameObject newPanelObject = Instantiate(mapPanelPrefab, contentPane.transform);
			RectTransform panelRect = newPanelObject.GetComponent<RectTransform>();
			panelRect.localPosition = new Vector3(panelRect.localPosition.x, -105 - (i + 1) * 205, panelRect.localPosition.z);
			MapPanel newPanel = newPanelObject.GetComponent<MapPanel>();
			newPanel.SetValues(mapData[i]);
			mapPanels[i] = newPanelObject;
		}

		// Adjust height of content pane to fit #MapPanels + 1 panels
		int panelHeight = Mathf.Max(0, (mapData.Count - 2) * 205);
		RectTransform contentRect = contentPane.GetComponent<RectTransform>();
		contentRect.sizeDelta = new Vector2(0, panelHeight);
	}

	/**Read from a file to get a list of string arrays that contain a string
	 * representation of each attribute of a MapPanel.
	 */
	public List<string[]> GetMapData(string file) {

		StreamReader sr = GetNewFileReader(file);

		List<string[]> mapData = new List<string[]>();
		while(!sr.EndOfStream) {
			string[] map = new string[MapPanel.numAttributes];
			for(int i = 0; i < MapPanel.numAttributes; i++) {
				map[i] = sr.ReadLine();
			}
			sr.ReadLine();
			mapData.Add(map);
		}
		sr.Close();

		return mapData;
	}
	
	public void SaveMapData(string seed, string width = "30", string height = "30", string score = "N/A", string scoreDate = "N/A", string time = "N/A", string timeDate = "N/A") {

		StreamWriter sw = GetNewFileWriter(saveFile);

		sw.WriteLine(seed);
		sw.WriteLine(width);
		sw.WriteLine(height);
		sw.WriteLine(score);
		sw.WriteLine(scoreDate);
		sw.WriteLine(time);
		sw.WriteLine(timeDate);
		sw.WriteLine();

		sw.Close();
	}

	public void DeleteMapData(int index) {

		// Remove the specified map from the list
		List<string[]> newContents = GetMapData(saveFile);
		newContents.RemoveAt(index);

		// Overwrite the file with the new map list
		File.Delete(saveFile);
		StreamWriter sw = GetNewFileWriter(saveFile);
		foreach(string[] map in newContents) {
			foreach(string s in map) {
				sw.WriteLine(s);
			}
			sw.WriteLine();
		}
		sw.Close();

		// Regenerate panels
		CreateMapPanels(newContents);
	}
}
