using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour {
    public string startMenu = null;//the menu that gets pushed on to the stack when the scene starts
    public UnityEvent onEmpty;

    private Stack<GameObject> menuStack = new Stack<GameObject>();

    private static MenuManager _instance = null;

    public static MenuManager instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<MenuManager>();
            }
            return _instance;
        }
    }

    public void Start() {
        if (startMenu != null && startMenu.Length > 0) {
            PushMenuName(startMenu);
        }
    }

    public void PushMenuPrefab(GameObject prefab) {
        if (menuStack.Count > 0) {
            GameObject currentMenu = menuStack.Peek();
            currentMenu.SetActive(false);
        }
        GameObject newMenu = Instantiate(prefab);
        Canvas c = FindObjectOfType<Canvas>();
        newMenu.SetActive(true);
        newMenu.transform.SetParent(c.transform);
        newMenu.transform.position = new Vector3(Screen.width / 2, Screen.height / 2);
        menuStack.Push(newMenu);
    }

    public void PushMenuName(string name) {
        GameObject prefab = (GameObject)Resources.Load("Menus/" + name);
        if (prefab == null) {
            throw new ArgumentException("Cannot load menu "+name);
        }
        PushMenuPrefab(prefab);
    }

    public void PopMenu() {
        GameObject oldMenu = menuStack.Pop();
        oldMenu.SetActive(false);
        Destroy(oldMenu);
        if (menuStack.Count > 0) {
            GameObject currentMenu = menuStack.Peek();
            currentMenu.SetActive(true);
        } else {
            Debug.Log("Empty");
            if (onEmpty != null) {
                Debug.Log("Invoking");
                onEmpty.Invoke();
            }
        }
    }
}
