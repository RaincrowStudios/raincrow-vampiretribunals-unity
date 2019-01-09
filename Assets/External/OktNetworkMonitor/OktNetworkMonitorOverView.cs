using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Oktagon.Network
{
    /// <summary>
    /// the network overview drawn on gui
    /// </summary>
    public class OktNetworkMonitorOverView : MonoBehaviour
    {
        public bool m_bClearButton = true;
        private OktNetworkMonitor m_pMonitor;
        string OverviewData;
        public Dictionary<string, Overview> m_pOverview = new Dictionary<string, Overview>();

        public class Overview
        {
            public string Key;
            public long TotalByte;
            public long Avarage;
            public int Count;
            public long Max;
        }


        private void Awake()
        {
            OktNetworkMonitor.OnDataUpdatedEvt += OktNetworkMonitor_OnDataUpdatedEvt;
            OktNetworkMonitor.OnMonitorStartEvt += OktNetworkMonitor_OnMonitorStartEvt;
        }

        private void OnDestroy()
        {
            OktNetworkMonitor.OnDataUpdatedEvt -= OktNetworkMonitor_OnDataUpdatedEvt;
            OktNetworkMonitor.OnMonitorStartEvt -= OktNetworkMonitor_OnMonitorStartEvt;
        }


        private void OktNetworkMonitor_OnMonitorStartEvt(OktNetworkMonitor pMonitor)
        {
            m_pMonitor = pMonitor;
        }

        private void OnGUI()
        {
            if (m_bClearButton && GUI.Button(new Rect(0, 100, 200, 50), "Clear"))
            {
                Clear();
            }
            //GUI.Box(new Rect(0, 150, Screen.width * 0.6f, 200), "");
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(1, 151, Screen.width * 0.6f, Screen.height - 150), OverviewData);
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(0, 150, Screen.width * 0.6f, Screen.height - 150), OverviewData);
        }


        public void Clear()
        {
            m_pOverview = new Dictionary<string, Overview>();
            OverviewData = "";
        }

        private void OktNetworkMonitor_OnDataUpdatedEvt(OktNetworkMonitor.RecordData pData)
        {
            string sOverKey = pData.Table;
            if (!m_pOverview.ContainsKey(sOverKey))
            {
                Overview pOver = new Overview();
                m_pOverview.Add(sOverKey, pOver);
            }

            // calculate overview
            long lTotalByte = pData.SizeResponse + pData.SizeRequest;
            Overview pOverview = m_pOverview[sOverKey];
            pOverview.Key = sOverKey;
            pOverview.TotalByte = pOverview.TotalByte + lTotalByte;
            pOverview.Avarage = pOverview.TotalByte / m_pMonitor.m_pDataStorage[sOverKey].Count;
            pOverview.Count++;
            if (lTotalByte > pOverview.Max)
                pOverview.Max = lTotalByte;

            // bake it
            BakeMonitor();
        }


        #region report bakery

        public void BakeMonitor()
        {
            // baking data results
            string sLog = "";
            int iCount = 0;
            long lTotalByte = 0;
            foreach (KeyValuePair<string, Overview> o in m_pOverview)
            {
                StringBuilder _sb1 = new StringBuilder();
                //_sb1.Append(o.Key.PadRight(25, '-'));
                _sb1.Append(string.Format("{0,-25:-}|", o.Key));
                _sb1.Append(string.Format("{0,5}", " c[" + o.Value.Count + "]"));
                _sb1.Append(string.Format("{0,15}", " t[" + OktNetworkMonitor.ToSizeString(o.Value.TotalByte) + "]"));
                _sb1.Append(string.Format("{0,10}", " max[" + OktNetworkMonitor.ToSizeString(o.Value.Max) + "]"));
                sLog += "\n" + _sb1.ToString();

                // counter
                iCount += o.Value.Count;
                lTotalByte += o.Value.TotalByte;
            }
            OverviewData = "PlayerIO Monitor count[" + iCount + "] total[" + OktNetworkMonitor.ToSizeString(lTotalByte) + "]" + sLog;
        }
        #endregion
    }
}