using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica {
    public class Terrain2D : MonoBehaviour, IPointerDownHandler {
        public void OnPointerDown(PointerEventData eventData) {
            switch (eventData.button) {
            case PointerEventData.InputButton.Left:
                ExecuteEvents.ExecuteHierarchy<IMapClickHandler>(
                    gameObject,
                    eventData,
                    (x,data) => x.OnMapLeftClick((PointerEventData) data));
                break;
            case PointerEventData.InputButton.Right:
            ExecuteEvents.ExecuteHierarchy<IMapClickHandler>(
                gameObject,
                eventData,
                (x,data) => x.OnMapRightClick((PointerEventData) data));
            break;
            }
        }
    }
}
