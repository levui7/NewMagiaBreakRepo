using UnityEngine;

[System.Serializable]
public class PlayerWeaponData
{
    public int currentAmmo = 6;
    public int fireAmmo = 0;
    public int waterAmmo = 0;

    public Element currentElement = Element.Physical;
}