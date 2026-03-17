using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    public Animator animator;

    public bool muerto = false;
    public float fuerzaSalto = 5f;
    public float fuerzaRebote = 5f;
    public float longitudRaycast = 0.1f;
    public LayerMask capaSuelo;

    public bool enSuelo;
    public bool recibirDanyo;
    private bool atacando;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!atacando)
        {
            Movimiento();

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRaycast, capaSuelo);
            enSuelo = hit.collider != null;

            if (enSuelo && Input.GetKeyDown(KeyCode.Space) && !recibirDanyo)
            {
                rb.AddForce(new Vector2(0f, fuerzaSalto), ForceMode2D.Impulse);
            }
        }

        Animaciones();

        if (Input.GetKeyDown(KeyCode.LeftShift) && !atacando && enSuelo)
        {
            Atacando();
        }
    }

    public void RecibirDanyo(Vector2 direccion, int CantidadDanyo)
    {
        if (!muerto)
        {
            if (!recibirDanyo)
            {
                recibirDanyo = true;

                GameManager.Instance.DanarJugador(CantidadDanyo);

                if (GameManager.Instance.vidaActual > 0)
                {
                    Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized;
                    rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
                }
                else
                {
                    Morir();
                }
            }
        }
    }

    public void DesactivarDanyo()
    {
        recibirDanyo = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void Movimiento()
    {
        if (!muerto)
        {
            float velocidadX = Input.GetAxis("Horizontal") * 5f * Time.deltaTime;

            animator.SetFloat("movement", velocidadX);

            if (velocidadX < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (velocidadX > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            Vector3 posicion = transform.position;

            if (!recibirDanyo)
                transform.position = new Vector3(posicion.x + velocidadX, posicion.y, posicion.z);
        }
    }

    public void Morir()
    {
        muerto = true;
        animator.SetBool("muerto", true);
        gameObject.layer = LayerMask.NameToLayer("PlayerDead");
    }

    public void Atacando()
    {
        atacando = true;
    }

    public void DesactivarAtacando()
    {
        atacando = false;
    }

    public void Animaciones()
    {
        animator.SetBool("ensuelo", enSuelo);
        animator.SetBool("atacar", atacando);
        animator.SetBool("recibirDanyo", recibirDanyo);
        animator.SetBool("muerto", muerto);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * longitudRaycast);
    }
}
