using System;
using System.Collections;
using UnityEngine;

// Controla el movimiento, la vida y la reaparición del jugador.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // Permite que otros scripts (enemigos, proyectiles, HUD...) encuentren al jugador
    public static PlayerController Instance { get; private set; }

    // VARIABLES PÚBLICAS

    [Header("Movimiento")]
    public float movingSpeed = 5f; // Velocidad al caminar
    public float runSpeed = 8f; // Velocidad al correr
    public float jumpForce = 7f; // Fuerza del salto
    public int maxJumps = 2; // 1 = salto normal, 2 = doble salto, 3 = triple...
    public float stompBounce = 9f; // Rebote al pisar un enemigo

    [Header("Vida")]
    public int maxHealth = 3; // Vida máxima del jugador

    [Header("Daño")]
    public float invulnerableTime = 1.5f; // Segundos sin poder recibir daño tras un golpe
    public float stunTime = 0.3f; // Segundos sin poder moverse tras un golpe
    public float knockbackX = 6f; // Empujón horizontal al recibir daño
    public float knockbackY = 7f; // Empujón vertical al recibir daño
    public float killY = -15f; // Si cae por debajo de esta altura, muere

    [Header("Teclas (editables en el Inspector)")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode altJumpKey = KeyCode.W;
    public KeyCode runKey = KeyCode.LeftShift;

    [Header("Efectos")]
    public GhostTrail ghostTrail;

    [Header("Orientación")]
    public bool spriteFacesRight = true; // Desmárcalo si tu sprite original mira a la izquierda

    // PROPIEDADES (solo lectura para otros scripts)

    public int Health { get { return health; } }
    public bool IsDead { get { return dead; } }
    public bool IsStunned { get { return stunTimer > 0f; } }
    public bool FacingRight { get { return facingRight; } }
    public Vector2 Velocity { get { return rb.linearVelocity; } }

    // EVENTOS

    // Se lanza cada vez que cambia la vida: (vida actual, vida máxima)
    public event Action<int, int> OnHealthChanged;

    // VARIABLES PRIVADAS

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private bool facingRight;
    private static readonly int TurnHash = Animator.StringToHash("Turn");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int DieHash = Animator.StringToHash("die");

    private float moveInput; // -1 = izquierda, 0 = quieto, 1 = derecha
    private bool isRunning;
    private bool isGrounded;
    private int jumpsLeft;

    private int health;
    private bool dead;
    private float invulnTimer;
    private float stunTimer;
    private Vector3 respawnPoint;

    //  AWAKE 

    // Se ejecuta antes que cualquier Start. Dejamos todo listo aquí para que
    // el HUD (que lee la vida en su Start) encuentre valores correctos.
    void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        facingRight = spriteFacesRight;
        jumpsLeft = maxJumps;
        health = maxHealth;
        respawnPoint = transform.position;
    }
    //  UPDATE 

    void Update()
    {
        if (dead) return;

        // Si se cae al vacío, muere
        if (transform.position.y < killY)
        {
            Die();
            return;
        }

        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (stunTimer > 0f) stunTimer -= Time.deltaTime;

        ReadInput();
        HandleFlip();
        UpdateAnimator();
        UpdateEffects();
        UpdateBlink();
    }

    // La estela solo sale al correr de verdad: tecla pulsada, en el suelo y con velocidad
    private void UpdateEffects()
    {
        if (ghostTrail == null) return;

        ghostTrail.emitting = isRunning && isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool(IsGroundedHash, isGrounded);
    }

    // Parpadea mientras es invulnerable
    private void UpdateBlink()
    {
        if (sr == null) return;

        if (invulnTimer > 0f)
            sr.enabled = Mathf.FloorToInt(Time.time * 15f) % 2 == 0;
        else
            sr.enabled = true;
    }

    private void HandleFlip()
    {
        if (moveInput > 0f && !facingRight)
            Flip();
        else if (moveInput < 0f && facingRight)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;

        // Invertimos la escala X: voltea sprite y colliders a la vez
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;

        // La animación de giro solo tiene sentido en el suelo
        if (isGrounded && animator != null)
            animator.SetTrigger(TurnHash);
    }

    // FIXEDUPDATE 

    void FixedUpdate()
    {
        if (dead) return;

        // Mientras está aturdido no se mueve (así se nota el empujón del golpe)
        if (stunTimer <= 0f)
            Move();
    }

    // TECLADO 

    private void ReadInput()
    {
        moveInput = 0f;

        if (Input.GetKey(leftKey))
            moveInput = -1f;
        if (Input.GetKey(rightKey))
            moveInput = 1f;

        isRunning = Input.GetKey(runKey);

        if ((Input.GetKeyDown(jumpKey) || Input.GetKeyDown(altJumpKey)) && stunTimer <= 0f)
            Jump();
    }

    //  MOVER 

    private void Move()
    {
        float speed = isRunning ? runSpeed : movingSpeed;

        // Solo cambiamos la velocidad horizontal; la vertical la manda la gravedad
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    //  SALTAR 

    private void Jump()
    {
        if (jumpsLeft <= 0) return;

        // Velocidad vertical a 0 para que todos los saltos tengan la misma altura
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpsLeft--;
    }

    //  DETECCIÓN DE SUELO 

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // normal.y > 0.5 = lo que tocamos está debajo de nosotros (suelo)
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;

                // Si no estamos subiendo, recuperamos los saltos
                if (rb.linearVelocity.y <= 0.1f)
                    jumpsLeft = maxJumps;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    //  VIDA Y DAÑO 

    // Único punto donde cambia la vida: así el evento siempre se lanza.
    private void SetHealth(int value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    // Los enemigos y proyectiles llaman a esta función.
    // Devuelve true si el daño se ha aplicado.
    public bool TakeDamage(int amount, Vector2 sourcePos)
    {
        if (dead || invulnTimer > 0f) return false;

        SetHealth(health - amount);
        if (health <= 0)
        {
            Die();
            return true;
        }

        invulnTimer = invulnerableTime;
        stunTimer = stunTime;

        // Empujón: nos lanza lejos de quien nos ha golpeado
        float dir = transform.position.x >= sourcePos.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * knockbackX, knockbackY);
        return true;
    }

    // Para pociones, corazones, etc.
    public void Heal(int amount)
    {
        if (dead) return;
        SetHealth(health + amount);
    }

    // Rebote al pisar un enemigo. force = -1 usa el valor de stompBounce.
    public void Bounce(float force = -1f)
    {
        if (force < 0f) force = stompBounce;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpsLeft = Mathf.Max(jumpsLeft, maxJumps - 1); // Recupera salto para encadenar pisotones
        isGrounded = false;
    }

    //  MORIR Y REAPARECER 

    public void Die()
    {
        if (dead) return;
        dead = true;
        SetHealth(0);

        // Lanza la animación de muerte
        if (animator != null) animator.SetTrigger(DieHash);

        // Quitamos los colliders para que caiga fuera del mapa
        SetCollidersEnabled(false);

        if (ghostTrail != null) ghostTrail.emitting = false;
        if (sr != null) sr.enabled = true;

        // Saltito de muerte
        rb.linearVelocity = new Vector2(0f, 10f);

        StartCoroutine(DeathRoutine());
    }

    // Espera un poco y deja que el GameManager decida: reaparecer o game over
    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance != null)
            GameManager.Instance.HandlePlayerDeath(this);
        else
            Respawn();
    }

    public void Respawn()
    {
        // Sale de la animación de muerte y vuelve a idle
        if (animator != null)
        {
            animator.ResetTrigger(DieHash);
            animator.Play("idle_hero", 0, 0f);
        }

        transform.position = respawnPoint;
        rb.position = respawnPoint;
        rb.linearVelocity = Vector2.zero;

        SetCollidersEnabled(true);

        dead = false;
        stunTimer = 0f;
        invulnTimer = 2f; // Un momento de protección al reaparecer
        jumpsLeft = maxJumps;
        SetHealth(maxHealth);
    }

    //  TELETRANSPORTE 

    private float teleportReadyTime; // Hasta este instante no puede volver a teletransportarse

    public bool CanTeleport { get { return Time.time >= teleportReadyTime; } }

    // Los teletransportadores llaman a esta función. Mantiene la velocidad.
    // cooldown = segundos hasta poder usar otro teletransportador.
    public void Teleport(Vector3 position, float cooldown)
    {
        teleportReadyTime = Time.time + cooldown;
        transform.position = position;
        rb.position = position;
    }

    // Los checkpoints llaman a esta función para guardar el punto de reaparición
    public void SetRespawn(Vector3 position)
    {
        respawnPoint = position;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        Collider2D[] cols = GetComponents<Collider2D>();
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = enabled;
    }
}
