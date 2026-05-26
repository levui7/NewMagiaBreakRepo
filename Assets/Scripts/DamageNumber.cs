using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifetime = 1f;
    public float riseSpeed = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
    }

    public void Setup(int damage, Color color)
    {
        if (text != null)
        {
            text.text = $"-{damage}";
            text.color = color;
        }
    }
}