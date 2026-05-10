using UnityEngine;

public class SensorCofre : MonoBehaviour
{
    public Animator chestAnimator;
    private bool isOpen = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            chestAnimator.SetBool("Open", true);
            isOpen = true;
        }
    }
}

