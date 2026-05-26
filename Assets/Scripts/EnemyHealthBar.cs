using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (target == null)
            target = transform.parent;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }

        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }
    }

    public void SetHealth(int current, int max)
    {
        if (slider == null)
            return;

        slider.maxValue = max;
        slider.value = current;
    }
}