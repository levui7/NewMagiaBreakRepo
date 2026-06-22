using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WallHitReceiver2D : MonoBehaviour
{
    public DestructibleWall2D wall;
    public int damagePerHit = 1;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = false;
    }

    public void TakeHit(int damage)
    {
        if (wall != null)
            wall.TakeDamage(damage > 0 ? damage : damagePerHit);
    }
}