using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Simple order to seek, reserve, and extract resources from the given target
    /// </summary>
    [SerializableType]
    public class HarvestFromReservoirOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private NeolithicObject targetObj;
        [SerializableMember(2)]
        private Reservoir reservoir;
        [SerializableMember(3)]
        private ResourceReservation resourceReservation;

        public HarvestFromReservoirOrder(ActorController actor, NeolithicObject target) : base(actor) {
            targetObj = target;
            reservoir = target.GetComponent<Reservoir>();
            GoToState("seekTarget", actor);
        }

        public override void Initialize(ActorController actor) {
            Resource r = actor.GetCarriedResource();
            if (r != null && r.resourceKind == reservoir.resourceResourceKind) {
                GoToState("storeContents", actor);
            } else {
                actor.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState("seekTarget",
                actor => new SimpleMoveOrder(
                    actor, 
                    actor.GameController.SnapToGround(targetObj.transform.position)),
                actor => GoToState("reservationWait", actor),
                null);
            CreateState("reservationWait",
                actor => {
                    resourceReservation = reservoir.NewReservation(actor.gameObject, 1);
                    return new WaitForReservationOrder(actor, resourceReservation);
                },
                actor => GoToState("getResource", actor),
                null);
            CreateState("getResource",
                actor => new ExtractFromReservoirOrder(actor, resourceReservation),
                actor => {
                    resourceReservation = null;
                    GoToState("storeContents", actor);
                },
                null);
            CreateState("storeContents",
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState("seekTarget", actor),
                null);
        }

        public override void Cancel() {
            base.Cancel();
            if (resourceReservation) {
                resourceReservation.Released = true;
            }
        }
    }
}
