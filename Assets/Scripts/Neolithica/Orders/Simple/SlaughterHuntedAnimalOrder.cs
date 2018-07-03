using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class SlaughterHuntedAnimalOrder : BaseOrder {
        [SerializableMember(1)] private float progress;
        [SerializableMember(2)] private readonly Herd herd;

        public SlaughterHuntedAnimalOrder(IOrderable a, Herd targetHerd) : base() {
            a.GetComponent<NeolithicObject>().statusString = "Killing snorgle";
            herd = targetHerd;
        }

        public override void DoStep(IOrderable orderable) {
            progress += Time.fixedDeltaTime;
            if (progress > 1.25f) {
                if (herd.KillAnimal()) {
                    var res = orderable.GameController.CreateResourcePile(ResourceKind.Meat, 1);
                    orderable.PickupResource(res);
                    Completed = true;
                }
                else {
                    progress /= 2.0f;
                }
            }
        }
    }
}