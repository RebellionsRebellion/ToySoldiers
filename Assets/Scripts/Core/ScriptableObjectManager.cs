using System;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScriptableObjectManager : MonoBehaviour
{
    [SerializeField] private ScriptableObject[] managedScriptableObjects;
    
    private void Awake()
    {
        Initialize();   
    }
    
    private void Initialize()
    {
        // Initialize all valid ScriptableObjects
        foreach (var so in managedScriptableObjects)
            if (so is IScriptableObjectInit initSo)
                initSo.Init();
    }
    
    
    # if UNITY_EDITOR
    [Button]
    private void PopulateScriptableObjects()
    {
        // Find all ScriptableObjects in the project
        var allScriptableObjects = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" });
        var managedList = new List<ScriptableObject>();
        foreach (var guid in allScriptableObjects)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so != null)
                managedList.Add(so);
        }

        managedScriptableObjects = managedList.ToArray();
    }
    #endif
}

public interface IScriptableObjectInit
{
    public void Init();
}