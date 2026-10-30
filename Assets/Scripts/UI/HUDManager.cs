using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Muestra las vidas, monedas y nivel actual.
public class HUDManager : MonoBehaviour
{
    [Header("Corazones (Heart1, Heart2, Heart3)")]
    public Image[] hearts;
    public Sprite fullHeart;  // Opcional: si lo dejas vacío, los corazones se ocultan al perderlos
    public Sprite emptyHeart; // Opcional

    [Header("Vidas")]
    public TMP_Text livesText; // LivesText

    [Header("Monedas")]
    public TMP_Text coinText; // CoinText

    [Header("Información del nivel")]
    public TMP_Text levelInfoText; // LevelInfo: muestra "Nivel X"

    // Usamos Start (y no OnEnable) para que PlayerController y GameManager
    // ya hayan ejecutado su Awake.
    void Start()
    {
        PlayerController player = PlayerController.Instance;
        GameManager game = GameManager.Instance;

        if (player != null)
        {
            player.OnHealthChanged += UpdateHearts;
            UpdateHearts(player.Health, player.maxHealth);
        }

        if (game != null)
        {
            game.OnCoinsChanged += UpdateCoins;
            game.OnLivesChanged += UpdateLives;
            UpdateCoins(game.Coins);
            UpdateLives(game.Lives);
            UpdateLevel(game.CurrentLevel);
        }
    }

    // Nos damos de baja para no dejar suscripciones colgando
    void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.OnHealthChanged -= UpdateHearts;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoins;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
        }
    }

    //  ACTUALIZAR UI 

    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            bool isFull = i < current;
            bool exists = i < max; // Por si maxHealth es menor que el nº de corazones

            hearts[i].gameObject.SetActive(exists);

            if (fullHeart != null && emptyHeart != null)
                hearts[i].sprite = isFull ? fullHeart : emptyHeart;
            else
                hearts[i].enabled = isFull;
        }
    }

    private void UpdateCoins(int coins)
    {
        coinText.text = "x " + coins;
    }

    private void UpdateLives(int lives)
    {
        livesText.text = "x " + lives;
    }

    private void UpdateLevel(int level)
    {
        if (levelInfoText == null)
        {
            Debug.LogWarning("HUDManager: arrastra el objeto LevelInfo al campo 'Level Info Text'.");
            return;
        }

        levelInfoText.text = "Nivel - " + level;
    }
}
