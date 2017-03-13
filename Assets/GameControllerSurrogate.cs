using AqlaSerializer;
using Neolithica;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Logistics;
using Neolithica.UI;
using System.Collections.Generic;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(GameController))]
    public class GameControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool BoxActive { get; set; }
        [SerializableMember(3)] public bool AdditiveSelect { get; set; }
        [SerializableMember(4)] public DayCycleController DayCycleController { get; set; }
        [SerializableMember(5)] public float Spirit { get; set; }
        [SerializableMember(8)] public GroundController GroundController { get; set; }
        [SerializableMember(9)] public List<CommandType> ForbiddenActions { get; set; }
        [SerializableMember(10)] public List<NeolithicObject> Selected { get; set; }
        [SerializableMember(11)] public LogisticsManager LogisticsManager { get; set; }
        [SerializableMember(13)] public StatManager StatManager { get; set; }
        [SerializableMember(14)] public TechManager TechManager { get; set; }

        public static implicit operator GameControllerSurrogate(GameController value) {
            if (value == null)
                return null;

            return new GameControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                BoxActive = value.boxActive,
                AdditiveSelect = value.additiveSelect,
                DayCycleController = value.DayCycleController,
                Spirit = value.Spirit,
                GroundController = value.GroundController,
                ForbiddenActions = value.ForbiddenActions,
                Selected = value.selected,
                LogisticsManager = value.LogisticsManager,
                StatManager = value.StatManager,
                TechManager = value.TechManager,
            };
        }

        public static implicit operator GameController(GameControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            GameController x = surrogate.Resolver.Resolve<GameController>();
            x.boxActive = surrogate.BoxActive;
            x.additiveSelect = surrogate.AdditiveSelect;
            x.DayCycleController = surrogate.DayCycleController;
            x.Spirit = surrogate.Spirit;
            x.GroundController = surrogate.GroundController;
            x.ForbiddenActions = surrogate.ForbiddenActions;
            x.selected = surrogate.Selected;
            x.LogisticsManager = surrogate.LogisticsManager;
            x.StatManager = surrogate.StatManager;
            x.TechManager = surrogate.TechManager;

            return x;
        }
    }
}
