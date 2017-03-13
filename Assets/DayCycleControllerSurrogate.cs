using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(DayCycleController))]
    public class DayCycleControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float Daytime { get; set; }
        [SerializableMember(3)] public float Daylength { get; set; }
        [SerializableMember(4)] public float X { get; set; }
        [SerializableMember(5)] public float Y { get; set; }
        [SerializableMember(6)] public float X2 { get; set; }
        [SerializableMember(7)] public float X3 { get; set; }
        [SerializableMember(8)] public float LightFalloff { get; set; }
        [SerializableMember(9)] public float SunIntensity { get; set; }
        [SerializableMember(10)] public float MoonIntensity { get; set; }
        [SerializableMember(11)] public float MinAmbient { get; set; }
        [SerializableMember(12)] public float MaxAmbient { get; set; }
        [SerializableMember(13)] public float MinR { get; set; }
        [SerializableMember(14)] public float MaxR { get; set; }
        [SerializableMember(15)] public float MinG { get; set; }
        [SerializableMember(16)] public float MaxG { get; set; }
        [SerializableMember(17)] public float MinB { get; set; }
        [SerializableMember(18)] public float MaxB { get; set; }

        public static implicit operator DayCycleControllerSurrogate(DayCycleController value) {
            if (value == null)
                return null;

            return new DayCycleControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Daytime = value.daytime,
                Daylength = value.daylength,
                X = value.x,
                Y = value.y,
                X2 = value.x2,
                X3 = value.x3,
                LightFalloff = value.lightFalloff,
                SunIntensity = value.sunIntensity,
                MoonIntensity = value.moonIntensity,
                MinAmbient = value.minAmbient,
                MaxAmbient = value.maxAmbient,
                MinR = value.minR,
                MaxR = value.maxR,
                MinG = value.minG,
                MaxG = value.maxG,
                MinB = value.minB,
                MaxB = value.maxB,
            };
        }

        public static implicit operator DayCycleController(DayCycleControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            DayCycleController x = surrogate.Resolver.Resolve<DayCycleController>();
            x.daytime = surrogate.Daytime;
            x.daylength = surrogate.Daylength;
            x.x = surrogate.X;
            x.y = surrogate.Y;
            x.x2 = surrogate.X2;
            x.x3 = surrogate.X3;
            x.lightFalloff = surrogate.LightFalloff;
            x.sunIntensity = surrogate.SunIntensity;
            x.moonIntensity = surrogate.MoonIntensity;
            x.minAmbient = surrogate.MinAmbient;
            x.maxAmbient = surrogate.MaxAmbient;
            x.minR = surrogate.MinR;
            x.maxR = surrogate.MaxR;
            x.minG = surrogate.MinG;
            x.maxG = surrogate.MaxG;
            x.minB = surrogate.MinB;
            x.maxB = surrogate.MaxB;

            return x;
        }
    }
}
