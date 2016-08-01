using UnityEngine;
using System.Collections;

public class MenuController: MonoBehaviour {
	public void PushMenuPrefab(GameObject prefab) {
        MenuManager.instance.PushMenuPrefab(prefab);
    }

    public void PushMenuName(string name) {
        MenuManager.instance.PushMenuName(name);
    }

    public void PopMenu() {
        MenuManager.instance.PopMenu();
    }
}
