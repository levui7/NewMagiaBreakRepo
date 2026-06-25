using UnityEngine;
using UnityEngine.InputSystem;

public class NoteInteractable : MonoBehaviour
{
    public Sprite noteSprite;

    private bool playerInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerInside = true;

            Debug.Log("Нажмите E чтобы прочитать записку");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerInside = false;
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            NoteUI.Instance.Show(noteSprite);
        }
    }
}
