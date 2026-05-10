using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform player;
    public float detectarPlayer;
    public float speed;
    public int vida = 3;

    private Rigidbody2D rb;
    private float movementX;
    private bool enMovimiento;
    private bool muerto;
    private bool recibirDanyo;  
    public float fuerzaRebote = 5f;
    private Animator animator;
    private Vector3 escalaOriginal;
    private SpriteRenderer sr;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        escalaOriginal = transform.localScale;

        // Busca al objeto que tenga el Tag "Player" y guarda su Transform
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

    }

    void Update()
    {
        // En Update calculamos SOLO la dirección y las animaciones
        CalcularDireccion();
        Animaciones();
    }

    void FixedUpdate()
    {
        // En FixedUpdate aplicamos el movimiento físico real
        // Esto hace que la velocidad sea constante independientemente de los FPS
        AplicarMovimientoFisico();
    }

    private void CalcularDireccion()
    {
        if (player == null || muerto) return;

        float distancePlayer = Vector2.Distance(transform.position, player.position);

        // Verificamos si el player existe y no está muerto antes de seguirlo
        PlayerController pc = player.GetComponent<PlayerController>();
        
        if (distancePlayer < detectarPlayer && pc != null && !pc.muerto)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Girar el sprite según dirección
            if (direction.x < 0)
            {
                transform.localScale = new Vector3(escalaOriginal.x, escalaOriginal.y, escalaOriginal.z);
            }
            else if (direction.x > 0)
            {
                transform.localScale = new Vector3(-escalaOriginal.x, escalaOriginal.y, escalaOriginal.z);
            }


            movementX = direction.x;
            enMovimiento = true;
        }
        else
        {
            movementX = 0;
            enMovimiento = false;
        }

        // Lógica para recuperar el control después del rebote por daño
        if (recibirDanyo && rb.linearVelocity.magnitude < 0.1f)
        {
            recibirDanyo = false;
            rb.linearVelocity = Vector2.zero;
        }

    }

    private void AplicarMovimientoFisico()
    {
        if (!muerto && !recibirDanyo && enMovimiento)
        {
            // Usamos Time.fixedDeltaTime porque estamos dentro de FixedUpdate
            rb.linearVelocity = new Vector2(movementX * speed, rb.linearVelocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 direccionDanyo = new Vector2(transform.position.x, 0);
            GameManager.Instance.DanarJugador(1);
            collision.gameObject.GetComponent<PlayerController>().RecibirDanyo(direccionDanyo, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Espada"))
        {
            Vector2 direccionDanyo = new Vector2(collision.gameObject.transform.position.x, 0);
            RecibirDanyo(direccionDanyo, 1);
        }
    }

    public void RecibirDanyo(Vector2 direccion, int CantidadDanyo)
    {
        if (!recibirDanyo)
        {
            vida -= CantidadDanyo;
            recibirDanyo = true;

            if (vida <= 0)
            {
                muerto = true;
                enMovimiento = false;
                rb.linearVelocity = Vector2.zero; // Detenerse al morir
            }
            else
            {
                // Aplicar fuerza de rebote
                Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized;
                rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
            }
        }
    }

    public void DeletBody()
    {
        Destroy(gameObject);
    }

    public void Animaciones()
    {
        if (animator != null)
        {
            animator.SetBool("enMovimiento", enMovimiento);
            animator.SetBool("muerto", muerto);
        }
    }
}