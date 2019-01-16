﻿using UnityEngine;

namespace Neolithica.Orders.Simple {
    public class IdleOrder : BaseOrder {
        private readonly Vector3 center;
        private Vector3 targetPosition;

        private const float range = 5.0f;

        public override string StatusString => "Idling";

        public IdleOrder(IOrderable actor) {
            center = targetPosition = actor.Transform.position;
        }

        public override void DoStep(IOrderable orderable) {
            if (orderable.MoveTowards(targetPosition, 0.08f)) {
                targetPosition = center + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
                targetPosition = orderable.GameController.SnapToGround(targetPosition);
            }
        }
    }
}
