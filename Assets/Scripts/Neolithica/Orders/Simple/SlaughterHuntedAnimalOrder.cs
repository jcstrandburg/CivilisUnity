using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    public class SlaughterHuntedAnimalOrder : BaseOrder {
        private float progress = 0;
        private Herd herd;

        public SlaughterHuntedAnimalOrder(ActorController a, Herd targetHerd) : base(a) {
            a.GetComponent<NeolithicObject>().statusString = "Killing snorgle";
            herd = targetHerd;
        }

        public override void DoStep() {
            progress += Time.fixedDeltaTime;
            if (progress > 1.25f) {
                if (herd.KillAnimal()) {
                    var resourceType = herd.resourceType;
                    var res = actor.GameController.CreateResourcePile(resourceType, 1);
                    actor.PickupResource(res);
                    this.completed = true;
                }
                else {
                    progress /= 2.0f;
                }
            }
        }
    }
}