using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Super;
namespace Neolithica.Orders.Simple {
    /// <summary>
    /// The base for all actor orders
    /// </summary>
    [SerializableType]
    [SerializeDerivedType(10, typeof(CatchFishOrder))]
    [SerializeDerivedType(11, typeof(CompleteConstructionReservation))]
    [SerializeDerivedType(12, typeof(ConvertResourceOrder))]
    [SerializeDerivedType(13, typeof(DoBuildingUpgrade))]
    [SerializeDerivedType(14, typeof(DumpCarriedResourceOrder))]
    [SerializeDerivedType(15, typeof(ExtractFromReservoirOrder))]
    [SerializeDerivedType(16, typeof(IdleOrder))]
    [SerializeDerivedType(17, typeof(MeditateOrder))]
    [SerializeDerivedType(18, typeof(ReserveStorageOrder))]
    [SerializeDerivedType(19, typeof(ReserveWarehouseContentsOrder))]
    [SerializeDerivedType(20, typeof(SimpleMoveOrder))]
    [SerializeDerivedType(21, typeof(SimpleWithdrawOrder))]
    [SerializeDerivedType(22, typeof(SlaughterHuntedAnimalOrder))]
    [SerializeDerivedType(23, typeof(StoreReservationOrder))]
    [SerializeDerivedType(24, typeof(TearDownOrder))]
    [SerializeDerivedType(25, typeof(GetConstructionJobOrder))]
    [SerializeDerivedType(50, typeof(StatefulSuperOrder))]
    public abstract class BaseOrder {
        public bool Completed;
        public bool Cancelled;
        public bool Failed;
        public bool Initialized;

        public bool Done => Completed || Cancelled || Failed;

        protected BaseOrder() {
            Completed = Cancelled = Failed = false;
        }

        public void Update(IOrderable orderable) {
            if (!Initialized) {
                Initialize(orderable);
                Initialized = true;
            }
            if (!Done) {
                DoStep(orderable);
            }
        }

        public virtual void Initialize(IOrderable orderable) {
        }

        /// <summary>
        /// Does a single step for this order
        /// </summary>
        /// <param name="orderable"></param>
        public abstract void DoStep(IOrderable orderable);

        /// <summary>
        /// Cancels this order, freeing any resources as appropriate
        /// </summary>
        public virtual void Cancel() {
            Cancelled = true;
        }

        public virtual void Pause() {
        }

        public virtual void Resume() {
        }
    }
}
