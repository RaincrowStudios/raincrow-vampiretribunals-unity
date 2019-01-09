using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace ServerAPI
{
    public class ManagerServer : IManager
    {
        public IEnumerator RequestRoutine(string url, string data, string method, bool requireToken, bool requireWssToken, Action<string, int> callback)
        {
            UnityWebRequest www = BakeRequest(url, data, method, requireToken, requireWssToken);
            API.CallRequestEvent(www, data);

            // request
            yield return www.SendWebRequest();

            // receive the response
            API.CallOnResponseEvent(www, data, www.downloadHandler.text);

            if (www.isNetworkError)
            {
                Debug.LogError(www.responseCode.ToString() + "\n" + url);
                if (callback != null)
                    callback("", (int)www.responseCode);
            }
            else
            {
                if (callback != null)
                    callback(www.downloadHandler.text, Convert.ToInt32(www.responseCode));
            }
        }

        private UnityWebRequest BakeRequest(string endpoint, string data, string method, bool requireLoginToken, bool requiresWssToken)
        {
            //// log it
            //string sRequest = "==> BakeRequest for: " + endpoint;
            //sRequest += "\n  endpoint: " + endpoint;
            //sRequest += "\n  method: " + method;
            //sRequest += "\n  data: " + data;
            //sRequest += "\n  bRequiresLoginToken: " + requireLoginToken;
            //sRequest += "\n  bRequiresWssToken: " + requiresWssToken;
            //if (requireLoginToken) sRequest += "\n  loginToken: " + API.loginToken;
            //if (requiresWssToken) sRequest += "\n  wssToken: " + API.wssToken;
            //Debug.Log(sRequest);

            UnityWebRequest www;
            if (method == "GET")
            {
                www = UnityWebRequest.Get(endpoint);
            }
            else
            {
                www = UnityWebRequest.Put(endpoint, data);
                www.method = method;
            }
            www.SetRequestHeader("Content-Type", "application/json");
            if (requireLoginToken)
            {
                www.SetRequestHeader("Authorization", "Bearer " + API.loginToken);
            }
            if (requiresWssToken)
            {
                www.SetRequestHeader("Authorization", "Bearer " + API.wssToken);
            }

            return www;
        }
    }
}