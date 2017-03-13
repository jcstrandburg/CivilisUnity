using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using AqlaSerializer;
using AqlaSerializer.Meta;
using UnityEngine;

namespace Tofu.Serialization {
    public class GameSerializer {
        public GameSerializer(IEnumerable<GameObject> prefabs, ITypeModelBuilder typeModelBuilder) {
            m_prefabs = prefabs.ToReadOnlyCollection();
            m_typeModelBuilder = typeModelBuilder;
        }

        public void Serialize<T>(Stream dest, T source) {
            TypeModel model = m_typeModelBuilder.BuildRuntimeTypeModel();
            model.SerializeWithLengthPrefix(dest, source, PrefixStyle.Base128, 0);
        }

        public T Deserialize<T>(Stream source) {
            TypeModel model = m_typeModelBuilder.BuildRuntimeTypeModel();
            var loadGameContext = new LoadGameContext(m_prefabs);
            StreamingContext context = new StreamingContext(StreamingContextStates.Persistence, loadGameContext);

            return (T)model.DeserializeWithLengthPrefix(source, null, typeof(T), PrefixStyle.Base128, 0, context);
        }

        private readonly ReadOnlyCollection<GameObject> m_prefabs;
        private readonly ITypeModelBuilder m_typeModelBuilder;
    }
}