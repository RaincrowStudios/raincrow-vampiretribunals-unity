using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : ScriptableObject
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/ServerAPI/Create Settings")]
    public static void CreateObject()
    {
        Settings asset = ScriptableObject.CreateInstance<Settings>();

        UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/Settings.asset");
        UnityEditor.AssetDatabase.SaveAssets();

        UnityEditor.EditorUtility.FocusProjectWindow();

        UnityEditor.Selection.activeObject = asset;
    }
#endif

    [System.Serializable]
    public class ServerSetting
    {
        public string name;
        public string hostAddress;
        public string wsAddress;
    }

    //main settings
    public ServerSetting[] m_Servers = new ServerSetting[] 
    {
        new ServerSetting()
        {
            name = "Local",
            hostAddress = "http://localhost:8080/api/",
            wsAddress = "http://localhost:8080/ws/"
        }
    };
    public int m_CurrentServer = 0;

    //fake server settings
    public bool m_FakeServer = false;
    public float m_ResponseDelayMin = 0.5f;
    public float m_ResponseDelayMax = 1f;


    public ServerSetting currentServer { get { return m_Servers[m_CurrentServer]; } }
    public string hostAddres { get { return m_FakeServer ? "LocalAPI/" : currentServer.hostAddress; } }
    public string wsAddres { get { return m_FakeServer ? "LocalAPI/" : currentServer.wsAddress; } }
}
