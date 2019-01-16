using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to generate spirit
    /// </summary>
    [SerializableType]
    public class MeditateOrder : BaseOrder {

        public override void DoStep(IOrderable orderable) {
            orderable.GameController.Spirit += 0.03f;
        }
    }
}
