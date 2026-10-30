using UnityEngine;

// El jugador reaparece aquí o pasa al siguiente nivel.
public class LevelPoint : MonoBehaviour
{
    public enum PointType { Checkpoint, Goal }

    [Header("Tipo")]
    public PointType type = PointType.Checkpoint;

    [Header("Aspecto (opcional, solo Checkpoint)")]
    public Sprite activeSprite; // Sprite que se pone al activarlo

    private bool used; // Para que no se active más de una vez

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || player.IsDead) return;

        used = true;

        if (type == PointType.Checkpoint)
            Activate(player);
        else if (GameManager.Instance != null)
            GameManager.Instance.NextLevel();
        else
            Debug.LogError("Falta el GameManager en la escena");
    }

    // Guarda este punto como lugar de reaparición
    private void Activate(PlayerController player)
    {
        player.SetRespawn(transform.position + Vector3.up * 0.3f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && activeSprite != null)
            sr.sprite = activeSprite;
    }
}
