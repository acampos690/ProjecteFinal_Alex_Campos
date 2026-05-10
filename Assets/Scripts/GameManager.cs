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
    private string baseUrl;
    private string progresoUrl;

    const string link = "http://AdventureTime.somee.com";

    [Header("Vida")]
    public int vidaMaxima = 6;
    public int vidaActual;

    [Header("UI")]
    public UIManager uiManager;

    [Header("Checkpoint")]
    public Vector3 checkpointPosicion;
    private Checkpoint checkpointActual;

    [Header("Datos Usuario")]
    public int usuarioID; // ID de tu tabla MySQL


    public float volumenGuardado = 0.5f;
    public int resolucionIndexGuardada = -1; // Cambiamos 0 por -1 para saber si es la primera vez+
    public int monedasActuales = 0; // Contador global de monedas

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
        public int monedas;
    }

    // --- CLASE AUXILIAR PARA EL CERTIFICADO SSL ---
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    void Awake()
    {
        baseUrl = $"{link}/publish/api/Estadisticas";
        progresoUrl = $"{link}/publish/api/Progreso";

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

        if (resolucionIndexGuardada == -1)
        {
            EstablecerResolucionPorDefecto();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuscarUIManager();

        AudioListener.volume = volumenGuardado;


        // Aplicar resolución guardada
        Resolution[] res = Screen.resolutions;
        if (resolucionIndexGuardada >= 0 && resolucionIndexGuardada < res.Length)
        {
            Resolution r = res[resolucionIndexGuardada];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            Debug.Log($"Resolución aplicada en {scene.name}: {r.width}x{r.height}");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActualizarTextoMonedas(monedasActuales);
        }

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

        if (uiManager != null)
        {
            uiManager.ActualizarCorazones(vidaActual, vidaMaxima);
        }

        // Reset de flags
        debeCargarPartida = false;
        modoCargaActivado = false;
    }

    public void EstablecerResolucionPorDefecto()
    {
        Resolution[] res = Screen.resolutions;
        // Buscamos 1920x1080 en la lista de resoluciones del monitor
        for (int i = 0; i < res.Length; i++)
        {
            if (res[i].width == 1920 && res[i].height == 1080)
            {
                resolucionIndexGuardada = i;
                Screen.SetResolution(1920, 1080, Screen.fullScreen);
                return;
            }
        }
        // Si no encuentra 1080p (monitor pequeño), ponemos la última de la lista (suele ser la nativa)
        resolucionIndexGuardada = res.Length - 1;
    }

    public void SumarMonedas(int cantidad)
    {
        monedasActuales += cantidad;

        // Aquí es donde conectamos con tu UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ActualizarTextoMonedas(monedasActuales);
        }

        Debug.Log("Monedas actuales: " + monedasActuales);
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

        string url = $"{progresoUrl}/Actualizar/{usuarioID}/1/{nombreEscenaActual}/{x}/{y}/{z}/{monedasActuales}";

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

                
                monedasActuales = datos.monedas;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ActualizarTextoMonedas(monedasActuales);
                }

                if (Application.CanStreamedLevelBeLoaded(datos.escena_nombre))
                {
                    Debug.Log("Intentando cargar escena: " + datos.escena_nombre);

                    yield return new WaitForSeconds(1f);

                    SceneManager.LoadScene(datos.escena_nombre);
                }
                else
                {
                    Debug.LogError("La escena no existe: " + datos.escena_nombre);
                }
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

        string url = $"{progresoUrl}/Actualizar/{usuarioID}/1/{escenaInicial}/{x}/{y}/{z}/0";

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