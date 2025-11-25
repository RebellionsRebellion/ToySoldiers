using System;
using UnityEngine;
using TMPro;

public class TempAmmoUI : MonoBehaviour
{
    public WeaponsSystem weaponsSystem;
    public TMP_Text ammoText;
    
    private void Update()
    {
        ammoText.text =
            $"{weaponsSystem.currentWeapon.CurrentAmmoInMag} / {weaponsSystem.currentWeapon.WeaponData.MagSize}";
    }
}


