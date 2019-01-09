#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Oktagon.Network
{

    /// <summary>
    /// Wip Network monitor
    /// Features:
    ///  - Log requests/responses
    ///  - Log Overview
    ///  - Filtered log
    /// </summary>
    public class OktNetworkMonitorWindow : EditorWindow
    {

        #region attrs

        // data
        private OktNetworkMonitor m_pMonitor;
        private Dictionary<string, List<OktNetworkMonitor.RecordData>> m_pDatas;

        // log
        private List<OktNetworkMonitor.RecordData> m_pToDraw;
        private bool m_bCollecting = true;
        private List<int> m_vSelectedIndex = new List<int>();
        // filter
        private List<string> FilterTableList = new List<string>();
        private List<string> FilterTableIgnoreList = new List<string>();
        private List<string> FilterRequestTypeList = new List<string>();
        private List<string> FilterRequestTypeIgnoreList = new List<string>();
        private bool m_bFilter = true;
        private bool m_bShowCallStack = true;
        private bool m_bShowRequest = true;
        private bool m_bShowResponse = true;
        private bool m_bShowKey = true;
        private List<string> m_pMonitorKeys;

        private bool m_bSortByIndex = true;

        private GUIStyle m_pNormal = GUIStyle.none;
        private GUIStyle m_pMedium = GUIStyle.none;
        private GUIStyle m_pHigh = GUIStyle.none;
        private Vector2 m_vScrollView_Logs = Vector2.zero;

        public bool Enabled
        {
            get { return m_pMonitor != null; }
        }

        public string FilterTables
        {
            get { return EditorPrefs.GetString("OktNetworkMonitorWindow.FilterTables", ""); }
            set { EditorPrefs.SetString("OktNetworkMonitorWindow.FilterTables", value); }
        }

        public string FilterTablesIgnore
        {
            get { return EditorPrefs.GetString("OktNetworkMonitorWindow.STablesIgnore", ""); }
            set { EditorPrefs.SetString("OktNetworkMonitorWindow.STablesIgnore", value); }
        }

        public string FilterRequestType
        {
            get { return EditorPrefs.GetString("OktNetworkMonitorWindow.FilterRequestType", ""); }
            set { EditorPrefs.SetString("OktNetworkMonitorWindow.FilterRequestType", value); }
        }
        public string FilterRequestTypeIgnore
        {
            get { return EditorPrefs.GetString("OktNetworkMonitorWindow.FilterRequestTypeIgnore", ""); }
            set { EditorPrefs.SetString("OktNetworkMonitorWindow.FilterRequestTypeIgnore", value); }
        }

        public bool ShowCallStack
        {
            get { return EditorPrefs.GetInt("OktNetworkMonitorWindow.ShowCallStack", 1) > 0; }
            set { EditorPrefs.SetInt("OktNetworkMonitorWindow.ShowCallStack", value ? 1 : 0); }
        }
        public bool ShowRequest
        {
            get { return EditorPrefs.GetInt("OktNetworkMonitorWindow.ShowRequest", 1) > 0; }
            set { EditorPrefs.SetInt("OktNetworkMonitorWindow.ShowRequest", value ? 1 : 0); }
        }
        public bool ShowResponse
        {
            get { return EditorPrefs.GetInt("OktNetworkMonitorWindow.ShowResponse", 1) > 0; }
            set { EditorPrefs.SetInt("OktNetworkMonitorWindow.ShowResponse", value ? 1 : 0); }
        }
        public bool ShowKey
        {
            get { return EditorPrefs.GetInt("OktNetworkMonitorWindow.ShowKey", 1) > 0; }
            set { EditorPrefs.SetInt("OktNetworkMonitorWindow.ShowKey", value ? 1 : 0); }
        }
        public float YellowResponse
        {
            get { return EditorPrefs.GetFloat("OktNetworkMonitorWindow.YellowResponse", 1);  }
            set { EditorPrefs.SetFloat("OktNetworkMonitorWindow.YellowResponse", 0); }
        }
        public float RedResponse
        {
            get { return EditorPrefs.GetFloat("OktNetworkMonitorWindow.RedResponse", 1); }
            set { EditorPrefs.SetFloat("OktNetworkMonitorWindow.RedResponse", 0); }
        }

        private List<string> MonitorKeys
        {
            get
            {
                if(m_pMonitorKeys == null)
                {
                    string sStored = EditorPrefs.GetString("OktNetworkMonitorWindow.MonitorKeys", "");
                    string[] vStored = sStored.Split(';');
                    m_pMonitorKeys = new List<string>(vStored);
                }
                return m_pMonitorKeys;
            }
        }

        #endregion


        #region init

        [MenuItem("Raincrow/Tools/Network Monitor")]
        static void Init()
        {
            Texture pIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Scripts/Oktagon/OktNetworkMonitor/Editor/Icons/PerformanceMonitor.png");
            OktNetworkMonitorWindow window;
            window = EditorWindow.CreateInstance(typeof(OktNetworkMonitorWindow)) as OktNetworkMonitorWindow;
            window.titleContent = new GUIContent("Net Monitor");
            window.titleContent = window.titleContent = new GUIContent("Net Monitor", pIcon);
            window.Show();
            window.LoadSettings();
        }
        static System.DateTime m_LastLoad = new System.DateTime(0);


        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            CheckEvents();
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        void CheckEvents()
        {
            if(m_LastLoad.Year != System.DateTime.Today.Year || m_LastLoad.Day != System.DateTime.Today.Day)
            {
                OktNetworkMonitor.OnMonitorUpdatedEvt -= NetMonitor_OnMonitorUpdatedEvt;
                OktNetworkMonitor.OnDataUpdatedEvt -= NetMonitor_OnDataUpdatedEvt;
                OktNetworkMonitor.OnMonitorStartEvt -= NetMonitor_OnMonitorStartEvt;
                OktNetworkMonitor.OnMonitorFinishEvt -= NetMonitor_OnMonitorFinishEvt;
                OktNetworkMonitor.OnMonitorUpdatedEvt += NetMonitor_OnMonitorUpdatedEvt;
                OktNetworkMonitor.OnDataUpdatedEvt += NetMonitor_OnDataUpdatedEvt;
                OktNetworkMonitor.OnMonitorStartEvt += NetMonitor_OnMonitorStartEvt;
                OktNetworkMonitor.OnMonitorFinishEvt += NetMonitor_OnMonitorFinishEvt;
                NetMonitor_OnMonitorUpdatedEvt(OktNetworkMonitor.Instance);
            }
        }

        protected void LoadSettings()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

            CheckEvents();

            m_pNormal = new GUIStyle();
            m_pNormal.normal.textColor = Application.HasProLicense() ? Color.white : Color.black;
            m_pMedium = new GUIStyle();
            m_pMedium.normal.textColor = Color.yellow;
            m_pHigh = new GUIStyle();
            m_pHigh.normal.textColor = Color.red;

            m_bShowCallStack = ShowCallStack;
            m_bShowRequest = ShowRequest;
            m_bShowResponse = ShowResponse;
            m_bShowKey = ShowKey;

            ParseFilter();
        }
        private void OnDestroy()
        {
            OktNetworkMonitor.OnMonitorUpdatedEvt -= NetMonitor_OnMonitorUpdatedEvt;
            OktNetworkMonitor.OnDataUpdatedEvt -= NetMonitor_OnDataUpdatedEvt;
            OktNetworkMonitor.OnMonitorStartEvt -= NetMonitor_OnMonitorStartEvt;
            OktNetworkMonitor.OnMonitorFinishEvt -= NetMonitor_OnMonitorFinishEvt;
        }



        #endregion


        #region draw
		protected void OnGUI()
        {
            CheckEvents();
            //EditorGUILayout.LabelField("==> " + m_iSelectedIndex);
            // top controller
            DrawTop();

            // filter
            DrawFilter();

            // overview
            DrawOverview();

            // logs
            DrawLogs();
        }


        void DrawTop()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear"))
            {
                ClearLog();
            }
            if (GUILayout.Button("Restart"))
            {
                LoadSettings();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawFilter()
        {
            m_bFilter = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_bFilter, "Table Filter (" + FilterTables + ") Ignore(" + FilterTablesIgnore + ")", true);
            if (m_bFilter)
            {
                
                // contains table
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);
                EditorGUILayout.LabelField("Table", GUILayout.Width(70));
                string sTable = EditorGUILayout.TextField(FilterTables);
                EditorGUILayout.EndHorizontal();
                // ignore table
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);
                EditorGUILayout.LabelField("Table IG", GUILayout.Width(70));
                string sTableIgnore = EditorGUILayout.TextField(FilterTablesIgnore);
                EditorGUILayout.EndHorizontal();
                // contains type
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);
                EditorGUILayout.LabelField("RType", GUILayout.Width(70));
                string sRequestType = EditorGUILayout.TextField(FilterRequestType);
                EditorGUILayout.EndHorizontal();
                // contains type
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);
                EditorGUILayout.LabelField("RType IG", GUILayout.Width(70));
                string sRequestTypeIgnore = EditorGUILayout.TextField(FilterRequestTypeIgnore);
                EditorGUILayout.EndHorizontal();

                // Toggle options
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);
                bool bSortByIdx = EditorGUILayout.ToggleLeft("Sort by index", m_bSortByIndex);
                bool bShowCallStack = EditorGUILayout.ToggleLeft("Show Call Stack", m_bShowCallStack);
                bool bShowRequest = EditorGUILayout.ToggleLeft("Show Requests", m_bShowRequest);
                bool bShowResponse = EditorGUILayout.ToggleLeft("Show Responses", m_bShowResponse);
                bool bShowKey = EditorGUILayout.ToggleLeft("Show Key", m_bShowKey);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                if (bSortByIdx != m_bSortByIndex)
                {
                    m_bSortByIndex = bSortByIdx;
                    BuildDataToDraw();
                }
                if (sTable != FilterTables || sTableIgnore != FilterTablesIgnore || sRequestType != FilterRequestType || sRequestTypeIgnore != FilterRequestTypeIgnore)
                {
                    FilterTables = sTable;
                    FilterTablesIgnore = sTableIgnore;
                    FilterRequestType = sRequestType;
                    FilterRequestTypeIgnore = sRequestTypeIgnore;
                    ParseFilter();
                    BuildDataToDraw();
                }

                if(bShowCallStack != m_bShowCallStack)
                {
                    m_bShowCallStack = bShowCallStack;
                    ShowCallStack = m_bShowCallStack;
                }
                if (bShowRequest != m_bShowRequest)
                {
                    m_bShowRequest = bShowRequest;
                    ShowRequest = m_bShowRequest;
                }
                if (bShowResponse != m_bShowResponse)
                {
                    m_bShowResponse = bShowResponse;
                    ShowResponse = m_bShowResponse;
                }
                if (bShowKey != m_bShowKey)
                {
                    m_bShowKey = bShowKey;
                    ShowKey = m_bShowKey;
                }
                

                // filter
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);

                for(int i =0; i < MonitorKeys.Count; i++)
                {
                    if (string.IsNullOrEmpty(MonitorKeys[i]))
                        continue;
                    if (GUILayout.Button(MonitorKeys[i]))
                    {
                        if (!FilterTableList.Contains(MonitorKeys[i]))
                        {
                            FilterTableList.Add(MonitorKeys[i]);
                            FilterTables = ToArrayString(FilterTableList.ToArray(), ';');
                            ParseFilter();
                            BuildDataToDraw();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }
        }

        void EditorGUILayoutSpace(float fSize)
        {
            EditorGUILayout.LabelField("", GUILayout.Width(fSize));
        }

        void ParseFilter()
        {
            // parse tables to collect
            if (!string.IsNullOrEmpty(FilterTablesIgnore))
            {
                string[] sFilter = FilterTablesIgnore.Split(';');
                FilterTableIgnoreList = new List<string>(sFilter);
            }
            else FilterTableIgnoreList = new List<string>();
            // parse tables to ignore
            if (!string.IsNullOrEmpty(FilterTables))
            {
                string[] sFilter = FilterTables.Split(';');
                FilterTableList = new List<string>(sFilter);
            }
            else FilterTableList = new List<string>();
            // parse request to collect
            if (!string.IsNullOrEmpty(FilterRequestType))
            {
                string[] sFilter = FilterRequestType.Split(';');
                FilterRequestTypeList = new List<string>(sFilter);
            }
            else FilterRequestTypeList = new List<string>();
            // parse request to collect
            if (!string.IsNullOrEmpty(FilterRequestTypeIgnore))
            {
                string[] sFilter = FilterRequestTypeIgnore.Split(';');
                FilterRequestTypeIgnoreList = new List<string>(sFilter);
            }
            else FilterRequestTypeIgnoreList = new List<string>();

        }

        void DrawOverview()
        {
        }
        
        void DrawLogs()
        {
            if (m_pDatas == null || m_pDatas.Count <= 0)
            {
                //m_iSelectedIndex = -1;
                m_vSelectedIndex = new List<int>();
                EditorGUILayout.LabelField("null data...");
                return;
            }

            m_vScrollView_Logs = EditorGUILayout.BeginScrollView(m_vScrollView_Logs);

            int iCound = m_pToDraw.Count;
            for (int i = 0; i < iCound; i++)
            {
                DrawData(m_pToDraw[i], i);
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawData(OktNetworkMonitor.RecordData pData, int iIdx)
        {
            if (pData == null)
                return;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            //bool bSelected = EditorGUILayout.Toggle(pData.Index == m_iSelectedIndex, GUILayout.Width(20));
            bool bContainsSelection = m_vSelectedIndex.Contains(pData.Index);
            bool bSelected = false;
            //switch (pData.GetSize())
            //{
            //    case OktNetworkMonitor.RecordData.SizeType.Extreme:
            //    case OktNetworkMonitor.RecordData.SizeType.High:
            //        bSelected = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), bContainsSelection, iIdx + ". " + pData.GetHeadMonitor(ShowKey), true, m_pHigh);
            //        break;
            //    case OktNetworkMonitor.RecordData.SizeType.Medium:
            //        bSelected = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), bContainsSelection, iIdx + ". " + pData.GetHeadMonitor(ShowKey), true, m_pMedium);
            //        break;
            //    default:
            //        bSelected = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), bContainsSelection, iIdx + ". " + pData.GetHeadMonitor(ShowKey), true, m_pNormal);
            //        break;
            //}

            //Color color = GUI.contentColor;
            //if (Application.HasProLicense())
            //    GUI.contentColor = Color.white;
            //bSelected = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), bContainsSelection, iIdx + ". " + pData.GetHeadMonitor(ShowKey), true, m_pNormal);
            //GUI.contentColor = color;
            bSelected = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), bContainsSelection, iIdx + ". " + pData.GetHeadMonitor(ShowKey), true);

            if (!bSelected && bContainsSelection)
                m_vSelectedIndex.Remove(pData.Index);
            if (bSelected)
            {
                if (!bContainsSelection)
                    m_vSelectedIndex.Add(pData.Index);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayoutSpace(15);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (m_bShowRequest)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("  ->", GUILayout.Width(25));
                    EditorGUILayout.TextArea(pData.Request);
                    EditorGUILayout.EndHorizontal();
                }

                if (m_bShowResponse)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("  <-", GUILayout.Width(25));
                    EditorGUILayout.TextArea(pData.GetResponse(false));
                    EditorGUILayout.EndHorizontal();
                }

                if (m_bShowCallStack)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("  st", GUILayout.Width(25));
                    EditorGUILayout.TextArea(pData.GetStack(false));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
                EditorGUILayout.EndHorizontal();
        }

        #endregion


        #region draw batch

        void BuildDataToDraw()
        {
            if (m_pDatas == null || m_pDatas.Count <= 0)
                return;
            // bake a list
            List<OktNetworkMonitor.RecordData> pToDraw = new List<OktNetworkMonitor.RecordData>();
            foreach (KeyValuePair<string, List<OktNetworkMonitor.RecordData>> datas in m_pDatas)
            {
                // filter the table
                if (FilterTableList.Count > 0) //&& !FilterTableList.Contains(datas.Key))
                {
                    bool bContinue = true;

                    foreach(string sFilter in FilterTableList)
                    {
                        if (datas.Key.ToLower().Contains(sFilter.ToLower()))
                            bContinue = false;
                    }

                    if (bContinue)
                        continue;
                }
                if (FilterTableIgnoreList.Count > 0 && FilterTableIgnoreList.Contains(datas.Key))
                    continue;

                int iCount = datas.Value.Count;
                for (int i = 0; i < iCount; i++)
                {
                    if (datas.Value[i] == null)
                        continue;
                    // filter the request type
                    if (FilterRequestTypeList != null && FilterRequestTypeList.Count > 0 && !FilterRequestTypeList.Contains(datas.Value[i].RequestType))
                        continue;
                    if (FilterRequestTypeIgnoreList != null && FilterRequestTypeIgnoreList.Count > 0 && FilterRequestTypeIgnoreList.Contains(datas.Value[i].RequestType))
                        continue;

                    pToDraw.Add(datas.Value[i]);
                    AddKey(datas.Key);
                }
            }
            // should we sort by index?
            if (m_bSortByIndex)
            {
                pToDraw.Sort(delegate (OktNetworkMonitor.RecordData x, OktNetworkMonitor.RecordData y)
                {
                    return x.Index.CompareTo(y.Index);
                });
            }
            m_pToDraw = pToDraw;
            Repaint();
        }

        private void ClearLog()
        {
            OktNetworkMonitorOverView pOver = GameObject.FindObjectOfType<OktNetworkMonitorOverView>();
            if (pOver != null) pOver.Clear();
            m_pDatas = null;
            if(m_pMonitor)
                m_pMonitor.Clear();
            BuildDataToDraw();
        }

        #endregion


        #region events

        private void NetMonitor_OnMonitorStartEvt(OktNetworkMonitor pMonitor)
        {
            m_pMonitor = pMonitor;
        }
        private void NetMonitor_OnMonitorFinishEvt(OktNetworkMonitor pMonitor)
        {
            m_pMonitor = null;
        }
        private void NetMonitor_OnMonitorUpdatedEvt(OktNetworkMonitor pMonitor)
        {
            if (m_bCollecting)
            {
                m_pMonitor = pMonitor;
                if(pMonitor)
                    m_pDatas = pMonitor.m_pDataStorage;
                BuildDataToDraw();
            }
        }
        private void NetMonitor_OnDataUpdatedEvt(OktNetworkMonitor.RecordData pData)
        {
        }

        #endregion


        #region Monitor Keys
        
        void AddKey(string sKey)
        {
            if (m_pMonitorKeys == null)
                return;
            if (!m_pMonitorKeys.Contains(sKey))
            {
                m_pMonitorKeys.Add(sKey);
                EditorPrefs.SetString("OktNetworkMonitorWindow.MonitorKeys", ToArrayString(m_pMonitorKeys.ToArray(), ';'));
            }
        }

        string ToArrayString(string[] sValues, char cSeparator)
        {
            string sFull = "";
            for (int i = 0; i < sValues.Length; i++)
            {
                sFull += sValues[i] + cSeparator;
            }
            return sFull;
        }
        #endregion

    }
}

#endif