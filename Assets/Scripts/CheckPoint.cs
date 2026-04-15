using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool checkPointActivo = false;
    private Animator animator;

    void Start()
    {
        // Obtenemos el Animator del objeto
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el jugador entra y el checkpoint aún no está activo
        if (collision.CompareTag("Player") && !checkPointActivo)
        {
            ActivarCheckPoint();
        }
    }

    void ActivarCheckPoint()
    {
        checkPointActivo = true;

        // Avisamos al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActuaizarCheckpoint(transform.position, this);
        }

        // Le decimos al Animator que cambie la animación
        if (animator != null)
        {
            animator.SetBool("activado", true);
        }
    }
}