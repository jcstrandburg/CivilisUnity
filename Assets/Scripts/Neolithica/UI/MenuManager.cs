using System;
using System.Collections.Generic;
using Neolithica.Extensions;
using Neolithica.MonoBehaviours;
using UnityEngine;
using UnityEngine.Events;

namespace Neolithica.UI {
    public class MenuManager : MonoBehaviour {
        public string startMenu = null;//the menu that gets pushed on to the stack when the scene starts
        public UnityEvent onEmpty;

        private readonly Stack<GameObject> menuStack = new Stack<GameObject>();

        private static MenuManager instance = null;
        public static MenuManager Instance => UnityExtensions.CacheComponent(ref instance, FindObjectOfType<MenuManager>);

        [Inject]
        public GameController GameController { get; set; }

        private Canvas canvas;
        private Canvas Canvas => this.CacheComponent(ref canvas, FindObjectOfType<Canvas>);

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
            GameObject newMenu = GameController.Factory.Instantiate(prefab);
            newMenu.SetActive(true);
            newMenu.transform.SetParent(Canvas.transform);
            newMenu.transform.position = new Vector3((float)Screen.width / 2, (float)Screen.height / 2);
            menuStack.Push(newMenu);
        }

        public void PushMenuName(string name) {
            GameObject prefab = (GameObject)Resources.Load($"Menus/{name}");
            if (prefab == null) {
                throw new ArgumentException($"Cannot load menu {name}", nameof(name));
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
            } else if (onEmpty != null) {
                Debug.Log("Invoking onEmpty");
                onEmpty.Invoke();
            }
        }
    }
}
