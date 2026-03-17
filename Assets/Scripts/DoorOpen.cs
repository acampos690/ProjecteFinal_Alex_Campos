using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Animator doorAnimator; // arrastra aquí el Animator de la puerta
    public bool isOpen = false; // estado inicial de la puerta

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimator.SetBool("Open", true);
        }
    }

}
