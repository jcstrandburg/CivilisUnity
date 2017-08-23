using System;
using System.Collections.Generic;
using Neolithica.Extensions;
using Neolithica.MonoBehaviours;
using UnityEngine;
using UnityEngine.Events;

namespace Neolithica.UI {
    public class MenuManager : MonoBehaviour {
        public string StartMenu = null;//the menu that gets pushed on to the stack when the scene starts
        public UnityEvent OnEmpty;

        private readonly Stack<GameObject> menuStack = new Stack<GameObject>();

        public static MenuManager Instance => UnityExtensions.CacheComponent(ref instance, FindObjectOfType<MenuManager>);
        private static MenuManager instance = null;

        [Inject]
        public GameController GameController { get; set; }

        private Canvas Canvas => this.CacheComponent(ref canvas, FindObjectOfType<Canvas>);
        private Canvas canvas;

        public void Start() {
            if (!string.IsNullOrEmpty(StartMenu)) {
                PushMenuName(StartMenu);
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

        public void PushMenuName(string menuName) {
            GameObject prefab = (GameObject)Resources.Load($"Menus/{menuName}");
            if (prefab == null) {
                throw new ArgumentException($"Cannot load menu {menuName}", nameof(menuName));
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
            } else if (OnEmpty != null) {
                Debug.Log("Invoking onEmpty");
                OnEmpty.Invoke();
            }
        }
    }
}
