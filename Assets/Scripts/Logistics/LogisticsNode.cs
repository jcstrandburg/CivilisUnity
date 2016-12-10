using UnityEngine;
using System.Collections;

public class LogisticsNode : MonoBehaviour {

    private LogisticsNetwork _network;
    public LogisticsNetwork logisticsNetwork {
        set {
            if (_network && _network != value) {
                _network.DetachNode(this);
                _network = null;
            }
            if (_network == null && value != null) {
                _network = value;
                _network.AttachNode(this);
            }
        }
        get {
            return _network;
        }
    }

    private LogisticsManager _manager;
    [Inject]
    public LogisticsManager logisticsManager {
        set {
            if (_manager && _manager != value) {
                _manager.UnregisterNode(this);
                _manager = null;
            }
            if (_manager == null && value != null) {
                _manager = value;
                _manager.RegisterNode(this);
            }
        }
        get {
            if (_manager == null) {
                if (_manager) {
                    _manager = GetComponentInParent<LogisticsManager>();
                }
            }
            return _manager;
        }
    }
}
