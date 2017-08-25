using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(CameraController))]
    public class CameraControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float ZoomLevel { get; set; }
        [SerializableMember(3)] public float XRotateLevel { get; set; }

        public static implicit operator CameraControllerSurrogate(CameraController value) {
            if (value == null)
                return null;

            return new CameraControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                ZoomLevel = SurrogateHelper.GetFieldValue<CameraController, float>(value, "zoomLevel"),
                XRotateLevel = SurrogateHelper.GetFieldValue<CameraController, float>(value, "xRotateLevel"),
            };
        }

        public static implicit operator CameraController(CameraControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            CameraController x = surrogate.Resolver.Resolve<CameraController>();
            SurrogateHelper.SetFieldValue(x, "zoomLevel", surrogate.ZoomLevel);
            SurrogateHelper.SetFieldValue(x, "xRotateLevel", surrogate.XRotateLevel);

            return x;
        }
    }
}
