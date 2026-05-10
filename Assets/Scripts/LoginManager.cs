using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputUsuario;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI textoError; // Para avisar si la clave está mal
    public Button botonEntrar;

    [Header("Panel Registro")]
    public GameObject panelRegistrar;
    public TMP_InputField regUsuario;
    public TMP_InputField regPassword;
    public TextMeshProUGUI textoErrorRegistro;

    [System.Serializable]
    public class User
    {
        public int id;
        public string username;
    }

    public class Register
    {
        public string Username;
        public string Password;
    }

    const string link = "http://AdventureTime.somee.com";

    private string loginUrl = $"{link}/publish/api/Users/login";
    private string urlRegistro = $"{link}/publish/api/Users/register";

 
    void Start()
    {
        textoError.text = "";
        botonEntrar.onClick.AddListener(OnBotonLoginClick);
    }

    public void OnBotonLoginClick()
    {
        StartCoroutine(ProcesoLogin());
    }

    IEnumerator ProcesoLogin()
    {
        string user = inputUsuario.text;
        string pass = inputPassword.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoError.text = "¡Introduce usuario y contraseña!";
            yield break;
        }

        // Construimos la URL: api/users/login/nombre/password
        string urlFinal = $"{loginUrl}/{user}/{pass}";

        using (UnityWebRequest www = UnityWebRequest.Get(urlFinal))
        {
            // Reutilizamos tu clase para saltar el certificado SSL si es necesario
            www.certificateHandler = new GameManager.BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 1. Convertimos la respuesta de la API a nuestro objeto User
                User usuarioLogueado = JsonConvert.DeserializeObject<User>(www.downloadHandler.text);

                // 2. IMPORTANTE: Guardamos el ID en el GameManager para que persista
                GameManager.Instance.usuarioID = usuarioLogueado.id;

                Debug.Log($"Bienvenido {usuarioLogueado.username}. ID guardado: {usuarioLogueado.id}");

                // 3. Vamos al Menú de Inicio
                SceneManager.LoadScene("Menu");
            }
            else
            {
                // Si la API devuelve 401 Unauthorized o error
                textoError.text = "Usuario o contraseña incorrectos";
                Debug.LogError("Error Login: " + www.error);
            }
        }
    }

        public void BotonAbrirRegistrar()
        {
            panelRegistrar.SetActive(true);
            textoErrorRegistro.text = "";
        }

        public void BotonVolverLogin()
        {
            panelRegistrar.SetActive(false);
            textoError.text = "";
        }

    public void OnBotonRegistrarClick()
    {
        StartCoroutine(ProcesoRegistro());
    }

    IEnumerator ProcesoRegistro()
    {
        string user = regUsuario.text; // Asegúrate de tener estas variables de los campos de registro
        string pass = regPassword.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoErrorRegistro.text = "¡Rellena todos los campos para registrarte!";
            yield break;
        }

        // 1. Creamos el objeto con los datos (debe coincidir con RegisterRequest de tu API)
        var datosRegistro = new { Username = user, Password = pass };
        string jsonDatos = JsonConvert.SerializeObject(datosRegistro);

        // 3. Creamos la petición POST
        using (UnityWebRequest www = new UnityWebRequest(urlRegistro, "POST"))
        {
            // Convertimos el JSON a bytes para el cuerpo de la petición
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonDatos);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // MUY IMPORTANTE: Decirle a la API que enviamos un JSON
            www.SetRequestHeader("Content-Type", "application/json");

            // Reutilizamos tu clase para saltar el certificado SSL
            www.certificateHandler = new GameManager.BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Usuario registrado con éxito: " + user);

                // 4. Limpiamos y volvemos al login
                regUsuario.text = "";
                regPassword.text = "";

                panelRegistrar.SetActive(false);    

                textoError.text = "¡Registro correcto! Ya puedes entrar.";
            }
            else
            {
                // Error si el usuario ya existe o fallo de servidor
                textoErrorRegistro.text = "Error al registrar: El usuario ya existe";
                Debug.LogError("Error Registro: " + www.downloadHandler.text);
            }
        }
    }
}

