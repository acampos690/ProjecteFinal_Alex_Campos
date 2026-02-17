using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectarPlayer;
    public float speed;

    private Rigidbody2D rb;
    private Vector2 movement; //direcció del moviment
    private bool enMovimiento;
    private bool recibirDanyo;
    public float fuerzaRebote = 5f;
    private Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float distancePlayer = Vector2.Distance(transform.position, player.position);//posicion entre el enemigo y el jugador

        if (distancePlayer < detectarPlayer)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            if (direction.x < 0)//si la velocidad es menor a cero se mueve hacia la izquierda
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (direction.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);//si la velovidad es mayor a cero se mueve hacia la derecha
            }

            movement = new Vector2(direction.x, 0);//la y no porque no vuela

            enMovimiento = true;
        }
        else
        {
            movement = Vector2.zero;//si sale fuera del rango el enemigo no se mueve
            enMovimiento = false;
        }

        if (recibirDanyo && rb.linearVelocity.magnitude < 0.1f) //es la velocidad total del rigidbody, cuando ya casi no se mueve se acabo el impulso del daño
        {
            recibirDanyo = false; //cuando casi no se mueva cambiamos a true para que se pueda mover otra vez
            rb.linearVelocity = Vector2.zero; //el rigid body vuelve a zero para que no acumule impulso
        }

        if (!recibirDanyo) //si no recibe daño se sigue moviendo hacia el player
        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);

        Animaciones();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 direccionDanyo = new Vector2(transform.position.x, 0);

            collision.gameObject.GetComponent<PlayerController>().RecibirDanyo(direccionDanyo, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Espada"))
        {
            Vector2 direccionDanyo = new Vector2(collision.gameObject.transform.position.x, 0);  //tener la posicion x de la espada

            RecibirDanyo(direccionDanyo, 1);
        }
    }
    public void RecibirDanyo(Vector2 direccion, int CantidadDanyo)
    {
        if (!recibirDanyo) //solo si no esta recibiendo daño porque si colisiona con dos enemigos recibe dañi infinito
        {
            recibirDanyo = true;
            Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized; //en el eje y solo un 1 para que salte poco
            rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
        }
    }

    public void Animaciones()
    {
        animator.SetBool("enMovimiento", enMovimiento);
    }
}
