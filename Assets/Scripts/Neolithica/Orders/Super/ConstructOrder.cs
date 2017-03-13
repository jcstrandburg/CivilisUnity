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

        public ConstructOrder(ActorController a, NeolithicObject target) : base(a) {
            manager = target.GetComponent<ConstructionManager>();
            GoToState("getConstructionJob");
        }

        public override void Initialize() {
            Resource r = Actor.GetCarriedResource();
            if (r != null)
                Actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState("getConstructionJob",
                () => new GetConstructionJobOrder(Actor, manager),
                () => {
                    if (manager.ConstructionFinished())
                        this.Completed = true;
                    else
                        GoToState("fetchResource");
                },
                null);
            CreateState("fetchResource",
                () => {
                    var res = Actor.GetComponent<ConstructionReservation>();
                    return new FetchAvailableResourceOrder(Actor, res.resourceResourceKind, 1);
                },
                () => GoToState("depositResource"),
                null);
            CreateState("depositResource",
                () => new CompleteConstructionReservation(Actor, manager),
                () => {
                    if (!manager.ConstructionFinished()) {
                        GoToState("getConstructionJob");
                    } else {
                        this.Completed = true;
                    }
                },
                null);
        }
    }
}