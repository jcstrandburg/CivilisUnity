using UnityEngine.EventSystems;

namespace Neolithica {
    public interface IGameClickHandler : IEventSystemHandler {
        void OnSelectClick();
        void OnContextClick();
    }
}