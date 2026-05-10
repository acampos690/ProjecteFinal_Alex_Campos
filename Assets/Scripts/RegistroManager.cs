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
        if (string.IsNullOrEmpty(inputUser.text) || string.IsNullOrEmpty(inputPass.text))
        {
            Debug.LogWarning("Rellena todos los campos");
            return;
        }
        StartCoroutine(EnviarRegistroCo());
    }

    IEnumerator EnviarRegistroCo()
    {
        // Creamos el objeto JSON (sin Email)
        var datos = new
        {
            Username = inputUser.text,
            Password = inputPass.text
        };

        string json = JsonUtility.ToJson(datos);
        string url = $"{link}/publish/api/Users/register";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            www.certificateHandler = new GameManager.BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("¡Usuario registrado! Ahora puedes loguear.");
                panelRegistro.SetActive(false);
                panelLogin.SetActive(true);
            }
            else
            {
                Debug.LogError("Fallo al registrar: " + www.downloadHandler.text);
            }
        }
    }
}