using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(CameraController))]
    public class CameraControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float ZoomLevel { get; set; }
        [SerializableMember(3)] public float XRotateLevel { get; set; }
        [SerializableMember(4)] public float StrategicRotation { get; set; }

        public static implicit operator CameraControllerSurrogate(CameraController value) {
            if (value == null)
                return null;

            return new CameraControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                ZoomLevel = value.zoomLevel,
                XRotateLevel = value.xRotateLevel,
                StrategicRotation = value.strategicRotation,
            };
        }

        public static implicit operator CameraController(CameraControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            CameraController x = surrogate.Resolver.Resolve<CameraController>();
            x.zoomLevel = surrogate.ZoomLevel;
            x.xRotateLevel = surrogate.XRotateLevel;
            x.strategicRotation = surrogate.StrategicRotation;

            return x;
        }
    }
}
