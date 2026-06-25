using UnityEngine;

[System.Serializable]
public class PlayerWeaponData
{
    public int currentAmmo = 6;
    public int fireAmmo = 0;
    public int waterAmmo = 0;
    public Element currentElement = Element.Physical;

    public void Reset(int magazineSize)
    {
        currentAmmo = Mathf.Max(1, magazineSize);
        fireAmmo = 0;
        waterAmmo = 0;
        currentElement = Element.Physical;
    }

    public void CopyFromWeapon(WeaponManager weapon)
    {
        if (weapon == null)
            return;

        currentAmmo = Mathf.Clamp(weapon.currentAmmo, 0, weapon.magazineSize);
        fireAmmo = Mathf.Max(0, weapon.fireAmmo);
        waterAmmo = Mathf.Max(0, weapon.waterAmmo);
        currentElement = weapon.CurrentElement;
    }
}