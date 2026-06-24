using UnityEngine;
using UnityEngine.UI;

public class HintImageUI : MonoBehaviour
{
    public static HintImageUI Instance;

    public Image hintImage;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Sprite sprite)
    {
        if (hintImage == null)
        {
            Debug.LogError("HintImage не назначен!");
            return;
        }

        if (sprite == null)
        {
            Debug.LogError("hintSprite не назначен!");
            return;
        }

        hintImage.sprite = sprite;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}