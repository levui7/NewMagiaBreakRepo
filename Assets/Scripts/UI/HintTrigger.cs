using UnityEngine;

public class HintTrigger : MonoBehaviour
{
    public Sprite hintSprite;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            return;

        if (HintImageUI.Instance != null)
            HintImageUI.Instance.Show(hintSprite);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            return;

        if (HintImageUI.Instance != null)
            HintImageUI.Instance.Hide();
    }
}