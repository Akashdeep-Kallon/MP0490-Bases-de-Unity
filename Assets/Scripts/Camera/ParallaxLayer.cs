using UnityEngine;

// Hace que el fondo se mueva más despacio que la cámara.
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax")]
    [Range(-0.5f, 1f)]
    public float parallaxX = 0.5f; // 0 = se queda en el mundo, 1 = pegada a la cámara

    [Range(-0.5f, 1f)]
    public float parallaxY = 0.1f;
    public bool infiniteHorizontal = true;

    private Transform cam;
    private Vector3 startPos;
    private Vector3 camStartPos;
    private float tileWidth; // Ancho de UNA repetición del sprite, en unidades de mundo

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
        camStartPos = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        tileWidth = sr.sprite.bounds.size.x * Mathf.Abs(transform.lossyScale.x);
    }

    // LateUpdate: se mueve después de la cámara, así no tiembla
    void LateUpdate()
    {
        Vector3 camDelta = cam.position - camStartPos;

        float x = startPos.x + camDelta.x * parallaxX;
        float y = startPos.y + camDelta.y * parallaxY;

        if (infiniteHorizontal)
        {
            // La capa se desplaza respecto a la cámara (1 - parallaxX) por unidad.
            // Compensamos con saltos de un tile entero: el salto no se nota porque el patrón se repite.
            float drift = camDelta.x * (1f - parallaxX);
            x += Mathf.Round(drift / tileWidth) * tileWidth;
        }

        transform.position = new Vector3(x, y, startPos.z);
    }
}
