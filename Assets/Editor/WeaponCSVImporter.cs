using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WeaponCSVImporter : MonoBehaviour
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
}
