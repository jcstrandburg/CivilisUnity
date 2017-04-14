using System;
using System.Collections.Generic;
using Neolithica.MonoBehaviours;
using UnityEngine;
using UnityEngine.Events;

namespace Neolithica.UI {
    public class MenuManager : MonoBehaviour {
        public string startMenu = null;//the menu that gets pushed on to the stack when the scene starts
        public UnityEvent onEmpty;

        private Stack<GameObject> menuStack = new Stack<GameObject>();

        private static MenuManager instance = null;
        public static MenuManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<MenuManager>();
                }
                return instance;
            }
        }

        [Inject]
        public GameController GameController { get; set; }

        private Canvas _canvas;
        private Canvas canvas {
            get {
                if (_canvas == null) {
                    _canvas = FindObjectOfType<Canvas>();
                }
                return _canvas;
            }
        }

        public void Start() {
            if (!string.IsNullOrEmpty(startMenu)) {
                PushMenuName(startMenu);
            }
        }

        public void PushMenuPrefab(GameObject prefab) {
            if (menuStack.Count > 0) {
                GameObject currentMenu = menuStack.Peek();
                currentMenu.SetActive(false);
            }
            var newMenu = GameController.Factory.Instantiate(prefab);
            newMenu.SetActive(true);
            newMenu.transform.SetParent(canvas.transform);
            newMenu.transform.position = new Vector3((float)Screen.width / 2, (float)Screen.height / 2);
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
}
