using UnityEngine;
using System.Collections;

public class SlaughterHuntedAnimalOrder : BaseOrder {
    int progress = 0;
    Herd herd;

    public SlaughterHuntedAnimalOrder(ActorController a, Herd targetHerd) : base(a) {
        a.GetComponent<NeolithicObject>().statusString = "Killing snorgle";
        herd = targetHerd;
    }

    public override void DoStep() {
        progress++;
        if (progress >= 60) {
            if (herd.KillAnimal()) {
                string rtag = herd.resourceTag;
                //GameObject prefab = herd.resourcePrefab;
                //GameObject res = UnityEngine.Object.Instantiate(prefab);

                GameObject res = GameController.instance.CreateResourcePile(rtag, 1.0f);

                actor.PickupResource(res);
                this.completed = true;
            }
            else {
                progress /= 2;
            }
        }
    }
}