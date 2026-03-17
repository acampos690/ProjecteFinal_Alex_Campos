using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform objetivo;
    public float velocidadCamara = 0.025f;
    public Vector3 desplazamiento;

    private void LateUpdate()
    {
        Vector3 posicionDeseada = objetivo.position + desplazamiento;

        Vector3 posicionSuave = Vector3.Lerp(transform.position, posicionDeseada, velocidadCamara); //interpola entre la posición actual de la cámara y la posición deseada para suavizar el movimiento

        transform.position = posicionSuave; //actualiza la posición de la cámara a la posición suavizada
    }
}
