using UnityEngine.EventSystems;

namespace Neolithica {
    public interface IHoverHandler : IEventSystemHandler {
        void HoverStart();
        void HoverEnd();
    }
}
