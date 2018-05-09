using UnityEngine.EventSystems;

namespace Neolithica {
    public interface IMapClickHandler : IEventSystemHandler {
        void OnMapLeftClick(PointerEventData eventData);
        void OnMapRightClick(PointerEventData eventData);
    }
}