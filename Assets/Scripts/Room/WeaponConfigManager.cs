using UnityEngine;

public class WeaponConfigManager : MonoBehaviour
{
    public static WeaponConfigManager Instance;

    public AttackMode selectedAttackMode =
        AttackMode.Single;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}