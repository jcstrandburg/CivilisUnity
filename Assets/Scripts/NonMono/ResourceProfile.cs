using System;

[Serializable]
public class ResourceProfile : ICloneable {

    public ResourceProfile(string rt, double a) {
        resourceTag = rt;
        amount = a;
    }

    public object Clone() {
        return MemberwiseClone();
    }

    public string resourceTag;
    public double amount;
}