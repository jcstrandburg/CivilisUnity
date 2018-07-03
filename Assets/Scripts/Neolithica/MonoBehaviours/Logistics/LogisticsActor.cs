using Neolithica.Extensions;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Logistics {
    [SavableMonobehaviour(24)]
    public class LogisticsActor : MonoBehaviour {

        private LogisticsManager _logisticsManager;
        [Inject]
        public LogisticsManager logisticsManager {
            set {
                _logisticsManager = value;
            }
            get {
                return this.CacheComponent(ref _logisticsManager, () => GetComponentInParent<LogisticsManager>());
            }
        }
    }
}
