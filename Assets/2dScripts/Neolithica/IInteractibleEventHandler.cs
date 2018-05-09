using UnityEngine.EventSystems;

namespace Neolithica {
    public interface IInteractibleEventHandler : IEventSystemHandler {
        void OnSelectClick(InteractibleEventData data);
        void OnContextClick(InteractibleEventData data);
    }
}