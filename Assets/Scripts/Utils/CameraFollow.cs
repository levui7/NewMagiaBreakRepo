using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;

    [SerializeField] private float minSize = 6f;
    [SerializeField] private float maxSize = 12f;

    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private Transform player1;
    private Transform player2;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void SetPlayer(int playerID, Transform player)
    {
        if (playerID == 1)
            player1 = player;
        else if (playerID == 2)
            player2 = player;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition;

        if (player1 != null && player2 != null)
        {
            targetPosition = (player1.position + player2.position) / 2f;

            float distance = Vector2.Distance(player1.position, player2.position);
            float targetSize = Mathf.Clamp(distance, minSize, maxSize);

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetSize,
                zoomSpeed * Time.deltaTime);
        }
        else if (player1 != null)
        {
            targetPosition = player1.position;
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                minSize,
                zoomSpeed * Time.deltaTime);
        }
        else if (player2 != null)
        {
            targetPosition = player2.position;
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                minSize,
                zoomSpeed * Time.deltaTime);
        }
        else
        {
            return;
        }

        targetPosition += offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime);
    }
}