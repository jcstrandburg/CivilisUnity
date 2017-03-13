using System;
using System.Runtime.Serialization;
using AqlaSerializer;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType]
    public class GameObjectSurrogate {
        [SerializableMember(1)]
        public string Name { get; set; }
        [SerializableMember(2)]
        public string PrefabId { get; set; }
        [SerializableMember(3)]
        public Vector3 Position { get; set; }
        [SerializableMember(4)]
        public Vector3 LocalScale { get; set; }
        [SerializableMember(5)]
        public Quaternion Rotation { get; set; }
        [SerializableMember(6)]
        public int Id { get; set; }
        [SerializableMember(7)]
        public bool HasParent { get; set; }
        [SerializableMember(8)]
        public GameObject Parent { get; set; }

        private ILoadGameContext context;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            this.context = context.Context as ILoadGameContext;
        }

        public static implicit operator GameObjectSurrogate(GameObject value) {
            if (value == null)
                return null;

            Savable savable = value.GetComponent<Savable>();
            if (savable == null)
                throw new InvalidOperationException(string.Format("Attempting to save object '{0}' with no Savable component", value.name));

            if (string.IsNullOrEmpty(savable.PrefabId))
                throw new InvalidOperationException("Attempting to save object with no PrefabId");

            var surrogate = new GameObjectSurrogate {
                Name = value.name,
                PrefabId = savable.PrefabId,
                Position = value.transform.position,
                LocalScale = value.transform.localScale,
                Rotation = value.transform.rotation, //TODO maybe use localRotation instead?
                Id = value.GetInstanceID(),
                HasParent = value.transform.parent != null,
                Parent = value.transform.parent != null ? value.transform.parent.gameObject : null,
            };
            return surrogate;
        }

        public static implicit operator GameObject(GameObjectSurrogate surrogate) {
            if (surrogate == null)
                return null;

            GameObject go = surrogate.context.MakeGameObject(surrogate.PrefabId);

            go.name = surrogate.Name;
            go.transform.position = surrogate.Position;
            go.transform.localScale = surrogate.LocalScale;
            go.transform.rotation = surrogate.Rotation;

            if (surrogate.Parent != null)
                go.transform.SetParent(surrogate.Parent.transform);

            return go;
        }
    }
}