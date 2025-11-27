using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System;
using Throwable_System.ThrowableTypes;

public class DataLoader
{
    public static List<WeaponDataSO> LoadWeaponsCSV()
    {
        List<WeaponDataSO> weapons = new List<WeaponDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("weapons");

        if(rows != null && rows.Count <= 1)
        {
            Debug.LogError("No weapon data found in CSV");
            return weapons;
        }

        // the csv parser will split into rows and columns, we iterate those rows and assign the data for each column for each row

        // skip header row if needed
        for (int i = 1; i < rows.Count; i++)
        {
            var columns = rows[i];

            // create a new ScriptableObject instance
            WeaponDataSO weaponData = ScriptableObject.CreateInstance<WeaponDataSO>();

            // assign values from CSV
            weaponData.ClassName = columns[0];
            weaponData.DisplayName = columns[1];
            weaponData.FireRateRPM = int.Parse(columns[2]);
            weaponData.Damage = int.Parse(columns[3]);
            
            weaponData.SupportedFireModes = columns[4]
                .Split(' ')
                .Where(x => Enum.TryParse<WeaponDataSO.FireModes>(x, true, out _))
                .Select(x => Enum.Parse<WeaponDataSO.FireModes>(x, true))
                .ToArray();
            
            weaponData.MagSize = int.Parse(columns[5]);
            weaponData.SpecialAmmo = bool.Parse(columns[6]);
            weaponData.ReloadTime = float.Parse(columns[7]);
            weaponData.BaseSpread = float.Parse(columns[8]);
            weaponData.HalfSpread = float.Parse(columns[9]);
            weaponData.MaxSpread = float.Parse(columns[10]);
            weaponData.ShotQuantity = int.Parse(columns[11]);
            weaponData.ShotSpread = float.Parse(columns[12]);
            weaponData.IsPhysicsBased = bool.Parse(columns[13]);
            weaponData.InitialVelocityMS = float.Parse(columns[14]);
            weaponData.MassKG = float.Parse(columns[15]);
            weaponData.Attachments = columns[16].Split(',');    // same as fire modes

            // save as an asset in the project so it can be referenced
            string assetPath = $"Assets/ScriptableObjects/Weapons/{weaponData.ClassName}.asset";
            AssetDatabase.CreateAsset(weaponData, assetPath);
            AssetDatabase.SaveAssets();

            weapons.Add(weaponData);
        }

        return weapons;
    }
    
    public static List<ThrowableDataSO> LoadThrowablesCSV()
    {
        List<ThrowableDataSO> throwables = new List<ThrowableDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("throwables");

        if(rows != null && rows.Count <= 1)
        {
            Debug.LogError("No weapon data found in CSV");
            return throwables;
        }

        // the csv parser will split into rows and columns, we iterate those rows and assign the data for each column for each row

        // skip header row if needed
        for (int i = 1; i < rows.Count; i++)
        {
            var columns = rows[i];

            // create a new ScriptableObject instance
            ThrowableDataSO throwableData = ScriptableObject.CreateInstance<ThrowableDataSO>();

            // assign values from CSV
            throwableData.ClassName = columns[0];
            throwableData.DisplayName = columns[1];
            
            throwableData.FuseTime = float.Parse(columns[2]);
            throwableData.IsImpactFuse = bool.Parse(columns[3]);
            
            throwableData.Damage = float.Parse(columns[4]);
            throwableData.Radius = float.Parse(columns[5]);
            
            throwableData.EffectType =
                Enum.Parse<ThrowableDataSO.EffectTypes>(columns[6], true);

            SetupPrefab(throwableData);

            // save as an asset in the project so it can be referenced
            string assetPath = $"Assets/ScriptableObjects/Throwables/{throwableData.ClassName}.asset";
            AssetDatabase.CreateAsset(throwableData, assetPath);
            AssetDatabase.SaveAssets();

            throwables.Add(throwableData);
        }

        return throwables;
    }

    private static void SetupPrefab(ThrowableDataSO throwableData)
    {
        string prefabPath = $"Assets/Prefabs/Throwables/{throwableData.ClassName}/{throwableData.ClassName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        // fallback to generic if prefab not found
        if (prefab == null)
        {
            Debug.Log("No prefab found, making a new one as a clone of generic");
            // load generic prefab
            string genericPath = "Assets/Prefabs/Throwables/generic/generic.prefab";
            GameObject genericPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(genericPath);

            if (genericPrefab != null)
            {
                Debug.Log("Found the generic one");
                // ensure folder exists
                string folderPath = $"Assets/Prefabs/Throwables/{throwableData.ClassName}";
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    Debug.Log("Creating a new folder");
                    AssetDatabase.CreateFolder("Assets/Prefabs/Throwables", throwableData.ClassName);
                }

                // create a copy of generic prefab at the correct path
                prefab = PrefabUtility.SaveAsPrefabAsset(genericPrefab, prefabPath);
                Debug.Log("Created a new prefab");
            }
            else
            {
                Debug.LogError("No generic prefab found! Make sure you have one before importing");
            }
        }

        throwableData.ThrowablePrefab = prefab;

        if (prefab != null)
        {
            GameObject tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // modify or add the component
            ThrowableTemplate existingTemplate = tempInstance.GetComponent<ThrowableTemplate>();

            if (existingTemplate != null)
            {
                if (throwableData.EffectType != ThrowableDataSO.EffectTypes.Custom)
                {
                    // remove the existing one if its non-custom
                    GameObject.DestroyImmediate(existingTemplate);
                }
            }
            
            // add the correct components if non custom
            switch (throwableData.EffectType)
            {
                case ThrowableDataSO.EffectTypes.Explosion:
                    if (tempInstance.GetComponent<ExplosiveGrenade>() == null)
                        tempInstance.AddComponent<ExplosiveGrenade>();
                    break;

                case ThrowableDataSO.EffectTypes.Flash:
                    if (tempInstance.GetComponent<FlashGrenade>() == null)
                        tempInstance.AddComponent<FlashGrenade>();
                    break;
            }
            
            // if after all this if it has a component set its values
            ThrowableTemplate throwableClass = tempInstance.GetComponent<ThrowableTemplate>();
            if (throwableClass != null)
            {
                throwableClass.FuseTime = throwableData.FuseTime;
                throwableClass.IsImpactFuse = throwableData.IsImpactFuse;
                
                throwableClass.Damage = throwableData.Damage;
                throwableClass.Radius = throwableData.Radius;
            }

            // apply changes back to the prefab
            PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
            GameObject.DestroyImmediate(tempInstance);
        }
    }
}
