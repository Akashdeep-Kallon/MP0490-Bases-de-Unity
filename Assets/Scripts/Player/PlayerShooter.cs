using UnityEngine;

// El jugador dispara si tiene monedas.
[RequireComponent(typeof(PlayerController))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Disparo")]
    public KeyCode fireKey = KeyCode.J;
    public int coinCost = 1; // Monedas que cuesta cada disparo
    public float cooldown = 0.3f; // Segundos mínimos entre disparos
    public float bulletSpeed = 12f;

    [Header("Salida de la bala (relativa al jugador, mirando a la derecha)")]
    public Vector2 muzzleOffset = new Vector2(0.6f, 0.1f);

    private PlayerController player;
    private float nextFireTime;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (player.IsDead || player.IsStunned) return;
        if (!Input.GetKeyDown(fireKey) || Time.time < nextFireTime) return;

        // Si no hay monedas suficientes, no se dispara
        if (!GameManager.Instance.TrySpendCoins(coinCost)) return;

        nextFireTime = Time.time + cooldown;
        Shoot();
    }

    private void Shoot()
    {
        float dir = player.FacingRight ? 1f : -1f;

        Vector2 spawnPos = (Vector2)transform.position
                         + new Vector2(muzzleOffset.x * dir, muzzleOffset.y);

        PlayerBullet.Spawn(spawnPos, dir, bulletSpeed);
    }
}
