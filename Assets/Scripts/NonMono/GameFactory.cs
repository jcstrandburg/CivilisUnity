using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;

/// <summary>
/// Helper factory class that handles dependency injection/service location
/// </summary>
public class GameFactory {

    private static GameController _gameController = null;
    [Injectable]
    public static GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
    }

    private GameUIController _guiController;
    [Injectable]
    public GameUIController guiController {
        get {
            if (_guiController == null) {
                _guiController = gameController.GuiController;
            }
            return _guiController;
        }
        set {
            _guiController = value;
        }
    }

    private GroundController _groundController;
    [Injectable]
    public GroundController groundController {
        get {
            if (_groundController == null) {
                _groundController = gameController.GroundController;
            }
            return _groundController;
        }
        set {
            _groundController = value;
        }
    }

    private StatManager _statManager;
    [Injectable]
    public StatManager statManager {
        get {
            if (_statManager == null) {
                //_statManager = gameController.StatManager;
                _statManager = GameObject.FindObjectOfType<StatManager>();
                //if (_statManager == null) {
                //    throw new System.Exception("fuck");
                //}
            }
            return _statManager;
        }
        set {
            _statManager = value;
        }
    }

    private SaverLoader _saverLoader;
    [Injectable]
    public SaverLoader saverLoader {
        get {
            if (_saverLoader == null) {
                _saverLoader = gameController.SaverLoader;
            }
            return _saverLoader;
        }
        set {
            _saverLoader = value;
        }
    }

    private MenuManager _menuManager;
    [Injectable]
    public MenuManager menuManager {
        get {
            if (_menuManager == null) {
                _menuManager = gameController.MenuManager;
            }
            return _menuManager;
        }
        set {
            _menuManager = value;
        }
    }

    private LogisticsManager _logisticsManager;
    [Injectable]
    public LogisticsManager logisticsManager {
        get {
            if (_logisticsManager == null) {
                _logisticsManager = GameObject.FindObjectOfType<LogisticsManager>();
            }
            return _logisticsManager;
        }
        set {
            _logisticsManager = value;
        }
    }

    /// <summary>
    /// Instatiates and injects a copy of the given GameObject (assumed to be a prefab)
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns>The instantiated object</returns>
    public GameObject Instantiate(GameObject prefab) {
        var instance = GameObject.Instantiate(prefab);
        InjectObject(instance);
        return instance;
    }

    /// <summary>
    /// Injects an individual component from the given source FieldInfo and PropertyInfo arrays
    /// </summary>
    /// <param name="comp"></param>
    /// <param name="myFields"></param>
    /// <param name="myProps"></param>
    public void InjectComponenent(Component comp, FieldInfo[] myFields, PropertyInfo[] myProps) {
        var flags = BindingFlags.Public | BindingFlags.Instance;

        //get fields and properties to be injected
        var compFields = comp.GetType()
                            .GetFields(flags)
                            .Where(field => field.IsDefined(typeof(Inject), false))
                            .ToArray();
        var compProperties = comp.GetType()
                                .GetProperties(flags)
                                .Where(prop => prop.IsDefined(typeof(Inject), false))
                                .ToArray();

        foreach (var compField in compFields) {
            //select from myFields where the type is correct, if any matches are found just use the first one
            var sourceFields = myFields
                                .Where(field => field.FieldType == compField.FieldType)
                                .ToArray();
            if (sourceFields.Length > 0) {
                compField.SetValue(comp, sourceFields[0].GetValue(this));
                //Debug.Log(string.Format("Injecting {0} on {1} for {2}", compField.Name, comp.GetType().Name, comp.gameObject.name));
                continue;
            }

            //select from myProps where the type is correct, if any matches are found just use the first one
            var sourceProps = myProps
                                .Where(prop => prop.PropertyType == compField.FieldType)
                                .ToArray();
            if (sourceProps.Length > 0) {
                compField.SetValue(comp, sourceProps[0].GetValue(this, null));
                //Debug.Log(string.Format("Injecting {0} on {1} for {2}", compField.Name, comp.GetType().Name, comp.gameObject.name));
                continue;
            }
        }

        foreach (var compProp in compProperties) {
            //select from myFields where the type is correct, if any matches are found just use the first one
            var sourceFields = myFields
                                .Where(field => field.FieldType == compProp.PropertyType)
                                .ToArray();
            if (sourceFields.Length > 0) {
                compProp.SetValue(comp, sourceFields[0].GetValue(this), null);
                //Debug.Log(string.Format("Injecting {0} on {1} for {2}", compProp.Name, comp.GetType().Name, comp.gameObject.name));
                continue;
            }

            //select from myProps where the type is correct, if any matches are found just use the first one
            var sourceProps = myProps
                                .Where(prop => prop.PropertyType == compProp.PropertyType)
                                .ToArray();
            if (sourceProps.Length > 0) {
                var value = sourceProps[0].GetValue(this, null);
                compProp.SetValue(comp, value, null);
                //Debug.Log(string.Format("Injecting {0} on {1} for {2} with value {3}", compProp.Name, comp.GetType().Name, comp.gameObject.name, value));
                continue;
            }
        }
    }

    /// <summary>
    /// Injects all fields marked with the "inject" attribute in all components in the
    /// given object and all of its children. The injected value is taken from fields
    /// or properties on this object with the "injectable" attribute
    /// </summary>
    /// <param name="obj">The object to be injected</param>
    public void InjectObject(GameObject obj) {
        var components = obj.GetComponentsInChildren<Component>();
        var myFields = this.GetType()
                           .GetFields()
                           .Where(field=>field.IsDefined(typeof(Injectable), false))
                           .ToArray();
        var myProps = this.GetType()
                           .GetProperties()
                           .Where(prop => prop.IsDefined(typeof(Injectable), false))
                           .ToArray();

        foreach (var component in components) {
            InjectComponenent(component, myFields, myProps);
        }
    }
}
