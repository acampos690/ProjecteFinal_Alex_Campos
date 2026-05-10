using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class OpcionesMenu : MonoBehaviour
{
    public TMP_Dropdown dropdownResolucion;
    public Slider sliderVolumen;

    Resolution[] resoluciones;
    private string urlAjustes = $"{link}/publish/api/Ajustes";
    private bool datosCargados = false;

    const string link = "http://AdventureTime.somee.com";

    [System.Serializable]
    public class AjustesUsuario
    {
        public int usuarioId;
        public float volumen;
        public int resolucionIndex;
    }

    void Start()
    {
        ConfigurarDropdown();
        StartCoroutine(CargarAjustesDesdeAPI());
    }

    void OnEnable()
    {
        SincronizarInterfaz();
    }

    void ConfigurarDropdown()
    {
        resoluciones = Screen.resolutions;
        dropdownResolucion.ClearOptions();

        List<string> opciones = new List<string>();

        int indice1080p = -1;
        int indiceNativo = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + "x" + resoluciones[i].height;
            opciones.Add(opcion);

            //buscamos si existe la de 1080p
            if (resoluciones[i].width == 1920 && resoluciones[i].height == 1080)
            {
                indice1080p = i;
            }

            //por si acaso, guardamos la que el monitor tiene ahora mismo
            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                indiceNativo = i;
            }
        }

        dropdownResolucion.AddOptions(opciones);


        if (GameManager.Instance.resolucionIndexGuardada > 0)//si ya hay algo guardado en el GameManager (que no sea 0), usamos eso.
        {
            dropdownResolucion.value = GameManager.Instance.resolucionIndexGuardada;
        }
        else if (indice1080p != -1) //si no hay nada (es 0), intentamos poner 1080p.
        {
            dropdownResolucion.value = indice1080p;
            GameManager.Instance.resolucionIndexGuardada = indice1080p; // Actualizamos GameManager
        }
        else //si 1080p no existe, usamos la nativa del monitor.
        {
            dropdownResolucion.value = indiceNativo;
        }

        dropdownResolucion.RefreshShownValue();

        // Aplicamos la resolución físicamente para que no se quede en 640x480 al arrancar
        AplicarResolucionFisica(dropdownResolucion.value);
    }

    IEnumerator CargarAjustesDesdeAPI()
    {
        datosCargados = false;
        int id = GameManager.Instance.usuarioID;

        using (UnityWebRequest www = UnityWebRequest.Get($"{urlAjustes}/{id}"))
        {
            www.certificateHandler = new GameManager.BypassCertificate();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var ajustes = JsonConvert.DeserializeObject<AjustesUsuario>(www.downloadHandler.text);
                if (ajustes != null)
                {
                    GameManager.Instance.volumenGuardado = ajustes.volumen;
                    GameManager.Instance.resolucionIndexGuardada = ajustes.resolucionIndex;

                    // Aplicamos a la UI sin disparar guardados
                    SincronizarInterfaz();

                    // Aplicamos al motor
                    AudioListener.volume = ajustes.volumen;
                    AplicarResolucionFisica(ajustes.resolucionIndex);

                    datosCargados = true;
                }
            }
            else
            {
                datosCargados = true;
            }
        }
    }

    // --- NUEVO MÉTODO PARA EL BOTÓN DE GUARDAR ---
    public void BotonGuardarAjustes()
    {
        if (!datosCargados) return;

        // Primero actualizamos el GameManager con lo que hay en la UI
        GameManager.Instance.volumenGuardado = sliderVolumen.value;
        GameManager.Instance.resolucionIndexGuardada = dropdownResolucion.value;

        // Luego lo mandamos a la nube
        StartCoroutine(EnviarAjustesAPI());
    }

    IEnumerator EnviarAjustesAPI()
    {
        AjustesUsuario nuevosAjustes = new AjustesUsuario
        {
            usuarioId = GameManager.Instance.usuarioID,
            volumen = GameManager.Instance.volumenGuardado,
            resolucionIndex = GameManager.Instance.resolucionIndexGuardada
        };

        string json = JsonConvert.SerializeObject(nuevosAjustes);

        UnityWebRequest www = new UnityWebRequest(urlAjustes, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        www.certificateHandler = new GameManager.BypassCertificate();

        yield return www.SendWebRequest();

        Debug.Log("RESULT: " + www.result);
        Debug.Log("CODE: " + www.responseCode);
        Debug.Log("RESPONSE: " + www.downloadHandler.text);
        Debug.Log("ERROR: " + www.error);

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Ajustes guardados en BD con éxito.");
        }
        else
        {
            Debug.LogError("Error al guardar ajustes");
        }
    }

    void SincronizarInterfaz()
    {
        if (GameManager.Instance != null)
        {
            bool estadoPrevio = datosCargados;
            datosCargados = false; // Evita bucle infinito al cambiar valores

            sliderVolumen.value = GameManager.Instance.volumenGuardado;
            dropdownResolucion.value = GameManager.Instance.resolucionIndexGuardada;
            dropdownResolucion.RefreshShownValue();

            datosCargados = estadoPrevio;
        }
    }

    public void CambiarResolucion(int index)
    {
        if (resoluciones != null && index < resoluciones.Length)
        {
            Resolution r = resoluciones[index];

            // El tercer parámetro (true/false) indica si es pantalla completa
           //FullScreenMode.Windowed para que la ventana cambie de tamaño
            Screen.SetResolution(r.width, r.height, FullScreenMode.Windowed);

            // Guardamos el índice en el GameManager para que al darle a "Guardar" sepa qué enviar
            if (GameManager.Instance != null)
            {
                GameManager.Instance.resolucionIndexGuardada = index;
            }
 
            //obliga a la ventana de Windows/Mac a re-dibujarse.
            // #if !UNITY_EDITOR para que solo actúe en el juego buildeado
            #if !UNITY_EDITOR
                Screen.fullScreen = Screen.fullScreen; 
            #endif

            Debug.Log($"Resolución cambiada a: {r.width}x{r.height}");
        }
    }

    void AplicarResolucionFisica(int index)
    {
        if (resoluciones != null && index < resoluciones.Length)
        {
            Resolution r = resoluciones[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        }
    }

    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.volumenGuardado = valor;
        }
    }
}