using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RegistroManager : MonoBehaviour
{
    public TMPro.TMP_InputField inputUser;
    public TMPro.TMP_InputField inputPass;

    public GameObject panelRegistro;
    public GameObject panelLogin;

    const string link = "http://AdventureTime.somee.com";

    public void OnClickRegistrar()
    {
        // Compruebo que los campos no estén vacíos
        if (string.IsNullOrEmpty(inputUser.text) || string.IsNullOrEmpty(inputPass.text))
        {
            Debug.LogWarning("Rellena todos los campos");
            return;
        }

        StartCoroutine(EnviarRegistroCo());
    }

    IEnumerator EnviarRegistroCo()
    {
        // Creo el JSON con los datos del usuario
        var datos = new
        {
            Username = inputUser.text,
            Password = inputPass.text
        };

        string json = JsonUtility.ToJson(datos);
        string url = $"{link}/publish/api/Users/register";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            // Envío de datos a la API
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Saltar certificado
            www.certificateHandler = new GameManager.BypassCertificate();

            yield return www.SendWebRequest();

            // Respuesta del servidor
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Usuario registrado correctamente");

                // Cambio de panel a login
                panelRegistro.SetActive(false);
                panelLogin.SetActive(true);
            }
            else
            {
                Debug.LogError("Error al registrar: " + www.downloadHandler.text);
            }
        }
    }
}