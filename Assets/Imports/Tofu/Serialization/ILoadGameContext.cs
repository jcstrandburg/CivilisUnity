using UnityEngine;

namespace Tofu.Serialization {
    public interface ILoadGameContext {
        GameObject MakeGameObject(string prefabName);
    }
}