using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Guarda las monedas y vidas y cambia de nivel.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Vidas")]
    public int startingLives = 3;

    //  ESTADO (solo lectura desde fuera) 

    public int Coins { get; private set; }
    public int Lives { get; private set; }

    // Nivel actual según el orden de la Scene List (Build Profiles): Level1 = 1, Level2 = 2...
    public int CurrentLevel { get { return SceneManager.GetActiveScene().buildIndex + 1; } }

    //  EVENTOS (el HUD se suscribe a estos) 

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnLivesChanged;

    //  INICIO 

    void Awake()
    {
        // Cada nivel puede tener su propio GameManager (para probar el nivel solo).
        // Si ya existe uno de un nivel anterior, nos quedamos con ese y borramos este.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetStats();
    }

    private void ResetStats()
    {
        Coins = 0;
        Lives = startingLives;
        OnCoinsChanged?.Invoke(Coins);
        OnLivesChanged?.Invoke(Lives);
    }

    // MONEDAS

    public void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    // Intenta gastar monedas. Devuelve false (y no gasta nada) si no hay suficientes.
    public bool TrySpendCoins(int amount)
    {
        if (Coins < amount) return false;

        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    //  VIDAS 

    // El jugador llama a esto cuando termina su animación de muerte.
    // Decide si reaparece o si la partida se acaba.
    public void HandlePlayerDeath(PlayerController player)
    {
        Lives--;
        OnLivesChanged?.Invoke(Lives);

        if (Lives > 0)
            player.Respawn();
        else
            RestartGame();
    }

    //  NIVELES 

    // La meta (Goal) llama a esto: carga el siguiente nivel de la Scene List.
    public void NextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            RestartGame(); // Era el último nivel: juego completado
    }

    // Game over o juego completado: todo desde cero
    private void RestartGame()
    {
        ResetStats();
        SceneManager.LoadScene(0); // El primer nivel de la Scene List
    }
}
