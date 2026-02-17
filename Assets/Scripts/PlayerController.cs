using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    public Animator animator;

    public float fuerzaSalto = 5f;
    public float fuerzaRebote = 5f;
    public float longitudRaycast = 0.1f;
    public LayerMask capaSuelo;

    public bool enSuelo;
    public bool recibirDanyo;
    private bool atacando;

    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //la variable rb obtiene el Rigidbody2D del personaje
        rb = GetComponent<Rigidbody2D>(); 
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
        if (!recibirDanyo) //solo si no esta recibiendo daño porque si colisiona con dos enemigos recibe dañi infinito
        {
            recibirDanyo = true;
            Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized; //en el eje y solo un 1 para que salte poco
            rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
        }
    }

    public void DesactivarDanyo()//para que pueda volver a recibir daño si hace falta
    {
        recibirDanyo = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void Movimiento()
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

        if(!recibirDanyo)
            transform.position = new Vector3(posicion.x + velocidadX, posicion.y, posicion.z);
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
    }
    void OnDrawGizmos()//sirve para poder ver la linea roja del Raycast dentro del editor
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * longitudRaycast);
    }
}
