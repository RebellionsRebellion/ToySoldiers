using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class DataLoader
{
    public static List<WeaponSO> LoadWeaponsCSV()
    {
        List<WeaponSO> weapons = new List<WeaponSO>();
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
            WeaponSO weapon = ScriptableObject.CreateInstance<WeaponSO>();

            // assign values from CSV
            weapon.ClassName = columns[0];
            weapon.DisplayName = columns[1];
            weapon.FireRateRPM = int.Parse(columns[2]);
            weapon.Damage = int.Parse(columns[3]);
            weapon.FireModes = columns[4].Split(' '); // i do this as just space seperated strings
            weapon.MagSize = int.Parse(columns[5]);
            weapon.SpecialAmmo = bool.Parse(columns[6]);
            weapon.ReloadTime = float.Parse(columns[7]);
            weapon.BaseSpread = float.Parse(columns[8]);
            weapon.HalfSpread = float.Parse(columns[9]);
            weapon.MaxSpread = float.Parse(columns[10]);
            weapon.ShotQuantity = int.Parse(columns[11]);
            weapon.ShotSpread = float.Parse(columns[12]);
            weapon.IsPhysicsBased = bool.Parse(columns[13]);

            // save as an asset in the project so it can be referenced
            string assetPath = $"Assets/ScriptableObjects/Weapons/{weapon.ClassName}.asset";
            AssetDatabase.CreateAsset(weapon, assetPath);
            AssetDatabase.SaveAssets();

            weapons.Add(weapon);
        }

        return weapons;
    }
}
