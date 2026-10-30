using UnityEngine;

// La bala enemiga se mueve y daña al jugador al tocarlo.
public class EnemyBullet : MonoBehaviour
{
    private static Sprite squareSprite;
    private int damage;

    public void Setup(Vector2 direction, float speed, int damageAmount)
    {
        damage = damageAmount;

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        CircleCollider2D bulletCollider = gameObject.AddComponent<CircleCollider2D>();
        bulletCollider.isTrigger = true;
        bulletCollider.radius = 0.5f;

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
        else if (!other.isTrigger && other.GetComponent<EnemyIA>() == null)
        {
            Destroy(gameObject);
        }
    }

    public static Sprite GetSquareSprite()
    {
        if (squareSprite != null) return squareSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        squareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return squareSprite;
    }
}