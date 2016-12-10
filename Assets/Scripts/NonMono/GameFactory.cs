using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

/// <summary>
/// Helper factory class that handles dependency injection/service location
/// </summary>
public class GameFactory {

    private GameController _gameController = null;
    [Injectable]
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameObject.FindObjectOfType<GameController>();
            }
            return _gameController;
        }
    }

    private GameUIController _guiController;
    [Injectable]
    public GameUIController guiController {
        get {
            if (_guiController == null) {
                _guiController = GameObject.FindObjectOfType<GameUIController>();
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
                _groundController = GameObject.FindObjectOfType<GroundController>();
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
                _statManager = GameObject.FindObjectOfType<StatManager>();
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
                _saverLoader = GameObject.FindObjectOfType<SaverLoader>();
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
                _menuManager = GameObject.FindObjectOfType<MenuManager>();
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

    /// <summary>Injectable fields from this class</summary>
    private Dictionary<Type, FieldInfo> myFields;
    /// <summary>Injectable properties from this class</summary>
    private Dictionary<Type, PropertyInfo> myProps;
    /// <summary>Injectable fields by object type</summary>
    private Dictionary<Type, FieldInfo[]> fieldInfoCache = new Dictionary<Type, FieldInfo[]>();
    /// <summary>Injectable properties by object type</summary>
    private Dictionary<Type, PropertyInfo[]> propInfoCache = new Dictionary<Type, PropertyInfo[]>();

    public GameFactory() {
        myFields = GetType()
            .GetFields()
            .Where(field => field.IsDefined(typeof(Injectable), false))
            .ToDictionary(
                ele => ele.FieldType,
                ele => ele);
        myProps = GetType()
            .GetProperties()
            .Where(prop => prop.IsDefined(typeof(Injectable), false))
            .ToDictionary(
                ele => ele.PropertyType,
                ele => ele);
    }

    /// <summary>
    /// Instatiates and injects a copy of the given GameObject (assumed to be a prefab)
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns>The instantiated object</returns>
    public GameObject Instantiate(GameObject prefab) {
        var instance = GameObject.Instantiate(prefab);
        InjectGameobject(instance);
        return instance;
    }

    /// <summary>
    /// Gets all Fields for the given type that can be injected.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private FieldInfo[] GetInjectableFields(Type t) {
        var flags = BindingFlags.Public | BindingFlags.Instance;

        if (!fieldInfoCache.ContainsKey(t)) {
            fieldInfoCache[t] = t
                .GetFields(flags)
                .Where(field => field.IsDefined(typeof(Inject), false))
                .ToArray();
        }
        return fieldInfoCache[t];
    }

    /// <summary>
    /// Gets all Properties for the given type that can be injected. The results will be cached.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private PropertyInfo[] GetInjectableProps(Type t) {
        var flags = BindingFlags.Public | BindingFlags.Instance;

        if (!propInfoCache.ContainsKey(t)) {
            propInfoCache[t] = t
                .GetProperties(flags)
                .Where(prop => prop.IsDefined(typeof(Inject), false))
                .ToArray();
        }
        return propInfoCache[t];
    }

    /// <summary>
    /// Injects an individual object from the Injectable fields and properties of this object
    /// </summary>
    /// <param name="injectme"></param>
    /// <returns>The object passed in</returns>
    public T InjectObject<T>(T injectme) {
        var compType = injectme.GetType();

        //get fields and properties to be injected
        var compFields = GetInjectableFields(compType);
        var compProperties = GetInjectableProps(compType);

        foreach (var compField in compFields) {
            var type = compField.FieldType;

            if (myFields.ContainsKey(type)) {
                var sourceField = myFields[type];
                compField.SetValue(injectme, sourceField.GetValue(this));
            }

            if (myProps.ContainsKey(type)) {
                var sourceProp = myProps[type];
                compField.SetValue(injectme, sourceProp.GetValue(this, null));
            }
        }

        foreach (var compProp in compProperties) {
            var type = compProp.PropertyType;

            if (myFields.ContainsKey(type)) {
                var sourceField = myFields[type];
                compProp.SetValue(injectme, sourceField.GetValue(this), null);
            }

            if (myProps.ContainsKey(type)) {
                var sourceProp = myProps[type];
                compProp.SetValue(injectme, sourceProp.GetValue(this, null), null);
            }
        }

        return injectme;
    }

    /// <summary>
    /// Injects all fields marked with the "inject" attribute in all components in the
    /// given object and all of its children. The injected value is taken from fields
    /// or properties on this object with the "injectable" attribute
    /// </summary>
    /// <param name="obj">The object to be injected</param>
    public void InjectGameobject(GameObject obj) {
        var components = obj.GetComponentsInChildren<Component>();

        foreach (var component in components) {
            InjectObject(component);
        }
    }

    /// <summary>
    /// Adds the given component type to the given game object and then injects it with dependencies
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns>The component added</returns>
    public T AddComponent<T>(GameObject go) where T : MonoBehaviour {
        var t = go.AddComponent<T>();
        InjectObject(t);
        return t;
    }

    /// <summary>
    /// Makes a component for an editor test, which involved instantiating a 
    /// temporary GameObject to add the component to
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The instantiated component</returns>
    //public T MakeTestComponent<T>() where T : MonoBehaviour {
    //    var tempGo = new GameObject();
    //    tempGo.name = String.Format("Temp_{0}_object", typeof(T).Name);
    //    var t = tempGo.AddComponent<T>();
    //    InjectObject(t);
    //    return t;
    //}
}
