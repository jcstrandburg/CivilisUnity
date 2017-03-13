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
                if (_logisticsManager == null) {
                    _logisticsManager = GetComponentInParent<LogisticsManager>();
                }
                return _logisticsManager;
            }
        }
    }
}
