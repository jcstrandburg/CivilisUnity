using System;

[System.Serializable]
public class UnityObjectReference {
    public string refID;
    public string typeName;

    public UnityObjectReference(string refID, string typeName) {
        this.refID = refID;
        this.typeName = typeName;
    }
}