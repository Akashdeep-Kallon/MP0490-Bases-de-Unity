using System.Collections.Generic;
using UnityEngine;

// Deja una estela del jugador al correr.
public class GhostTrail : MonoBehaviour
{
    [Header("Origen")]
    public SpriteRenderer source; // El SpriteRenderer del personaje

    [Header("Efecto")]
    public float spawnInterval = 0.05f; // Segundos entre cada fantasma
    public float lifetime = 0.3f;       // Cuánto tarda en desaparecer
    public Color tint = new Color(0.5f, 0.8f, 1f, 0.6f); // Color y opacidad inicial
    public int orderOffset = -1;        // -1 = justo detrás del personaje

    // Lo activa quien use el efecto (por ejemplo, el PlayerController)
    [HideInInspector] public bool emitting;

    private class Ghost
    {
        public SpriteRenderer renderer;
        public float age;
    }

    private readonly List<Ghost> active = new List<Ghost>();
    private readonly Stack<Ghost> pool = new Stack<Ghost>(); // Reutilizamos objetos en vez de crearlos siempre
    private float timer;

    void Update()
    {
        if (emitting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnGhost();
                timer = spawnInterval;
            }
        }
        else
        {
            timer = 0f; // El primer fantasma sale al instante al empezar a correr
        }

        FadeGhosts();
    }

    private void SpawnGhost()
    {
        Ghost ghost = pool.Count > 0 ? pool.Pop() : CreateGhost();

        // Copiamos posición, escala (incluye el volteo) y sprite actual
        Transform t = ghost.renderer.transform;
        t.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        t.localScale = source.transform.lossyScale;

        ghost.renderer.sprite = source.sprite;
        ghost.renderer.flipX = source.flipX;
        ghost.renderer.flipY = source.flipY;
        ghost.renderer.sortingLayerID = source.sortingLayerID;
        ghost.renderer.sortingOrder = source.sortingOrder + orderOffset;
        ghost.renderer.color = tint;

        ghost.age = 0f;
        ghost.renderer.gameObject.SetActive(true);
        active.Add(ghost);
    }

    private Ghost CreateGhost()
    {
        GameObject go = new GameObject("Ghost"); // Sin padre: se queda en el mundo, no sigue al personaje
        return new Ghost { renderer = go.AddComponent<SpriteRenderer>() };
    }

    private void FadeGhosts()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Ghost ghost = active[i];
            ghost.age += Time.deltaTime;
            float t = ghost.age / lifetime;

            if (t >= 1f)
            {
                ghost.renderer.gameObject.SetActive(false);
                pool.Push(ghost);
                active.RemoveAt(i);
                continue;
            }

            Color c = tint;
            c.a = tint.a * (1f - t); // Desvanecer
            ghost.renderer.color = c;
        }
    }
}