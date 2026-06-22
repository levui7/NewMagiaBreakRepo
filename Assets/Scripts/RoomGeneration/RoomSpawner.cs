using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public Direction direction;
    public enum Direction
    {
        Top,      // Верх
        Bottom,   // Низ
        Left,     // Лево
        Right,    // Право
        None      // Нет направления
    }

    private RoomVariants variants;        // Ссылка на варианты комнат
    private int rand;                     // Случайное число для выбора комнаты
    private bool spawned = false;         // Флаг: комната уже создана?
    private float waitTime = 3f;          // Время до самоуничтожения объекта

    private void Start()
    {
        variants = GameObject.FindGameObjectWithTag("Rooms")
                              .GetComponent<RoomVariants>();
        Destroy(gameObject, waitTime);
        Invoke("Spawn", 0.2f);
    }

    public void Spawn()
    {
        if (!spawned)
        {
            if (direction == Direction.Top)
            {
                rand = Random.Range(0, variants.topRooms.Length);
                Instantiate(variants.topRooms[rand], transform.position,
                           variants.topRooms[rand].transform.rotation);
            }

            if (direction == Direction.Bottom)
            {
                rand = Random.Range(0, variants.bottomRooms.Length);
                Instantiate(variants.bottomRooms[rand], transform.position,
                           variants.bottomRooms[rand].transform.rotation);
            }

            if (direction == Direction.Right)
            {
                rand = Random.Range(0, variants.rightRooms.Length);
                Instantiate(variants.rightRooms[rand], transform.position,
                           variants.rightRooms[rand].transform.rotation);
            }

            if (direction == Direction.Left)
            {
                rand = Random.Range(0, variants.leftRooms.Length);
                Instantiate(variants.leftRooms[rand], transform.position,
                           variants.leftRooms[rand].transform.rotation);
            }

            spawned = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("RoomPoint") &&
            other.GetComponent<RoomSpawner>().spawned)
        {
            Destroy(gameObject);
        }
    }
}
