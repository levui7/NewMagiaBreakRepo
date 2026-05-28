using UnityEngine;

public class CartridgePickup : MonoBehaviour
{
    public WeaponManager.Element element = WeaponManager.Element.Fire;
    public int amount = 3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        WeaponManager weapon = player.GetComponent<WeaponManager>();

        if (weapon == null)
            return;

        weapon.AddElementAmmo(element, amount);

        Destroy(gameObject);
    }
}