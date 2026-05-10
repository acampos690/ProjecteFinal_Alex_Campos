using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // Velocidad de movimiento del jugador
    public float speed = 5f;

    // Referencia al animator para controlar animaciones
    public Animator animator;

    // Estado de vida del jugador
    public bool muerto = false;

    // Fuerza del salto
    public float fuerzaSalto = 5f;

    // Fuerza del rebote cuando recibe daño
    public float fuerzaRebote = 5f;

    // Distancia del raycast para comprobar suelo
    public float longitudRaycast = 0.1f;

    // Velocidad base (por si luego la uso para boosts o cambios)
    public float velocidadBase = 5f;

    // Controla si el jugador está en boost de velocidad
    private bool enBoost = false;

    // Capa que considera como suelo
    public LayerMask capaSuelo;

    // Estados del jugador
    public bool enSuelo;
    public bool recibirDanyo;
    private bool atacando;

    // Rigidbody del jugador
    private Rigidbody2D rb;

    void Start()
    {
        // Cojo el Rigidbody al iniciar
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Si no está atacando puede moverse normalmente
        if (!atacando)
        {
            Movimiento();

            // Raycast hacia abajo para comprobar si está en el suelo
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRaycast, capaSuelo);
            enSuelo = hit.collider != null;

            // Salto si está en el suelo y pulsa espacio
            if (enSuelo && Input.GetKeyDown(KeyCode.Space) && !recibirDanyo)
            {
                rb.AddForce(new Vector2(0f, fuerzaSalto), ForceMode2D.Impulse);
            }
        }

        // Actualizo animaciones siempre
        Animaciones();

        // Ataque con Shift si no está atacando y está en el suelo
        if (Input.GetKeyDown(KeyCode.LeftShift) && !atacando && enSuelo)
        {
            Atacando();
        }

        // Si cae fuera del mapa, muere
        if (transform.position.y < -50f && !muerto)
        {
            Morir();
        }
    }

    // Boost de velocidad temporal
    public void ActivarBoost(float multiplicador, float duracion)
    {
        if (!enBoost)
            StartCoroutine(Boost(multiplicador, duracion));
    }

    private IEnumerator Boost(float multiplicador, float duracion)
    {
        enBoost = true;

        float velocidadOriginal = speed;
        speed = velocidadOriginal * multiplicador;

        // Espero la duración del boost
        yield return new WaitForSeconds(duracion);

        // Vuelvo a la velocidad normal
        speed = velocidadOriginal;
        enBoost = false;
    }

    // Función para recibir daño
    public void RecibirDanyo(Vector2 direccion, int CantidadDanyo)
    {
        if (!muerto)
        {
            if (!recibirDanyo)
            {
                recibirDanyo = true;

                // Le resto vida al jugador desde el GameManager
                GameManager.Instance.DanarJugador(CantidadDanyo);

                // Si todavía tiene vida, lo empujo hacia atrás
                if (GameManager.Instance.vidaActual > 0)
                {
                    Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized;
                    rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
                }
                else
                {
                    // Si no tiene vida, muere
                    Morir();
                }
            }
        }
    }

    // Reseteo el estado de daño
    public void DesactivarDanyo()
    {
        recibirDanyo = false;
        rb.linearVelocity = Vector2.zero;
    }

    // Movimiento horizontal del jugador
    public void Movimiento()
    {
        if (!muerto)
        {
            float velocidadX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

            animator.SetFloat("movement", velocidadX);

            // Giro del personaje según dirección
            if (velocidadX < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (velocidadX > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            Vector3 posicion = transform.position;

            // Solo se mueve si no está recibiendo daño
            if (!recibirDanyo)
                transform.position = new Vector3(posicion.x + velocidadX, posicion.y, posicion.z);
        }
    }

    // Muerte del jugador
    public void Morir()
    {
        muerto = true;
        animator.SetBool("muerto", true);

        // Llamo al respawn desde el GameManager
        GameManager.Instance.RespawnJugador();
    }

    // Respawn en checkpoint
    public void Respawn(Vector3 posicionCheckpoint)
    {
        muerto = false;
        recibirDanyo = false;
        atacando = false;

        transform.position = posicionCheckpoint;
        rb.linearVelocity = Vector2.zero;

        animator.SetBool("muerto", false);
        animator.SetBool("recibirDanyo", false);
    }

    // Activar ataque
    public void Atacando()
    {
        atacando = true;
    }

    // Desactivar ataque (lo llama la animación)
    public void DesactivarAtacando()
    {
        atacando = false;
    }

    // Control de animaciones
    public void Animaciones()
    {
        animator.SetBool("ensuelo", enSuelo);
        animator.SetBool("atacar", atacando);
        animator.SetBool("recibirDanyo", recibirDanyo);
        animator.SetBool("muerto", muerto);
    }

    // Debug visual del raycast en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * longitudRaycast);
    }
}