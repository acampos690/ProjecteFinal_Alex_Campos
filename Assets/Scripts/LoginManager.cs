using Newtonsoft.Json;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputUsuario;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI textoError;
    public Button botonEntrar;

    [Header("Registro")]
    public GameObject panelRegistrar;
    public TMP_InputField regUsuario;
    public TMP_InputField regPassword;
    public TextMeshProUGUI textoErrorRegistro;

    const string link = "http://AdventureTime.somee.com";

    private string loginUrl = $"{link}/publish/api/Users/login";
    private string urlRegistro = $"{link}/publish/api/Users/register";

    void Start()
    {
        // Listener del botón de login
        textoError.text = "";
        botonEntrar.onClick.AddListener(OnBotonLoginClick);
    }

    public void OnBotonLoginClick()
    {
        StartCoroutine(ProcesoLogin());
    }

    // LOGIN
    IEnumerator ProcesoLogin()
    {
        string user = inputUsuario.text;
        string pass = inputPassword.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoError.text = "Introduce usuario y contraseña";
            yield break;
        }

        string urlFinal = $"{loginUrl}/{user}/{pass}";

        using (UnityWebRequest www = UnityWebRequest.Get(urlFinal))
        {
            www.certificateHandler = new GameManager.BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Guardo datos del usuario logueado
                User usuario = JsonConvert.DeserializeObject<User>(www.downloadHandler.text);
                GameManager.Instance.usuarioID = usuario.id;

                // Cambio a escena del menú
                SceneManager.LoadScene("Menu");
            }
            else
            {
                textoError.text = "Usuario o contraseña incorrectos";
            }
        }
    }

    // ABRIR / CERRAR REGISTRO
    public void BotonAbrirRegistrar()
    {
        panelRegistrar.SetActive(true);
    }

    public void BotonVolverLogin()
    {
        panelRegistrar.SetActive(false);
    }

    // REGISTRO
    public void OnBotonRegistrarClick()
    {
        StartCoroutine(ProcesoRegistro());
    }

    IEnumerator ProcesoRegistro()
    {
        string user = regUsuario.text;
        string pass = regPassword.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoErrorRegistro.text = "Rellena todos los campos";
            yield break;
        }

        var datos = new { Username = user, Password = pass };
        string json = JsonConvert.SerializeObject(datos);

        using (UnityWebRequest www = new UnityWebRequest(urlRegistro, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            www.certificateHandler = new GameManager.BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Registro correcto
                regUsuario.text = "";
                regPassword.text = "";
                panelRegistrar.SetActive(false);

                textoError.text = "Registro correcto";
            }
            else
            {
                textoErrorRegistro.text = "Error al registrar";
            }
        }
    }

    // Clase usuario (respuesta API)
    public class User
    {
        public int id;
        public string username;
    }
}