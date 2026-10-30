using UnityEngine;

// Esta moneda se recoge al tocarla.
public class CoinManager : MonoBehaviour
{
    public int value = 1; // Cuánto suma esta moneda

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        GameManager.Instance.AddCoins(value);
        Destroy(gameObject);
    }
}
