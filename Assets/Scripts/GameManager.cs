using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Vida")]
    public int vidaMaxima = 6;
    public int vidaActual;

    [Header("UI")]
    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Detectar UIManager al cargar escenas
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        vidaActual = vidaMaxima;
        BuscarUIManager();
        uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuscarUIManager();

        if (uiManager != null)
        {
            uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
        }
    }

    void BuscarUIManager()
    {
        uiManager = Object.FindFirstObjectByType<UIManager>();
    }

    public void DanarJugador(int cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual < 0)
            vidaActual = 0;

        if (uiManager != null)
        {
            uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
        }
    }

    public void PausarJuego(bool pausar)
    {
        if (pausar)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;

        if (uiManager != null)
        {
            uiManager.MostrarMenuPausa(pausar);
        }
    }
}