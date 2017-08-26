using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(GroundController))]
    public class GroundControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float SnowThreshold { get; set; }
        [SerializableMember(3)] public float StoneThreshhold { get; set; }
        [SerializableMember(4)] public float GrassThreshold { get; set; }
        [SerializableMember(5)] public float WaterLevel { get; set; }
        [SerializableMember(6)] public GameController GameController { get; set; }
        [SerializableMember(7)] public NewGameSettings Settings { get; set; }
        [SerializableMember(8)] public TerrainSettings TerrainSettings { get; set; }

        public static implicit operator GroundControllerSurrogate(GroundController value) {
            if (value == null)
                return null;

            return new GroundControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                SnowThreshold = value.snowThreshold,
                StoneThreshhold = value.stoneThreshhold,
                GrassThreshold = value.grassThreshold,
                WaterLevel = value.waterLevel,
                GameController = value.GameController,
                Settings = value.newGameSettings,
                TerrainSettings = value.terrainSettings,
            };
        }

        public static implicit operator GroundController(GroundControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            GroundController x = surrogate.Resolver.Resolve<GroundController>();
            x.snowThreshold = surrogate.SnowThreshold;
            x.stoneThreshhold = surrogate.StoneThreshhold;
            x.grassThreshold = surrogate.GrassThreshold;
            x.waterLevel = surrogate.WaterLevel;
            x.GameController = surrogate.GameController;
            x.newGameSettings = surrogate.Settings;
            x.terrainSettings = surrogate.TerrainSettings;

            return x;
        }
    }
}
