using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ServerAPI
{
    public class ManagerLocal : IManager
    {
        public IEnumerator RequestRoutine(string endpoint, string data, string method, bool requireToken, bool requireWssToken, Action<string, int> callback)
        {
            // just to log in monitor
            UnityWebRequest www = BakeRequest(endpoint, data, method);
            API.CallRequestEvent(www, data);

            //fake delay
            float responseDelay = UnityEngine.Random.Range(API.settings.m_ResponseDelayMin, API.settings.m_ResponseDelayMax);
            yield return new WaitForSecondsRealtime(responseDelay);

            string response = LoadFile(endpoint);
            API.CallOnResponseEvent(www, data, response);

            if (string.IsNullOrEmpty(response))
                callback("", 400);
            else
                callback(response, 200);
        }

        private UnityWebRequest BakeRequest(string endpoint, string data, string method)
        {
            UnityWebRequest www = UnityWebRequest.Put(endpoint, data);
            www.method = method;
            string bearer = "Bearer " + API.loginToken;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", bearer);
            return www;
        }
        
        public static string LoadFile(string path)
        {
            TextAsset pText = Resources.Load<TextAsset>(path);

            string response = "";
            if (pText != null)
                response = pText.text;
            else
                Debug.LogError("not found: " + path);

            // so we can save and use the text again
            Resources.UnloadAsset(pText);
            
            return response;
        }
    }
}