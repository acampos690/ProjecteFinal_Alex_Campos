using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuInicio : MonoBehaviour
{
    public string nombreEscenaJuego = "GameEntrance";

    [Header("Animaciones")]
    public Animator animatorNuevaPartida;
    public Animator animatorCargarPartida;

    public GameObject menuPrincipal;
    public GameObject menuOpciones;


    public void BotonNuevaPartida()
    {
        if (animatorNuevaPartida != null)
            animatorNuevaPartida.SetBool("ClicNueva", true);

        GameManager.Instance.ResetearDatosPartidaNueva();

        StartCoroutine(NuevaPartidaCompleta());
    }

    public void BotonCargarPartida()
    {
        if (animatorCargarPartida != null)
            animatorCargarPartida.SetBool("CargarPartida", true);

        // Activamos modo carga
        GameManager.Instance.modoCargaActivado = true;
        GameManager.Instance.debeCargarPartida = true;

        StartCoroutine(CargarPartidaReal());
    }

    public void BotonOpciones()
    {
        menuPrincipal.SetActive(false);
        menuOpciones.SetActive(true);
    }

    public void BotonVolverMenu()
    {
        menuPrincipal.SetActive(true);
        menuOpciones.SetActive(false);
    }

    public void BottonVolverOpciones()
    {
        menuOpciones.SetActive(false);
        menuPrincipal.SetActive(true);
    }
    
    IEnumerator NuevaPartidaCompleta()
    {
        yield return new WaitForSeconds(1f);

        //borra progreso antiguo de la BD
        yield return StartCoroutine(GameManager.Instance.ResetearProgresoEnAPI());

        //cargaescena limpia
        SceneManager.LoadScene("GameEntrance");
    }
    
    IEnumerator CargarPartidaReal()
    {
        yield return new WaitForSeconds(1f);

        //Esto llama a la API y carga la escena guardada (Level1, etc.)
        StartCoroutine(GameManager.Instance.CargarProgresoDesdeAPI());
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}