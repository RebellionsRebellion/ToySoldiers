using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

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
            weaponData.FireModes = columns[4].Split(' '); // i do this as just space seperated strings
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

    public static List<SoundDataSO> LoadSoundsCSV()
    {
        List<SoundDataSO> sounds = new List<SoundDataSO>();
        List<string[]> rows = CSVParser.LoadFromCSV("sounds");

        if (rows == null || rows.Count <= 1)
        {
            Debug.LogError("No sound data found in CSV");
            return sounds;
        }

        // skip header row
        for (int i = 1; i < rows.Count; i++)
        {
            string[] columns = rows[i];

            // create a new ScriptableObject instance
            SoundDataSO soundData = ScriptableObject.CreateInstance<SoundDataSO>();

            // assign values from CSV
            soundData.WwiseName = columns[0];
            soundData.Description = columns[2];

            // parse enum safely
            SoundType parsedType;
            if (Enum.TryParse<SoundType>(columns[1], true, out parsedType))
                soundData.Type = parsedType;
            else
                soundData.Type = SoundType.WwiseEvent; // fallback default

            soundData.Is2D = false; // default

            // save as an asset
            string assetPath = $"Assets/ScriptableObjects/Sounds/{soundData.WwiseName}.asset";
            AssetDatabase.CreateAsset(soundData, assetPath);
            AssetDatabase.SaveAssets();

            sounds.Add(soundData);
        }

        return sounds;
    }
}
