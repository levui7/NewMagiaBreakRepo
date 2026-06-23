using UnityEngine;

public class WeaponStation : MonoBehaviour
{
    public static WeaponStation Instance;

    public bool playerInside;

    private void Awake()
    {
        Instance = this;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
            playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
            playerInside = false;
    }
}