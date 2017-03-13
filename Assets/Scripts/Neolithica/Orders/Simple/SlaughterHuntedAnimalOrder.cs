using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class SlaughterHuntedAnimalOrder : BaseOrder {
        [SerializableMember(1)]
        private float progress = 0;
        [SerializableMember(2)]
        private Herd herd;

        public SlaughterHuntedAnimalOrder(ActorController a, Herd targetHerd) : base(a) {
            a.GetComponent<NeolithicObject>().statusString = "Killing snorgle";
            herd = targetHerd;
        }

        public override void DoStep() {
            progress += Time.fixedDeltaTime;
            if (progress > 1.25f) {
                if (herd.KillAnimal()) {
                    var res = Actor.GameController.CreateResourcePile(ResourceKind.Meat, 1);
                    Actor.PickupResource(res);
                    this.Completed = true;
                }
                else {
                    progress /= 2.0f;
                }
            }
        }
    }
}