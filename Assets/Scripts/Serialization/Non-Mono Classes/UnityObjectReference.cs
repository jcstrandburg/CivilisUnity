/// <summary>
/// Stores a reference to a GameObject or MonoBehaviour with an associated
/// ObjectIdentifier component so we can rebuild the original reference after deserialization
/// </summary>
[System.Serializable]
public class UnityObjectReference {
    public string refID;
    public string typeName;
    public string refName;

    public UnityObjectReference(string refID, string typeName, string refName) {
        this.refID = refID;
        this.typeName = typeName;
        this.refName = refName;
    }
}