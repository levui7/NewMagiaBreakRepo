using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 12;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        WeaponManager weapon = player.GetComponent<WeaponManager>();

        if (weapon == null)
            return;

        weapon.AddAmmo(ammoAmount);

        Destroy(gameObject);
    }
}