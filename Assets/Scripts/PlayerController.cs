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

    [Header("Sistema de Vida")]
    public Image[] corazones;          // Los 3 corazones en UI

    public Sprite corazonLleno;
    public Sprite corazonMedio;
    public Sprite corazonVacio;

    public int vidaMaxima = 6;         // 3 corazones = 6 puntos
    private int vidaActual;

    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //la variable rb obtiene el Rigidbody2D del personaje
        rb = GetComponent<Rigidbody2D>(); 

        vidaActual = vidaMaxima;

        ActualizarCorazones();
    }

    // Update is called once per frame
    void Update()
    {
            if (!atacando)
            {
                Movimiento();

                //empieza desde la posicion del jugador, mira hacia abajo y va a ser del tamaño de la longitud
                //que le hemos asignado y busca colisionar con la capa del suelo          
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRaycast, capaSuelo);
                enSuelo = hit.collider != null;//si la linea colisiona con el suelo pasa as er verdadero

                if (enSuelo && Input.GetKeyDown(KeyCode.Space) && !recibirDanyo)
                {
                    //al rigid body le añadimos una fuerza al vector nuevo que en x es 0, y la fuerza sera Impulso
                    rb.AddForce(new Vector2(0f, fuerzaSalto), ForceMode2D.Impulse);
                }
            }

            Animaciones();

            if (Input.GetKeyDown(KeyCode.LeftShift) && !atacando && enSuelo)//si presionas shift, no esta atacando y esta en el suelo, ataca
            {
                Atacando();
            }
    }

    public void RecibirDanyo(Vector2 direccion, int CantidadDanyo)
    {
        if (!muerto)
        {
            if (!recibirDanyo) //solo si no esta recibiendo daño porque si colisiona con dos enemigos recibe dañi infinito
            {
                recibirDanyo = true;

                vidaActual -= CantidadDanyo;

                if (vidaActual <= 0)
                {
                    vidaActual = 0;
                }

                ActualizarCorazones();

                if (vidaActual == 0)
                {
                    Morir();
                }

                if (vidaActual > 0)
                {
                    Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized; //en el eje y solo un 1 para que salte poco
                rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void DesactivarDanyo()//para que pueda volver a recibir daño si hace falta
    {
        recibirDanyo = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void Movimiento()
    {
        if (!muerto)
        {
        float velocidadX = Input.GetAxis("Horizontal") * 5f * Time.deltaTime;

        // llama a la variable animator, y indica que el primer parametro es movement
        //y en el segundo parametro le pasa la variable velocidadX que es lo que queremos guardar
        animator.SetFloat("movement", velocidadX);

        if (velocidadX < 0)//si la velocidad es menor a cero se mueve hacia la izquierda
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (velocidadX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);//si la velovidad es mayor a cero se mueve hacia la derecha
        }

        Vector3 posicion = transform.position;

        if (!recibirDanyo)
            transform.position = new Vector3(posicion.x + velocidadX, posicion.y, posicion.z);
        }
    }

    public void ActualizarCorazones()
    {
        int vidaTemporal = vidaActual;

        Debug.Log("Actualizando corazones. Vida actual: " + vidaActual);

        for (int i = 0; i < corazones.Length; i++)
        {
            if (vidaTemporal >= 2)
            {
                corazones[i].sprite = corazonLleno;
                vidaTemporal -= 2;
            }
            else if (vidaTemporal == 1)
            {
                corazones[i].sprite = corazonMedio;
                vidaTemporal -= 1;
            }
            else
            {
                corazones[i].sprite = corazonVacio;
            }

            Debug.Log("Corazon " + i + " cambiado");
        }
    }

    public void Morir()
    {
        muerto = true;
        animator.SetBool("muerto", true);
        // Cambiar a layer que no colisiona con enemigos
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
        animator.SetBool("ensuelo", enSuelo);//le envia la variable en suelo a un booleano
        animator.SetBool("atacar", atacando);
        animator.SetBool("recibirDanyo", recibirDanyo);
        animator.SetBool("muerto", muerto);
    }
    void OnDrawGizmos()//sirve para poder ver la linea roja del Raycast dentro del editor
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * longitudRaycast);
    }
}
