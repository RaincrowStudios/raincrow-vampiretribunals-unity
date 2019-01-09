using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace ServerAPI
{    
    public class API : MonoBehaviour
    {
        public static API Instance { get; private set; }
        public static event Action<UnityWebRequest, string> OnRequestEvt;
        public static event Action<UnityWebRequest, string, string> OnResponseEvt;


        public static Settings settings { get { return Instance.m_Settings; } }
        public static string loginToken;
        public static string wssToken;

        [SerializeField] private Settings m_Settings;

        private IManager m_Manager;
              

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.transform.root.gameObject);
            Instance = this;

            if (settings.m_FakeServer)
                m_Manager = new ManagerLocal();
            else
                m_Manager = new ManagerServer();
        }




        public static void Request<T>(string endpoint, string data, string method, Action<T, int> callback = null, bool requireToken = true, bool requireWssToken = false)
        {
            Instance.StartCoroutine(Instance.m_Manager.RequestRoutine(settings.hostAddres + endpoint, data, method, requireToken, requireWssToken, (response, result) =>
            {
                if (result == 200)
                {
                    if (callback != null)
                    {
                        if (typeof(T) == typeof(string))
                        {
                            callback((T)(object)response, result);
                        }
                        else
                        {
                            T obj = JsonConvert.DeserializeObject<T>(response);
                            callback(obj, result);
                        }
                    }
                }
                else
                {
                    if (callback != null)
                        callback(default(T), result);
                }
            }));
        }

        public static void CallRequestEvent(UnityWebRequest pReq, string sRequestData)
        {
            if (OnRequestEvt != null)
                OnRequestEvt(pReq, sRequestData);
        }

        public static void CallOnResponseEvent(UnityWebRequest pRequest, string sRequestData, string sResponseData)
        {
            if (OnResponseEvt != null)
                OnResponseEvt(pRequest, sRequestData, sResponseData);
        }

        public static void GET<T>(string endpoint, string data, Action<T, int> callback = null, bool requireToken = true, bool requireWssToken = false)
        {
            Request<T>(endpoint, data, "GET", callback = null, requireToken, requireWssToken);
        }

        public static void PUT<T>(string endpoint, string data, Action<T, int> callback = null, bool requireToken = true, bool requireWssToken = false)
        {
            Request<T>(endpoint, data, "PUT", callback = null, requireToken, requireWssToken);
        }

        public static void POST<T>(string endpoint, string data, Action<T, int> callback = null, bool requireToken = true, bool requireWssToken = false)
        {
            Request<T>(endpoint, data, "POST", callback = null, requireToken, requireWssToken);
        }

        public static void DELETE<T>(string endpoint, string data, Action<T, int> callback = null, bool requireToken = true, bool requireWssToken = false)
        {
            Request<T>(endpoint, data, "DELETE", callback = null, requireToken, requireWssToken);
        }
    }
}