using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

<<<<<<<< HEAD:Assets/Editor/CSVImporter.cs
public class CSVImporter : MonoBehaviour
========
public class CSVImporters : MonoBehaviour
>>>>>>>> dev:Assets/Editor/CSVImporters.cs
{
    [MenuItem("Tools/Data Importing/Import Weapons")]
    public static void ImportWeapons()
    {
        // find the asset with assetdatabase
        string[] guids = AssetDatabase.FindAssets("t:WeaponTypesSO");

        if (guids.Length == 0)
        {
            Debug.LogError("WeaponTypes asset not found in project! Please make one before continuing to import");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        WeaponTypesSO so = AssetDatabase.LoadAssetAtPath<WeaponTypesSO>(path);

        // load data from the relevant csv
        so.WeaponTypes = DataLoader.LoadWeaponsCSV().ToArray();

        // save it to the SO
        EditorUtility.SetDirty(so);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Weapons updated.");
    }
    
<<<<<<<< HEAD:Assets/Editor/CSVImporter.cs
    [MenuItem("Tools/Data Importing/Import Sounds")]
    public static void ImportSounds()
    {
        // find the asset with assetdatabase
        string[] guids = AssetDatabase.FindAssets("t:SoundTypesSO");

        if (guids.Length == 0)
        {
            Debug.LogError("SoundTypes asset not found in project! Please make one before continuing to import");
========
    [MenuItem("Tools/Data Importing/Import Throwables")]
    public static void ImportThrowables()
    {
        // find the asset with assetdatabase
        string[] guids = AssetDatabase.FindAssets("t:ThrowableTypesSO");

        if (guids.Length == 0)
        {
            Debug.LogError("ThrowableTypes asset not found in project! Please make one before continuing to import");
>>>>>>>> dev:Assets/Editor/CSVImporters.cs
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
<<<<<<<< HEAD:Assets/Editor/CSVImporter.cs
        SoundTypesSO so = AssetDatabase.LoadAssetAtPath<SoundTypesSO>(path);

        // load data from the relevant csv
        so.SoundTypes = DataLoader.LoadSoundsCSV().ToArray();
========
        ThrowableTypesSO so = AssetDatabase.LoadAssetAtPath<ThrowableTypesSO>(path);

        // load data from the relevant csv
        so.ThrowableTypes = DataLoader.LoadThrowablesCSV().ToArray();
>>>>>>>> dev:Assets/Editor/CSVImporters.cs

        // save it to the SO
        EditorUtility.SetDirty(so);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

<<<<<<<< HEAD:Assets/Editor/CSVImporter.cs
        Debug.Log("Sounds updated.");
========
        Debug.Log("Throwables updated.");
>>>>>>>> dev:Assets/Editor/CSVImporters.cs
    }
}
