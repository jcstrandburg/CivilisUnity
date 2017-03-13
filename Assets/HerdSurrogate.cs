using AqlaSerializer;
using Neolithica.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(Herd))]
    public class HerdSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float NodeProgress { get; set; }
        [SerializableMember(3)] public float MigrateSpeed { get; set; }
        [SerializableMember(4)] public float RespawnDelay { get; set; }
        [SerializableMember(5)] public float RespawnProgress { get; set; }
        [SerializableMember(6)] public GameController GameController { get; set; }
        [SerializableMember(8)] public GameObject AnimalPrefab { get; set; }
        [SerializableMember(9)] public int CurrentNode { get; set; }
        [SerializableMember(10)] public int MaxSize { get; set; }
        [SerializableMember(11)] public List<AnimalController> Animals { get; set; }
        [SerializableMember(12)] public Vector3 Diff { get; set; }
        [SerializableMember(13)] public Vector3 RabbitPos { get; set; }
        [SerializableMember(14)] public Vector3[] Path { get; set; }

        public static implicit operator HerdSurrogate(Herd value) {
            if (value == null)
                return null;

            return new HerdSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                NodeProgress = value.nodeProgress,
                MigrateSpeed = value.migrateSpeed,
                RespawnDelay = value.respawnDelay,
                RespawnProgress = value.respawnProgress,
                GameController = value.GameController,
                AnimalPrefab = value.animalPrefab,
                CurrentNode = value.currentNode,
                MaxSize = value.maxSize,
                Animals = value.animals,
                Diff = value.diff,
                RabbitPos = value.rabbitPos,
                Path = value.path,
            };
        }

        public static implicit operator Herd(HerdSurrogate surrogate) {
            if (surrogate == null)
                return null;

            Herd x = surrogate.Resolver.Resolve<Herd>();
            x.nodeProgress = surrogate.NodeProgress;
            x.migrateSpeed = surrogate.MigrateSpeed;
            x.respawnDelay = surrogate.RespawnDelay;
            x.respawnProgress = surrogate.RespawnProgress;
            x.GameController = surrogate.GameController;
            x.animalPrefab = surrogate.AnimalPrefab;
            x.currentNode = surrogate.CurrentNode;
            x.maxSize = surrogate.MaxSize;
            x.animals = surrogate.Animals;
            x.diff = surrogate.Diff;
            x.rabbitPos = surrogate.RabbitPos;
            x.path = surrogate.Path;

            return x;
        }
    }
}
