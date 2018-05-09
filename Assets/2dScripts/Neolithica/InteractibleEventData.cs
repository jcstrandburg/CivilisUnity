using UnityEngine.EventSystems;

namespace Neolithica {
    public class InteractibleEventData : BaseEventData {
        public InteractibleEventData(EventSystem eventSystem, Interactible target) : base(eventSystem) {
            Target = target;
        }

        public Interactible Target { get; }
    }
}