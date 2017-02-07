using UnityEngine;

namespace Neolithica.MonoBehaviours.Logistics {
    public class LogisticsNode : MonoBehaviour {

        private LogisticsNetwork logisticsNetwork;
        public LogisticsNetwork LogisticsNetwork {
            set {
                if (logisticsNetwork && logisticsNetwork != value) {
                    logisticsNetwork.DetachNode(this);
                    logisticsNetwork = null;
                }
                if (logisticsNetwork == null && value != null) {
                    logisticsNetwork = value;
                    logisticsNetwork.AttachNode(this);
                }
            }
            get {
                return logisticsNetwork;
            }
        }

        private LogisticsManager logisticsManager;
        [Inject]
        public LogisticsManager LogisticsLogisticsManager {
            set {
                if (logisticsManager && logisticsManager != value) {
                    logisticsManager.UnregisterNode(this);
                    logisticsManager = null;
                }
                if (logisticsManager == null && value != null) {
                    logisticsManager = value;
                    logisticsManager.RegisterNode(this);
                }
            }
            get {
                if (logisticsManager == null) {
                    if (logisticsManager) {
                        logisticsManager = GetComponentInParent<LogisticsManager>();
                    }
                }
                return logisticsManager;
            }
        }
    }
}
