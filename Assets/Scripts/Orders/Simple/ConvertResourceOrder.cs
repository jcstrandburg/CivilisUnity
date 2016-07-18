using UnityEngine;
using System.Collections;

/// <summary>
/// Simple order to convert a carried resource of one type to another type
/// </summary>
public class ConvertResourceOrder : BaseOrder {
    Resource sourceResource;
    string toTag;

    public ConvertResourceOrder(ActorController a, string fromTag, string toTag) : base(a) {
        Resource r = a.GetCarriedResource();
        if (r.typeTag != fromTag) {
            Debug.Log("Actor does not have resource " + fromTag + " to convert");
            this.failed = true;
        }
        sourceResource = r;
        this.toTag = toTag;
    }

    public override void DoStep() {
        GameObject newResource = GameController.instance.CreateResourcePile(toTag, 1.0f);
        Resource r = newResource.GetComponent<Resource>();
        r.amount = sourceResource.amount;
        actor.PickupResource(newResource);
        sourceResource.transform.SetParent(null);
        UnityEngine.Object.Destroy(sourceResource.gameObject);
        this.completed = true;
    }
}