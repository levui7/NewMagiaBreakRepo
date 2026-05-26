using UnityEngine;

public class CartridgePickup : MonoBehaviour
{
    public WeaponManager.Element element = WeaponManager.Element.Fire;
    public int amount = 6;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        WeaponManager weaponManager = player.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.AddCartridge(element, amount);
            Destroy(gameObject);
        }
    }
}
