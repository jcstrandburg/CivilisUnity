using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Testing order to transmute one resource to another
    /// </summary>
    [SerializableType]
    public class ConstructOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private ConstructionManager manager;

        public ConstructOrder(ActorController actor, NeolithicObject target) {
            manager = target.GetComponent<ConstructionManager>();
            GoToState(cGetConstructionJob, actor);
        }

        public override void Initialize(ActorController actor) {
            Resource r = actor.GetCarriedResource();
            if (r != null)
                actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(cGetConstructionJob,
                actor => new GetConstructionJobOrder(actor, manager),
                actor => {
                    if (manager.ConstructionFinished())
                        this.Completed = true;
                    else
                        GoToState(cFetchResource, actor);
                },
                null);
            CreateState(cFetchResource,
                actor => {
                    var res = actor.GetComponent<ConstructionReservation>();
                    return new FetchAvailableResourceOrder(actor, res.resourceResourceKind, 1);
                },
                actor => GoToState(cDepositResource, actor),
                null);
            CreateState(cDepositResource,
                actor => new CompleteConstructionReservation(actor, manager),
                actor => {
                    if (!manager.ConstructionFinished()) {
                        GoToState(cGetConstructionJob, actor);
                    } else {
                        this.Completed = true;
                    }
                },
                null);
        }

        private const string cDepositResource = "cDepositResource";
        private const string cFetchResource = "cFetchResource";
        private const string cGetConstructionJob = "getConstructionJob";
    }
}