using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Simple order to seek, reserve, and extract resources from the given target
    /// </summary>
    [SerializableType]
    public class HarvestFromReservoirOrder : StatefulSuperOrder {
        [SerializableMember(1)] private readonly GameObject targetObj;
        [SerializableMember(2)] private readonly Reservoir reservoir;
        [SerializableMember(3)] private ResourceReservation resourceReservation;

        public HarvestFromReservoirOrder(ActorController actor, GameObject target) {
            targetObj = target;
            reservoir = target.GetComponent<Reservoir>();
            GoToState(cSeekTarget, actor);
        }

        public override void Initialize(ActorController actor) {
            Resource r = actor.GetCarriedResource();
            if (r != null && r.resourceKind == reservoir.resourceKind) {
                GoToState(cStoreContents, actor);
            } else {
                actor.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState(cSeekTarget,
                actor => new SimpleMoveOrder(
                    actor, 
                    actor.GameController.SnapToGround(targetObj.transform.position)),
                actor => GoToState(cReservationWait, actor),
                null);
            CreateState(cReservationWait,
                actor => {
                    resourceReservation = reservoir.NewReservation(actor.gameObject, 1);
                    return new WaitForReservationOrder(actor, resourceReservation);
                },
                actor => GoToState(cGetResource, actor),
                null);
            CreateState(cGetResource,
                actor => new ExtractFromReservoirOrder(actor, resourceReservation),
                actor => {
                    resourceReservation = null;
                    GoToState(cStoreContents, actor);
                },
                null);
            CreateState(cStoreContents,
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState(cSeekTarget, actor),
                null);
        }

        public override void Cancel() {
            base.Cancel();
            if (resourceReservation) {
                resourceReservation.Released = true;
            }
        }

        private const string cSeekTarget = "seekTarget";
        private const string cStoreContents = "storeContents";
        private const string cReservationWait = "reservationWait";
        private const string cGetResource = "getResource";
    }
}
