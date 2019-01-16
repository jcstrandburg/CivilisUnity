using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Testing order to transmute one resource to another
    /// </summary>
    [SerializableType]
    public class ConstructOrder : StatefulSuperOrder {
        [SerializableMember(1)] private readonly ConstructionManager manager;

        public ConstructOrder(IOrderable actor, GameObject target) {
            manager = target.GetComponent<ConstructionManager>();
            GoToState(cGetConstructionJob, actor);
        }

        public override void Initialize(IOrderable orderable) {
            Resource r = orderable.GetCarriedResource();
            if (r != null)
                orderable.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(cGetConstructionJob,
                actor => new GetConstructionJobOrder(actor, manager),
                actor => {
                    if (manager.ConstructionFinished())
                        Completed = true;
                    else
                        GoToState(cFetchResource, actor);
                },
                null);
            CreateState(cFetchResource,
                actor => {
                    var res = actor.GetComponent<ConstructionReservation>();
                    return new FetchAvailableResourceOrder(actor, res.resourceKind, 1);
                },
                actor => GoToState(cDepositResource, actor),
                null);
            CreateState(cDepositResource,
                actor => new CompleteConstructionReservation(manager),
                actor => {
                    if (!manager.ConstructionFinished()) {
                        GoToState(cGetConstructionJob, actor);
                    } else {
                        Completed = true;
                    }
                },
                null);
        }

        private const string cDepositResource = "cDepositResource";
        private const string cFetchResource = "cFetchResource";
        private const string cGetConstructionJob = "getConstructionJob";
    }
}