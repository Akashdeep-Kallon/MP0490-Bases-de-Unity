using UnityEngine;

// Lleva al jugador al teletransportador enlazado.
public class Teleporter : MonoBehaviour
{
    public Teleporter target; // A cuál te lleva
    public float cooldown = 1f; // Segundos hasta poder volver a teletransportarte
    public float spinSpeed = 90f; // Giro decorativo (0 = no gira)

    void Update()
    {
        transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || player.IsDead) return;

        if (target == null)
        {
            Debug.LogWarning("El teletransportador '" + name + "' no tiene Target asignado", this);
            return;
        }

        // El cooldown vive en el jugador: así, al llegar al destino,
        // el otro teletransportador no te devuelve de inmediato.
        if (!player.CanTeleport) return;

        player.Teleport(target.transform.position, cooldown);
    }

    // Dibuja una línea en la vista Scene para ver qué enlaza con qué
    void OnDrawGizmos()
    {
        if (target == null) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
}
