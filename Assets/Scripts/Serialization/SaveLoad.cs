using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// </summary>
//public static class SaveLoad {

//	//You may define any path you like, such as "c:/Saved Games"
//	//remember to use slashes instead of backslashes! ("/" instead of "\")
//	//Application.DataPath: http://docs.unity3d.com/ScriptReference/Application-dataPath.html
//	//Application.persistentDataPath: http://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
//	public static string saveGamePath = Application.persistentDataPath + "/Saved Games/";

//	public static void Save(SaveGame saveGame, SaveLoadContext context=null) {
//        StreamingContext ctx = new StreamingContext(StreamingContextStates.All, context);
//        SurrogateSelector ss = new SurrogateSelector();
//        AddSurrogates(ref ss, ctx);
//        BinaryFormatter bf = new BinaryFormatter(ss, ctx);
		
	
//		//Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
//		//You can also use any path you like
//		CheckPath(saveGamePath);
//        //Debug.Log(saveGamePath);

//        using (FileStream file = File.Create(saveGamePath + saveGame.savegameName + ".sav")) {
//            try {
//                bf.Serialize(file, saveGame);
//                Debug.Log("Saved Game: " + saveGame.savegameName);
//            }
//            catch (Exception e) {
//                Debug.Log("Failed to Serialize save game data!");
//                Debug.Log(e);
//            }
//        }
//	}	
	
//	public static SaveGame Load(string gameToLoad, SaveLoadContext context) {
//		if(File.Exists(saveGamePath + gameToLoad + ".sav")) {
//            StreamingContext ctx = new StreamingContext(StreamingContextStates.All, context);
//            SurrogateSelector ss = new SurrogateSelector();
//            AddSurrogates(ref ss, ctx);
//            BinaryFormatter bf = new BinaryFormatter(ss, ctx);
			
//			FileStream file = File.Open(saveGamePath + gameToLoad + ".sav", FileMode.Open);
//			SaveGame loadedGame = (SaveGame)bf.Deserialize(file);
//			file.Close();
//			Debug.Log("Loaded Game: " + loadedGame.savegameName);
//			return loadedGame;
//		}
//		else {
//			Debug.Log(gameToLoad + " does not exist!");
//			return null;
//		}
//	}
	
//	private static void AddSurrogates(ref SurrogateSelector ss, StreamingContext sContext) {
//		Vector3Surrogate Vector3_SS = new Vector3Surrogate();
//		ss.AddSurrogate(typeof(Vector3), sContext, Vector3_SS);
//		Texture2DSurrogate Texture2D_SS = new Texture2DSurrogate();
//		ss.AddSurrogate(typeof(Texture2D), sContext, Texture2D_SS);
//		ColorSurrogate Color_SS = new ColorSurrogate();
//		ss.AddSurrogate(typeof(Color), sContext, Color_SS);
//		GameObjectSurrogate GameObject_SS = new GameObjectSurrogate();
//		ss.AddSurrogate(typeof(GameObject), sContext, GameObject_SS);
//		TransformSurrogate Transform_SS = new TransformSurrogate();
//		ss.AddSurrogate(typeof(Transform), sContext, Transform_SS);
//		QuaternionSurrogate Quaternion_SS = new QuaternionSurrogate();
//		ss.AddSurrogate(typeof(Quaternion), sContext, Quaternion_SS);

//        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
//            if (type.GetCustomAttributes(typeof(CustomSerialize), true).Length > 0) {
//                //Debug.Log("Adding surrogate for type: " + type.Name);
//                CustomSerializedObjectSurrogate surrogate = new CustomSerializedObjectSurrogate();
//                ss.AddSurrogate(type, sContext, surrogate);
//            }
//        }
//    }

//	private static void CheckPath(string path) {
//		try 
//		{
//			// Determine whether the directory exists. 
//			if (Directory.Exists(path)) {
//				//Debug.Log("That path exists already.");
//				return;
//			}
			
//			// Try to create the directory.
//			Directory.CreateDirectory(path);
//			Debug.Log("The directory was created successfully at " + path);

//		} 
//		catch (Exception e) {
//			Debug.Log("The process failed: " + e.ToString());
//		} 
//		finally {}
//	}
//}
