using UnityEngine;

// La cámara sigue al jugador en horizontal.
public class CameraTargetX : MonoBehaviour
{
    public Transform player; // Arrastra aquí al jugador

    private float fixedY;

    void Start()
    {
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(player.position.x, fixedY, 0f);
    }
}