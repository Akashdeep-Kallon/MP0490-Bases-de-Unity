using UnityEngine;

// Movimiento y disparo de los enemigos.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyIA : MonoBehaviour
{
    public enum EnemyType { Walker, Shooter }

    [Header("Tipo")]
    public EnemyType type = EnemyType.Walker;

    [Header("Daño")]
    public int contactDamage = 1; // Daño al tocar al jugador
    public bool canBeStomped = true; // Muere si el jugador le cae encima

    [Header("Walker")]
    public float walkSpeed = 2f;
    public int direction = -1; // -1 = izquierda, 1 = derecha
    public LayerMask groundMask; // Pon aquí la capa del suelo

    [Header("Shooter")]
    public float range = 10f; // Distancia a la que ve al jugador
    public float fireInterval = 2f; // Segundos entre disparos
    public float bulletSpeed = 5f;
    public int bulletDamage = 1;

    private Rigidbody2D rb;
    private Collider2D col;
    private float fireTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.freezeRotation = true;
    }

    // La física (movimiento) va en FixedUpdate
    void FixedUpdate()
    {
        if (type == EnemyType.Walker)
            Walk();
    }

    void Update()
    {
        if (type == EnemyType.Shooter)
            Shoot();
    }

    //  MUERTE 

    // Punto único para matar al enemigo (pisotón, bala del jugador...).
    // Aquí podrás añadir después animación, sonido, soltar monedas, etc.
    public void Kill()
    {
        Destroy(gameObject);
    }

    //  WALKER 

    void Walk()
    {
        Bounds b = col.bounds;

        // Punto justo delante del enemigo (fuera de su propio collider)
        Vector2 front = new Vector2(b.center.x + direction * (b.extents.x + 0.05f), b.center.y);

        // ¿Hay una pared delante?
        bool wall = Physics2D.Raycast(front, Vector2.right * direction, 0.1f, groundMask);

        // ¿Hay suelo delante? (rayo hacia abajo desde delante de los pies)
        Vector2 feetFront = new Vector2(front.x, b.min.y + 0.1f);
        bool ground = Physics2D.Raycast(feetFront, Vector2.down, 0.5f, groundMask);

        // Si hay pared o no hay suelo, damos la vuelta
        if (wall || !ground)
            direction = -direction;

        // Movemos solo en X (la Y la controla la gravedad)
        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);

        // Giramos el sprite según la dirección
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -direction;
        transform.localScale = scale;
    }

    //  SHOOTER 

    void Shoot()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null || player.IsDead) return;

        // Si el jugador está lejos, no hacemos nada
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance > range)
        {
            fireTimer = 0f;
            return;
        }

        // Cuenta atrás para disparar
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire(player.transform.position);
        }
    }

    void Fire(Vector2 target)
    {
        // Creamos la bala: un cuadradito rojo
        GameObject bullet = new GameObject("EnemyBullet");
        bullet.transform.position = transform.position;
        bullet.transform.localScale = Vector3.one * 0.3f;

        SpriteRenderer sprite = bullet.AddComponent<SpriteRenderer>();
        sprite.sprite = EnemyBullet.GetSquareSprite();
        sprite.color = Color.red;
        // Dirección hacia el jugador
        Vector2 aim = (target - (Vector2)transform.position).normalized;

        // La bala se encarga de moverse y de hacer daño
        bullet.AddComponent<EnemyBullet>().Setup(aim, bulletSpeed, bulletDamage);
    }

    //  DAÑO AL JUGADOR 

    // Se llaman solos cuando el jugador toca al enemigo
    void OnCollisionStay2D(Collision2D c) { Touch(c.collider); }
    void OnTriggerStay2D(Collider2D c) { Touch(c); }

    void Touch(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || player.IsDead) return;

        // ¿El jugador cae desde arriba?
        bool fromAbove = player.Velocity.y <= 0f && other.bounds.min.y > col.bounds.center.y;

        if (canBeStomped && fromAbove)
        {
            player.Bounce(); // El jugador rebota
            Kill();          // El enemigo muere
        }
        else
        {
            player.TakeDamage(contactDamage, transform.position);
        }
    }
}
