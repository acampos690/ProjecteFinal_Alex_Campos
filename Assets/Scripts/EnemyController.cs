using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectarPlayer;
    public float speed;
    public int vida = 3;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool enMovimiento;
    private bool muerto;
    private bool recibirDanyo;
    public float fuerzaRebote = 5f;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Movimiento();
        Animaciones();
    }

    private void Movimiento()
    {
        float distancePlayer = Vector2.Distance(transform.position, player.position);

        if (distancePlayer < detectarPlayer && !player.GetComponent<PlayerController>().muerto && !muerto)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            if (direction.x < 0)
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            else if (direction.x > 0)
            {
                transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            }

            movement = new Vector2(direction.x, 0);
            enMovimiento = true;
        }
        else
        {
            movement = Vector2.zero;
            enMovimiento = false;
        }

        if (recibirDanyo && rb.linearVelocity.magnitude < 0.1f)
        {
            recibirDanyo = false;
            rb.linearVelocity = Vector2.zero;
        }

        if (!recibirDanyo)
            rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
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
            }
            else
            {
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
        animator.SetBool("enMovimiento", enMovimiento);
        animator.SetBool("muerto", muerto);
    }
}
