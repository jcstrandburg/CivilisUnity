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
            GoToState("getConstructionJob", actor);
        }

        public override void Initialize(ActorController actor) {
            Resource r = actor.GetCarriedResource();
            if (r != null)
                actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState("getConstructionJob",
                actor => new GetConstructionJobOrder(actor, manager),
                actor => {
                    if (manager.ConstructionFinished())
                        this.Completed = true;
                    else
                        GoToState("fetchResource", actor);
                },
                null);
            CreateState("fetchResource",
                actor => {
                    var res = actor.GetComponent<ConstructionReservation>();
                    return new FetchAvailableResourceOrder(actor, res.resourceResourceKind, 1);
                },
                actor => GoToState("depositResource", actor),
                null);
            CreateState("depositResource",
                actor => new CompleteConstructionReservation(actor, manager),
                actor => {
                    if (!manager.ConstructionFinished()) {
                        GoToState("getConstructionJob", actor);
                    } else {
                        this.Completed = true;
                    }
                },
                null);
        }
    }
}