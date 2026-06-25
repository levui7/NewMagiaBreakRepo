using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    public Image noteImage;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Sprite sprite)
    {
        if (sprite == null)
            return;

        noteImage.sprite = sprite;
        gameObject.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Hide();
        }
    }
}