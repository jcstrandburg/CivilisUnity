using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to generate spirit
    /// </summary>
    [SerializableType]
    public class MeditateOrder : BaseOrder {
        public MeditateOrder(ActorController a, NeolithicObject target) : base(a) {
        }

        public override void DoStep() {
            Actor.GameController.Spirit += 0.03f;
        }
    }
}
