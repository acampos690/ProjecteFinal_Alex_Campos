using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // Asegúrate de tener Json.NET en el proyecto

public class APIConnection : MonoBehaviour
{
    // Cambia el puerto si el tuyo es distinto al de la imagen
    [Header("Configuración API")]
    public string apiUrl = "https://localhost:44351/api/highscores";

    // --- CLASE PARA SALTARSE EL ERROR DE CERTIFICADO ---
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // Confía en cualquier certificado (solo para desarrollo)
        }
    }
    /*
    // --- MÉTODO PARA ENVIAR DATOS (POST) ---
    public IEnumerator PostHighScore(string name, int score)
    {
        // Creamos el objeto siguiendo tu modelo de la API
        var data = new
        {
            playerName = name,
            highScore = score
        };

        string body = JsonConvert.SerializeObject(data);

        // Configuramos la petición
        using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, body, "application/json"))
        {
            // Aplicamos el bypass de certificado
            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error API: " + www.error);
            }
            else
            {
                Debug.Log("¡Conexión exitosa! Datos guardados: " + body);
            }
        }
    }
    */
    // --- MÉTODO PARA PEDIR DATOS (GET) ---
    public IEnumerator GetHighScores(Action<string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {   
            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error API: " + www.error);
                callback(null);
            }
            else
            {
                callback(www.downloadHandler.text);
            }
        }
    }
}