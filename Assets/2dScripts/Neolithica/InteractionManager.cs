﻿using System;
using System.Collections.Generic;
using System.Linq;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using Neolithica.Orders.Super;
using Neolithica.UI;
using Tofu.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica {
    public class InteractionManager : MonoBehaviour, IMapClickHandler, IInteractibleEventHandler {

        [Inject] public GameUIController GameUIController;

        /// <summary>A list containing all selected Interactibles</summary>
        private List<Interactible> selected = new List<Interactible>();

        private bool boxActive;
        private bool isInAdditiveMode;
        private Vector3 boxStart, boxEnd;

        public void FixedUpdate() {
            selected.RemoveAll(x => x == null);
            isInAdditiveMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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
            GameUIController.HideContextMenu();
            UpdateSelection(new[] { data.Target });
        }

        public void OnContextClick(InteractibleEventData data) {
            // TODO: forbidden actions
            // var forbidden = new HashSet<CommandType>(ForbiddenActions);

            var clickee = data.Target;

            var selectedActions = GetSelectedAbilities();
            var availableActions = selectedActions
                .Intersect(clickee.actionProfile.targetActions)
                .ToArray();
            GameUIController.ShowContextMenu2D(availableActions, commandType => IssueOrderToSelected(commandType, clickee.gameObject));
        }

        private void IssueOrderToSelected(CommandType commandType, GameObject target) {
            var actors = selected.Select(go => go.GetComponent<IOrderable>()).Where(a => a != null);
            foreach (IOrderable actor in actors) {
                BaseOrder newOrder;
                switch (commandType) {
                case CommandType.ChopWood:
                case CommandType.MineGold:
                case CommandType.MineStone:
                case CommandType.Forage:
                    newOrder = new HarvestFromReservoirOrder(actor, target);
                    break;
                case CommandType.ChuckWood:
                    newOrder = new TransmuteOrder(actor, target, ResourceKind.Wood, ResourceKind.Gold);
                    break;
                case CommandType.Meditate:
                    newOrder = new MeditateOrder();
                    break;
                case CommandType.Hunt:
                    newOrder = new HuntOrder(actor, target.GetComponentInParent<Herd>());
                    break;
                case CommandType.Fish:
                    newOrder = new FishOrder(actor, target);
                    break;
                case CommandType.Construct:
                    newOrder = new ConstructOrder(actor, target);
                    break;
                case CommandType.TearDown:
                    newOrder = new TearDownOrder(actor, target);
                    break;
                case CommandType.ForestGarden:
                    var prefab = (GameObject)Resources.Load("Buildings/ForestGarden");
                    if (prefab == null) {
                        throw new InvalidOperationException("Can't find prefab");
                    }
                    newOrder = new UpgradeReservoirOrder(actor, target, prefab);
                    break;
                default:
                    throw new InvalidOperationException($"Unrecognized {nameof(CommandType)} {commandType}");
                }

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    actor.EnqueueOrder(newOrder);
                }
                else {
                    actor.OverrideOrder(newOrder);
                }
            }
        }

        private CommandType[] GetSelectedAbilities() {
            if (!selected.Any()) {
                return new CommandType[] {};
            }

            return selected
                .Select(s => s.actionProfile.abilities)
                .Aggregate((current, a) => a.Intersect(current).ToArray());
        }

        public void OnMapLeftClick(PointerEventData eventData) {
            GameUIController.HideContextMenu();
            StartBoxSelect();
        }

        public void OnMapRightClick(PointerEventData eventData) {
            GameUIController.HideContextMenu();
            Vector3 position = eventData.pointerCurrentRaycast.worldPosition;

            foreach (Actor actor in selected.Select(it => it.GetComponent<Actor>()).WhereNotNull()) {
                var order = new SimpleMoveOrder(actor, position);

                if (isInAdditiveMode)
                    actor.EnqueueOrder(order);
                else
                    actor.OverrideOrder(order);
            }
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

            if (selectables.Count > 1 || (isInAdditiveMode && (selected.Count + selectables.Count) > 1))
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
            var newSelected = (isInAdditiveMode ? selected : Enumerable.Empty<Interactible>())
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
