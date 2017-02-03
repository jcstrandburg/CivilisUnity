using UnityEngine;

public class MenuController: MonoBehaviour {

    [Inject]
    public MenuManager MenuManager { get; set; }

	public void PushMenuPrefab(GameObject prefab) {
        MenuManager.Instance.PushMenuPrefab(prefab);
    }

    public void PushMenuName(string name) {
        MenuManager.Instance.PushMenuName(name);
    }

    public void PopMenu() {
        MenuManager.Instance.PopMenu();
    }
}
