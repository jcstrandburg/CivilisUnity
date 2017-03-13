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

        public HarvestFromReservoirOrder(ActorController a, NeolithicObject target) : base(a) {
            targetObj = target;
            reservoir = target.GetComponent<Reservoir>();
            GoToState("seekTarget");
        }

        public override void Initialize() {
            Resource r = Actor.GetCarriedResource();
            if (r != null && r.resourceKind == reservoir.resourceResourceKind) {
                GoToState("storeContents");
            } else {
                Actor.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState("seekTarget",
                () => new SimpleMoveOrder(
                    Actor, 
                    Actor.GameController.SnapToGround(targetObj.transform.position)),
                () => GoToState("reservationWait"),
                null);
            CreateState("reservationWait",
                () => {
                    resourceReservation = reservoir.NewReservation(Actor.gameObject, 1);
                    return new WaitForReservationOrder(Actor, resourceReservation);
                },
                () => GoToState("getResource"),
                null);
            CreateState("getResource",
                () => new ExtractFromReservoirOrder(Actor, resourceReservation),
                () => {
                    resourceReservation = null;
                    GoToState("storeContents");
                },
                null);
            CreateState("storeContents",
                () => new StoreCarriedResourceOrder(Actor),
                () => GoToState("seekTarget"),
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
