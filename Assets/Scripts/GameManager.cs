using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json; // Necesario para procesar los datos de la API

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración API")]
    // Cambia el puerto si es necesario
    public string baseUrl = "https://localhost:44351/api/Estadisticas";

    [Header("Vida")]
    public int vidaMaxima = 6;
    public int vidaActual;

    [Header("UI")]
    public UIManager uiManager;

    [Header("Checkpoint")]
    public Vector3 checkpointPosicion;
    private Checkpoint checkpointActual;

    [Header("Datos Usuario")]
    public int usuarioID = 1; // ID de tu tabla MySQL

    // --- CLASE AUXILIAR PARA EL CERTIFICADO SSL ---
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    // --- CLASE PARA DESERIALIZAR EL JSON DE LA API ---
    private class EstadisticasData
    {
        public int vida_actual { get; set; }
        public int vida_max { get; set; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Cargar vida desde la base de datos MySQL a través de la API
        StartCoroutine(CargarVidaDesdeAPI());

        BuscarUIManager();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            checkpointPosicion = player.transform.position;
        }
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

        // Guardar vida en MySQL cada vez que cambia
        StartCoroutine(GuardarVidaEnAPI());
    }

    public void PausarJuego(bool pausar)
    {
        Time.timeScale = pausar ? 0 : 1;

        if (uiManager != null)
        {
            uiManager.MostrarMenuPausa(pausar);
        }
    }

    public void ActuaizarCheckpoint(Vector3 nuevaPosicion, Checkpoint nuevoCheckpoint)
    {
        checkpointPosicion = nuevaPosicion;
        checkpointActual = nuevoCheckpoint;
    }

    public void RespawnJugador()
    {
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {

        // 1. Detenemos cualquier intento de carga que pueda estar en curso
        StopCoroutine(nameof(CargarVidaDesdeAPI));

        yield return new WaitForSeconds(1f);

        vidaActual = vidaMaxima;

        if (uiManager == null)
        {
            uiManager = Object.FindFirstObjectByType<UIManager>();
        }

        if (uiManager != null)
        {
            Debug.Log("UI encontrada. Actualizando corazones a: " + vidaActual);
            uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerController>().Respawn(checkpointPosicion);
        }

        // Guardar vida reseteada en BD
        StartCoroutine(GuardarVidaEnAPI());
    }

    IEnumerator GuardarVidaEnAPI()
    {
        // Construimos la ruta exacta de tu Controller: api/Estadisticas/ActualizarVida/{usuarioId}/{vida}
        string url = $"{baseUrl}/ActualizarVida/{usuarioID}/{vidaActual}";

        // Usamos PUT porque tu API espera un HttpPut
        using (UnityWebRequest www = UnityWebRequest.Put(url, ""))
        {
            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al guardar en MySQL: " + www.error);
            }
            else
            {
                Debug.Log("Vida actualizada en MySQL con éxito.");
            }
        }
    }

    IEnumerator CargarVidaDesdeAPI()
    {
        // Ruta para obtener datos: api/Estadisticas/ObtenerVida/{usuarioId}
        string url = $"{baseUrl}/ObtenerVida/{usuarioID}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Leemos el JSON que devuelve la API
                    var datos = JsonConvert.DeserializeObject<EstadisticasData>(www.downloadHandler.text);

                    vidaActual = datos.vida_actual;
                    // vidaMaxima = datos.vida_max; // Opcional si quieres cargar también el tope

                    Debug.Log($"Datos cargados de MySQL. Vida actual: {vidaActual}");
                }
                catch
                {
                    Debug.LogError("Error al procesar el JSON de la API");
                    vidaActual = vidaMaxima;
                }
            }
            else
            {
                Debug.LogWarning("No se pudo conectar con la API para cargar. Usando valores por defecto.");
                vidaActual = vidaMaxima;
            }
        }

        // Refrescar UI tras la carga
        if (uiManager != null)
            uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
    }
}