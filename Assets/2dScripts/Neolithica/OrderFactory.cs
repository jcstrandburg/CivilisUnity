using System;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using Neolithica.Orders.Super;
using UnityEngine;

namespace Neolithica {
    public class OrderFactory : MonoBehaviour {

        /// <summary>
        /// Constructs an order from the orderTag against the given target, 
        /// and assigns it to all selected actors
        /// </summary>
        //public void IssueOrder(IReadOnlyList<GameObject> selected, CommandType orderTag, GameObject target) {
        //    var actors = selected.Select(go => go.GetComponent<ActorController>()).Where(a => a != null);
        //    foreach (var actor in actors) {
        //        BaseOrder newOrder;
        //        switch (orderTag) {
        //            case CommandType.ChopWood:
        //            case CommandType.MineGold:
        //            case CommandType.MineStone:
        //            case CommandType.Forage:
        //                newOrder = new HarvestFromReservoirOrder(actor, target);
        //                break;
        //            case CommandType.ChuckWood:
        //                newOrder = new TransmuteOrder(actor, target, ResourceKind.Wood, ResourceKind.Gold);
        //                break;
        //            case CommandType.Meditate:
        //                newOrder = new MeditateOrder();
        //                break;
        //            case CommandType.Hunt:
        //                newOrder = new HuntOrder(actor, target.GetComponentInParent<Herd>());
        //                break;
        //            case CommandType.Fish:
        //                newOrder = new FishOrder(actor, target);
        //                break;
        //            case CommandType.Construct:
        //                newOrder = new ConstructOrder(actor, target);
        //                break;
        //            case CommandType.TearDown:
        //                newOrder = new TearDownOrder(actor, target);
        //                break;
        //            case CommandType.ForestGarden:
        //                var prefab = (GameObject)Resources.Load("Buildings/ForestGarden");
        //                if (prefab == null) {
        //                    throw new InvalidOperationException("Can't find prefab");
        //                }
        //                newOrder = new UpgradeReservoirOrder(actor, target, prefab);
        //                break;
        //            default:
        //                throw new InvalidOperationException($"Unrecognized order tag {orderTag}");
        //        }

        //        Factory.InjectObject(newOrder);
        //        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
        //            actor.EnqueueOrder(newOrder);
        //        }
        //        else {
        //            actor.OverrideOrder(newOrder);
        //        }
        //    }
        //}
    }
}
