using System.Collections.Generic;

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// </summary>
[System.Serializable]
public class SaveGame {
	
	public string savegameName = "New SaveGame";
	public List<SceneObject> sceneObjects = new List<SceneObject>();

	public SaveGame() {

	}

	public SaveGame(string s, List<SceneObject> list) {
		savegameName = s;
		sceneObjects = list;
	}
}
