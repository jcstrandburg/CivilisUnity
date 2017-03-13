using Neolithica.DependencyInjection;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Logistics;
using Neolithica.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Neolithica {
    public class MainGameFactory : GameFactoryBase {
        public MainGameFactory(GameObject rootObject) : base(BuildResolvers(rootObject)) {}

        private static IEnumerable<IDependencyResolver> BuildResolvers(GameObject rootObject) {
            return new List<IDependencyResolver> {
                new SingletonMonoBehaviourResolver<BuildingBlueprint>(rootObject),
                new SingletonMonoBehaviourResolver<GameController>(rootObject),
                new SingletonMonoBehaviourResolver<GameUIController>(),
                new SingletonMonoBehaviourResolver<GroundController>(rootObject),
                new SingletonMonoBehaviourResolver<StatManager>(rootObject),
                new SingletonMonoBehaviourResolver<SaverLoader>(),
                new SingletonMonoBehaviourResolver<MenuManager>(rootObject),
                new SingletonMonoBehaviourResolver<LogisticsManager>(rootObject),
                new SingletonMonoBehaviourResolver<DayCycleController>(rootObject),
            };
        }
    }
}