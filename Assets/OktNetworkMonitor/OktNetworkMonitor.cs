using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace Oktagon.Network
{

    /// <summary>
    /// a network monitor without any proxy
    /// </summary>
    public class OktNetworkMonitor : MonoBehaviour
    {
        private string m_LogFile = null;


        // Use this for initialization
        void Awake()
        {
            if(Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
            // collect monitors
            var type = typeof(IMonitor);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            foreach (Type mytype in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IMonitor))))
            {
                IMonitor pMon = Activator.CreateInstance(mytype) as IMonitor;
                m_pMonitorSources.Add(pMon);
                pMon.SetupMonitor(this);
            }

            Write("Monitor Started at " + System.DateTime.Now.ToString(), false);
            if (OnMonitorStartEvt != null)
                OnMonitorStartEvt(this);
        }


        private void OnDestroy()
        {
            for (int i = 0; i < m_pMonitorSources.Count; i++)
                m_pMonitorSources[i].Destroy();

            if (OnMonitorFinishEvt != null)
                OnMonitorFinishEvt(this);
        }

        public RecordData GetDataById(object pObj)
        {
            foreach(KeyValuePair<string, List<RecordData>> pair in m_pDataStorage)
            {
                foreach(RecordData pData in pair.Value)
                {
                    if (pData.ReferenceId == pObj)
                        return pData;
                }
            }
            return null;
        }

        public void AddDataRequest(RecordData pData)
        {
            AddData(pData, false);
        }
        public void AddDataResponse(RecordData pData)
        {
            Write(pData.ToString(m_WriteLog_Compact));
        }
        public void AddData(RecordData pData, bool bWrite = true)
        {
            string sOverKey = pData.Table;
            // setup if empty
            if (!m_pDataStorage.ContainsKey(sOverKey))
            {
                List<RecordData> pDataList = new List<RecordData>();
                m_pDataStorage.Add(sOverKey, pDataList);
            }

            // set new data
            m_pDataStorage[sOverKey].Add(pData);
            // write it
            if(bWrite)
                Write(pData.ToString(m_WriteLog_Compact));

            if (OnDataUpdatedEvt != null)
                OnDataUpdatedEvt(pData);
            if (OnMonitorUpdatedEvt != null)
                OnMonitorUpdatedEvt(this);
        }

        public void Clear()
        {
            m_pDataStorage = new Dictionary<string, List<RecordData>>();
        }


        #region Helpers

        public static double GetByteCount(string sString)
        {
            return System.Text.ASCIIEncoding.ASCII.GetByteCount(sString);
        }
        public static string StringSize(string sString)
        {
            return ToSizeString(GetByteCount(sString));
        }
        public static string ToSizeString(double bytes)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            const string format = "#,0.0";

            if (bytes < 1024)
                return bytes.ToString("#,0", culture) + " b";
            bytes /= 1024;
            if (bytes < 1024)
                return bytes.ToString(format, culture) + " KB";
            bytes /= 1024;
            if (bytes < 1024)
                return bytes.ToString(format, culture) + " MB";
            bytes /= 1024;
            if (bytes < 1024)
                return bytes.ToString(format, culture) + " GB";
            bytes /= 1024;
            return bytes.ToString(format, culture) + " TB";
        }
        
        public void Write(string sLog, bool bAppend = true)
        {
            if (!m_WriteLog)
                return;

            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");
            if (m_LogFile == null)
                m_LogFile = string.Format("Logs/OktNetworkLog-{0}.txt", DateTime.Now.ToString("dd-MM-HH-mm"));
            var writer = new StreamWriter(m_LogFile, bAppend);
            writer.Write(sLog);
            writer.Write("\n");
            writer.Flush();
            writer.Close();
        }
        #endregion


        #region Attributes

        public static OktNetworkMonitor Instance;

        [Header("Log Behaviour")]
        public bool m_WriteLog = true;
        public bool m_WriteLog_Compact = true;


        public Dictionary<string, List<RecordData>> m_pDataStorage = new Dictionary<string, List<RecordData>>();

        public delegate void OnDataUpdatedDelegate(RecordData pData);
        public delegate void OnMonitorEvent(OktNetworkMonitor pMonitor);

        public static event OnDataUpdatedDelegate OnDataUpdatedEvt;
        public static event OnMonitorEvent OnMonitorUpdatedEvt;
        public static event OnMonitorEvent OnMonitorStartEvt;
        public static event OnMonitorEvent OnMonitorFinishEvt;

        // monitors
        private List<IMonitor> m_pMonitorSources = new List<IMonitor>();

        #endregion


        #region Helper classes

        public class RecordData
        {
            public enum SizeType
            {
                None, Low, Medium, High, Extreme
            };

            public static int GlobalIndex;
            public int Index;
            public string RequestType;
            public string ResponseType;
            public string Table;
            public string Request;
            public string Response;
            public string Stack;
            public int CallbackCount = 0;
            public SizeType Size = SizeType.None;

            public long SizeResponse = 0;
            public long SizeRequest = 0;
            public System.DateTime Date;

            public object ReferenceId;

            static public float SizeLow = 1024 * 10;
            static public float SizeMedium = 1024 * 50;
            static public float SizeHigh = 1024 * 150;
            static public float SizeExtreme = 1024 * 10;

            public RecordData()
            {
                Date = System.DateTime.Now;
                Index = GlobalIndex++;
            }


            public SizeType GetSize()
            {
                if(Size == SizeType.None)
                {
                    SizeType eRequest = SizeType.None;
                    long iSize = SizeRequest;// / 1024;
                    if (iSize < SizeLow)                    eRequest = SizeType.Low;
                    else if (iSize < SizeMedium)            eRequest = SizeType.Medium;
                    else if (iSize < SizeHigh)              eRequest = SizeType.High;
                    else                                    eRequest = SizeType.Extreme;

                    SizeType eResponse = SizeType.None;
                    iSize = SizeResponse;// / 1024;
                    if (iSize < SizeLow)                    eResponse = SizeType.Low;
                    else if (iSize < SizeMedium)            eResponse = SizeType.Medium;
                    else if (iSize < SizeHigh)              eResponse = SizeType.High;
                    else                                    eResponse = SizeType.Extreme;

                    Size = (SizeType) (Math.Max((int)eRequest, (int)eResponse));
                }
                return Size;
            }

            public string GetHead()
            {
                return ""
                    + "Table[" + Table + "]"
                    + " Request[" + RequestType + "] "
                    + " Response[" + ResponseType + "]"
                    + " ->[" + ToSizeString(SizeRequest) + "]"
                    + " <-[" + ToSizeString(SizeResponse) + "]"
                    + " at " + Date.ToShortTimeString() + "."
                    ;
            }
            public string GetHeadMonitor(bool bShowKey = true)
            {
                return ""
                    + "[" + CallbackCount + "] "
                    + (bShowKey ? string.Format("{0,-25:-}|", Table) : "")
                    //+ string.Format(" ->[{0, 10}: {1, 10}] ", RequestType, ToSizeString(SizeRequest))
                    //+ string.Format(" ->[{0, 15}: {1, 15}] ", ResponseType, ToSizeString(SizeResponse))
                    //+ "Table[" + Table + "]"
                    + " ->[" + RequestType + ": "+ ToSizeString(SizeRequest) + "] "
                    + " <-[" + ResponseType + ": " + ToSizeString(SizeResponse) + "] "
                    //+ (CallbackCount > 0 ? " c["+ CallbackCount + "]" : "")
                    //+ " Request[" + RequestType + "] "
                    //+ " Response[" + ResponseType + "]"
                    //+ " ->[" + ToSizeString(SizeRequest) + "]"
                    //+ " <-[" + ToSizeString(SizeResponse) + "]"
                    + "    " + GetRequest(true, 100)
                    ;
            }
            public string ToString(bool bCompact)
            {
                return "------------------\n"
                    + "Key[" + Table + "]"
                    + " RequestType[" + RequestType + "] "
                    + " ResponseType[" + ResponseType + "]"
                    + " SizeResponse[" + ToSizeString(SizeResponse) + "]"
                    + " SizeRequest[" + ToSizeString(SizeRequest) + "]"
                    + " at " + Date.ToLongTimeString() + "."
                    + "\n  - Request: " + GetRequest(bCompact).Replace("\n", "\n\t\t") + "."
                    + "\n  - Response: " + GetResponse(bCompact).Replace("\n", "\n\t\t") + "."
                    + "\n  - Stack: " + GetStack(bCompact).Replace("\n", "\n\t\t") + "."
                    ;
            }
            public string GetStack(bool bCompact)
            {
                return Parse(Stack, bCompact);
            }
            public string GetRequest(bool bCompact, int iMax = -1)
            {
                if (string.IsNullOrEmpty(Request))
                {
                    return "";
                }
                if (iMax > 0 && Request.Length >= iMax -1)
                {
                    string s = Parse(Request, bCompact);
                    try
                    {
                        return s.Substring(0, iMax - 1);
                    }catch(Exception e) { }
                    return s;
                }
                return Parse(Request, bCompact);
            }
            public string GetResponse(bool bCompact)
            {
                if (string.IsNullOrEmpty(Response))
                {
                    return "";
                }
                return Parse(Response, bCompact);
            }
            public string Parse(string sValue, bool bCompact)
            {
                if (sValue == null)
                    sValue = "";
                if (bCompact)
                {
                    sValue = sValue
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Replace("\t", "")
                        ;
                }
                return sValue;
            }
        }
        #endregion

    }
}