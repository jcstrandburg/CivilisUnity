using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica {
    public class InteractionManager : MonoBehaviour, IMapClickHandler, IInteractibleEventHandler {

        /// <summary>A list containing all selected Interactibles</summary>
        private List<Interactible> selected = new List<Interactible>();

        private bool boxActive;
        private bool additiveSelect;
        private Vector3 boxStart, boxEnd;

        public void FixedUpdate() {
            selected.RemoveAll(x => x == null);
            additiveSelect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public void Update() {
            if (boxActive)
                UpdateBoxSelect();

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                StartBoxSelect();
        }

        public void OnGUI() {
            if (!boxActive) return;

            Vector2 start = boxStart;
            Vector2 end = boxEnd;

            if (end.x < start.x) {
                float temp = end.x;
                end = new Vector2(start.x, end.y);
                start = new Vector2(temp, start.y);
            }
            if (end.y > start.y) {
                float temp = end.y;
                end = new Vector2(end.x, start.y);
                start = new Vector2(start.x, temp);
            }

            GUI.Box(new Rect(start.x, Screen.height - start.y, end.x - start.x, start.y - end.y), "");
        }

        public void OnSelectClick(InteractibleEventData data) {
            UpdateSelection(new[] { data.Target });
        }

        public void OnContextClick(InteractibleEventData data) {
            throw new System.NotImplementedException();
        }

        public void OnMapLeftClick(PointerEventData eventData) {
            StartBoxSelect();
        }

        public void OnMapRightClick(PointerEventData eventData) {
            throw new System.NotImplementedException();
        }

        private void StartBoxSelect() {
            boxEnd = boxStart = Input.mousePosition;
            boxActive = true;
        }

        private void UpdateBoxSelect() {
            boxEnd = Input.mousePosition;
            Vector2 start = boxStart;
            Vector2 end = boxEnd;

            if ( end.x < start.x) {
                float temp = end.x;
                end = new Vector2(start.x, end.y);
                start = new Vector2(temp, start.y);
            }
            if ( end.y > start.y ) {
                float temp = end.y;
                end = new Vector2(end.x, start.y);
                start = new Vector2(start.x, temp);
            }
            var screenRect = new Rect (start.x, Screen.height-start.y, end.x-start.x, start.y-end.y);

            var selectables = new List<Interactible>();
            foreach (Interactible no in FindObjectsOfType<Interactible>().Where(n => n.Selectability != Selectability.Unselectable)) {
                Vector2 loc = Camera.main.WorldToScreenPoint(no.transform.position);
                loc = new Vector2(loc.x, Screen.height-loc.y);
                no.HoverEnd();
                if (screenRect.Contains(loc)) {
                    selectables.Add(no);
                }
            }

            if (selectables.Count > 1 || (additiveSelect && (selected.Count + selectables.Count) > 1))
                selectables.RemoveAll(obj => obj.Selectability != Selectability.Multiselectable);

            foreach (Interactible obj in selectables)
                obj.HoverStart();

            if (!Input.GetMouseButton(0))
                EndBoxSelect(selectables);
        }

        private void EndBoxSelect(IReadOnlyList<Interactible> selectables) {
            boxActive = false;

            foreach (Interactible x in selectables)
                x.HoverEnd();

            UpdateSelection(selectables);
        }

        private void UpdateSelection(IEnumerable<Interactible> selectables) {
            var newSelected = (additiveSelect ? selected : Enumerable.Empty<Interactible>())
                .Concat(selectables)
                .ToList();

            newSelected = (newSelected.Count > 1 ?
                    newSelected.Where(x => x.Selectability == Selectability.Multiselectable) :
                    newSelected)
                .ToList();

            foreach (Interactible noLongerSelected in selected.Except(newSelected))
                noLongerSelected.Deselect();

            foreach (Interactible newlySelected in newSelected.Except(selected))
                newlySelected.Select();

            selected = newSelected.ToList();
        }
    }
}
