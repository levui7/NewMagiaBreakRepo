using UnityEngine;

public class UpgradeStationUITrigger : MonoBehaviour
{
    public LobbyShopUIController shopUI;

    private bool playerInside;

    public GameObject interactText;

    private void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            shopUI.OpenShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        playerInside = true;

        if (interactText != null)
            interactText.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        playerInside = false;

        if (interactText != null)
            interactText.SetActive(false);
    }
}