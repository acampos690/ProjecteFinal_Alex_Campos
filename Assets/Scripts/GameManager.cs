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
    public string progresoUrl = "https://localhost:44351/api/Progreso";

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

    [System.Serializable]
    public class DatosVida
    {
        // Datos de la tabla EstadisticasJugador
        public int vida_actual;
        public int vida_max;
    }

    public bool debeCargarPartida = false;
    public bool modoCargaActivado = false; // Solo true cuando pulsas "Cargar Partida"

    [System.Serializable]
    public class ProgresoJugador
    {
        // Datos de la tabla ProgresoJugador
        public string escena_nombre;
        public int nivel;
        public float respawn_x;
        public float respawn_y;
        public float respawn_z;
    }

    // --- CLASE AUXILIAR PARA EL CERTIFICADO SSL ---
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
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

        // Si entramos al menú, reseteamos flags para evitar bucles
        if (scene.name == "Menu")
        {
            debeCargarPartida = false;
            modoCargaActivado = false;
            return;
        }

        // Solo posicionamos y cargamos vida si venimos de pulsar "Continuar" o "Cargar"
        if (debeCargarPartida)
        {
            StartCoroutine(FinalizarCargaYPosicionar());
        }
    }

    // Corrutina para asegurar que el Player existe antes de moverlo
    IEnumerator FinalizarCargaYPosicionar()
    {
        // Esperamos a que termine el frame de carga
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Teletransporte
            player.transform.position = new Vector3(checkpointPosicion.x, checkpointPosicion.y + 0.5f, checkpointPosicion.z);
            Debug.Log("Jugador teletransportado a: " + player.transform.position);
        }

        // Cargamos la vida después del TP
        yield return StartCoroutine(CargarVidaDesdeAPI());

        // Reset de flags
        debeCargarPartida = false;
        modoCargaActivado = false;
    }


    // Modifica este método para que sea un Reset Real
    public void ResetearDatosPartidaNueva()
    {
        debeCargarPartida = false;
        modoCargaActivado = false;

        vidaActual = vidaMaxima;

        //reset posición
        checkpointPosicion = Vector3.zero;

        //limpiar checkpoint
        checkpointActual = null;

        Debug.Log("Nueva partida reseteada en memoria.");
    }
    void ActualizarPosicionInicial() //pone la posicion en la que esta colocada desde el unity
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            checkpointPosicion = player.transform.position;
            Debug.Log("Posición inicial guardada: " + checkpointPosicion);
        }
        else
        {
            Debug.LogWarning("No se encontró al Player para establecer la posición inicial.");
        }
    }

    public void BuscarUIManager()
    {
        //se busca el script en la escena 
        uiManager = GameObject.FindAnyObjectByType<UIManager>();

        if (uiManager == null)
        {
            Debug.LogWarning("Ojo: No hay UIManager en esta escena.");
        }
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

        //detenemos cualquier intento de carga que pueda estar en curso
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
    public void BotonGuardar()
    {
        Debug.Log("Guardando..");
        StartCoroutine(GuardarVidaEnAPI());
        StartCoroutine(GuardarProgresoEnAPI());
    }

    public void BotonCargarPartida()
    {
        modoCargaActivado = true;
        debeCargarPartida = true;
        StartCoroutine(CargarProgresoDesdeAPI());
    }

    public void BotonSalir()
    {
        SceneManager.LoadScene("Menu");

        Time.timeScale = 1f;//devuelvo el timepo a la normalidad

        Cursor.lockState = CursorLockMode.None; //para que el raton sea visible otra vez 
        Cursor.visible = true;
    }

    IEnumerator GuardarVidaEnAPI()
    {
        string url = $"{baseUrl}/ActualizarVida/{usuarioID}/{vidaActual}";
        using (UnityWebRequest www = UnityWebRequest.Put(url, ""))
        {
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();
        }
    }

    IEnumerator GuardarProgresoEnAPI()
    {
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        if (nombreEscenaActual == "Menu") yield break;

        string x = checkpointPosicion.x.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string y = checkpointPosicion.y.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string z = checkpointPosicion.z.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string url = $"{progresoUrl}/Actualizar/{usuarioID}/1/{nombreEscenaActual}/{x}/{y}/{z}";

        using (UnityWebRequest www = UnityWebRequest.Put(url, ""))
        {
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();
        }
    }

    

    public IEnumerator CargarProgresoDesdeAPI()
    {
        string url = $"{progresoUrl}/Obtener/{usuarioID}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var datos = JsonConvert.DeserializeObject<ProgresoJugador>(www.downloadHandler.text);
                checkpointPosicion = new Vector3(datos.respawn_x, datos.respawn_y, datos.respawn_z);

                // Cambiamos de escena. Al terminar, OnSceneLoaded se encargará de la vida y posición
                SceneManager.LoadScene(datos.escena_nombre);
            }
        }
    }

    public IEnumerator CargarVidaDesdeAPI()
    {
        string url = $"{baseUrl}/ObtenerVida/{usuarioID}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var datos = JsonConvert.DeserializeObject<DatosVida>(www.downloadHandler.text);
                vidaActual = datos.vida_actual;
                if (uiManager != null) uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
            }
        }
    }

    public IEnumerator ResetearProgresoEnAPI()
    {
        string escenaInicial = "GameEntrance";

        string x = "0";
        string y = "0";
        string z = "0";

        string url = $"{progresoUrl}/Actualizar/{usuarioID}/1/{escenaInicial}/{x}/{y}/{z}";

        using (UnityWebRequest www = UnityWebRequest.Put(url, ""))
        {
            www.certificateHandler = new BypassCertificate();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Progreso reseteado en la BD.");
            }
            else
            {
                Debug.LogError("Error al resetear progreso: " + www.error);
            }
        }
    }
}