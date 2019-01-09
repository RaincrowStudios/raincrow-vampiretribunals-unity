using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerAPI;

namespace Oktagon.Network
{
    public class ApiManagerMonitor : IMonitor
    {

        private OktNetworkMonitor m_pMonitor;
        private bool m_bRecord = true;

        public bool Record
        {
            get { return m_bRecord; }
            set { m_bRecord = value; }
        }


        /// <summary>
        /// set it up
        /// </summary>
        /// <param name="pMonitor"></param>
        public void SetupMonitor(OktNetworkMonitor pMonitor)
        {
            m_pMonitor = pMonitor;
            API.OnRequestEvt += APIManager_OnRequestEvt;
            API.OnResponseEvt += APIManager_OnResponseEvt;
        }

        public void Destroy()
        {
            m_pMonitor = null;
            API.OnRequestEvt -= APIManager_OnRequestEvt;
            API.OnResponseEvt -= APIManager_OnResponseEvt;
        }


        private void APIManager_OnRequestEvt(UnityEngine.Networking.UnityWebRequest obj, string sRequest)
        {
            // bake them
            OktNetworkMonitor.RecordData pData = new OktNetworkMonitor.RecordData();

            string sHead = obj.GetRequestHeader("Authorization");
            pData.Table = "UnityWebRequest";
            pData.Request = obj.url + "\n" + sRequest + "\nAuthorization:\n" + sHead;
            pData.RequestType = obj.method;
            pData.SizeRequest = 0;
            pData.ResponseType = "";
            pData.ReferenceId = obj;
            
#if UNITY_EDITOR
            // only collect stack on editor due to performance
            pData.Stack = UnityEngine.StackTraceUtility.ExtractStackTrace();
#endif

            // add it
            m_pMonitor.AddDataRequest(pData);
        }

        private void APIManager_OnResponseEvt(UnityEngine.Networking.UnityWebRequest obj, string sRequest, string sResponse)
        {
            //just tracking the response
            if (!Record)
                return;

            OktNetworkMonitor.RecordData pData = m_pMonitor.GetDataById(obj);
            // bake them
            if (pData == null)
            {
                pData = new OktNetworkMonitor.RecordData();
            }

            string sHead = obj.GetRequestHeader("Authorization");
            pData.Table = "UnityWebRequest";
            pData.Request = obj.url + "\n" + sRequest + "\nAuthorization:\n" + sHead;
            pData.RequestType = obj.method;
            pData.SizeRequest = 0;// System.Text.ASCIIEncoding.ASCII.GetByteCount(sJsonRequest);

            if (API.settings.m_FakeServer)
            {
                pData.Response = sResponse;
            }
            else
            {
                pData.Response = sResponse.Replace("{", "{\n").Replace("}", "\n}").Replace(",", ",\n");
            }

            pData.ResponseType = "";
            pData.SizeResponse = sResponse != null ? sResponse.Length : 0;
            m_pMonitor.AddDataResponse(pData);
        }
    }

}