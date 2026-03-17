using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DBTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Start() se está ejecutando");
        StartCoroutine(ProbarConexion());
    }

    IEnumerator ProbarConexion()
    {
        Debug.Log("Iniciando petición...");

        UnityWebRequest www = UnityWebRequest.Get("http://ellaboratori.cat/files/alex/ConexionBDD.php");
        yield return www.SendWebRequest();

        Debug.Log("Petición completada");

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + www.error);
        }
    }
}
