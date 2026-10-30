using UnityEngine;

// Crea la bala del jugador y detecta sus impactos.
public class PlayerBullet : MonoBehaviour
{
    private const float LifeTime = 3f; // Se destruye sola pasado este tiempo

    private static Sprite squareSprite; // Se crea una vez y se reutiliza

    // CREACIÓN 

    public static void Spawn(Vector2 position, float direction, float speed)
    {
        GameObject go = new GameObject("PlayerBullet");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.25f;

        SpriteRenderer sprite = go.AddComponent<SpriteRenderer>();
        sprite.sprite = GetSquareSprite();
        sprite.color = Color.yellow;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Vuela recta
        rb.linearVelocity = new Vector2(direction * speed, 0f);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        go.AddComponent<PlayerBullet>();
        Destroy(go, LifeTime);
    }

    private static Sprite GetSquareSprite()
    {
        if (squareSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return squareSprite;
    }

    // IMPACTOS 

    void OnTriggerEnter2D(Collider2D other)
    {
        // Al jugador lo ignoramos (la bala nace pegada a él)
        if (other.GetComponent<PlayerController>() != null) return;

        // Enemigo: lo mata y la bala desaparece
        EnemyIA enemy = other.GetComponent<EnemyIA>();
        if (enemy != null)
        {
            enemy.Kill();
            Destroy(gameObject);
            return;
        }

        // Suelo o pared (collider sólido): la bala desaparece.
        // Los triggers (monedas, balas enemigas...) se atraviesan.
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
