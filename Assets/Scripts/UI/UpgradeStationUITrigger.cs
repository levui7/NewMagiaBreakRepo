using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeStationUITrigger : MonoBehaviour
{
    public LobbyShopUIController shopUI;

    public GameObject promptObject;

    private bool playerInside;

    private void Update()
    {
        if (!playerInside)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            shopUI.OpenShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        playerInside = true;

        if (promptObject != null)
            promptObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        playerInside = false;

        if (promptObject != null)
            promptObject.SetActive(false);
    }
}