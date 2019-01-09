using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;

namespace ServerAPI
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : Editor
    {
        private Settings m_Settings;
        private bool[] toggleServerView
        {
            get { return JsonConvert.DeserializeObject<bool[]>(EditorPrefs.GetString("ServerSettings.toggleServerView", "[true]")); }
            set { EditorPrefs.SetString("ServerSettings.toggleServerView", JsonConvert.SerializeObject(value)); }
        }
        private bool toggleFakeView
        {
            get { return EditorPrefs.GetBool("ServerSettings.toggleFakeView", false); }
            set { EditorPrefs.SetBool("ServerSettings.toggleFakeView", value); }
        }

        private void OnEnable()
        {
            m_Settings = target as Settings;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            using (new BoxScope())
            {
                CentralizedLabel("Server settings");
            }

            using (new BoxScope())
            {
                DrawFakeServer();

                for (int i = 0; i < m_Settings.m_Servers.Length; i++)
                {
                    DrawServer(i);
                }

                GUILayout.Space(5);
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add server"))
                    {
                        AddServer();
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);
        }

        private void DrawFakeServer()
        {
            bool selected = m_Settings.m_FakeServer;
            Color color = GUI.backgroundColor;
            if(selected) GUI.backgroundColor = Color.green;

            using (new BoxScope())
            {
                GUI.backgroundColor = color;

                //header
                using (new GUILayout.HorizontalScope())
                {
                    string buttonIcon = toggleFakeView ? "▼" : "▶";
                    if (GUILayout.Button(buttonIcon + " " + "Fake Server", "Label"))
                    {
                        toggleFakeView = !toggleFakeView;
                    }

                    EditorGUI.BeginDisabledGroup(selected);
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                    {
                        m_Settings.m_FakeServer = true;
                    }
                    EditorGUI.EndDisabledGroup();
                }

                //settings
                if (toggleFakeView)
                {
                    float labelWidth = 100;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Response Delay", GUILayout.Width(labelWidth));
                        m_Settings.m_ResponseDelayMin = EditorGUILayout.FloatField(m_Settings.m_ResponseDelayMin);
                        GUILayout.Label("-");
                        m_Settings.m_ResponseDelayMax = EditorGUILayout.FloatField(m_Settings.m_ResponseDelayMax);
                    }
                }
            }
        }
        
        private void DrawServer (int index)
        {
            Settings.ServerSetting server = m_Settings.m_Servers[index];

            bool selected = m_Settings.m_FakeServer == false && index == m_Settings.m_CurrentServerIndex;
            Color color = GUI.backgroundColor;
            if (selected) GUI.backgroundColor = Color.green;

            using (new BoxScope())
            {
                GUI.backgroundColor = color;

                //header
                using (new GUILayout.HorizontalScope())
                {
                    string buttonIcon = toggleServerView[index] ? "▼" : "▶";
                    if (GUILayout.Button(buttonIcon + " " + server.name, "Label"))
                    {
                        bool[] aux = toggleServerView;
                        aux[index] = !aux[index];
                        toggleServerView = aux;
                    }

                    EditorGUI.BeginDisabledGroup(selected);
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                    {
                        m_Settings.m_FakeServer = false;
                        m_Settings.m_CurrentServerIndex = index;
                    }
                    EditorGUI.EndDisabledGroup();


                    if (GUILayout.Button("✕", GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog("Remove from list", "Remove " + server.name + "?", "Ok", "Cancel"))
                        {
                            RemoveServer(server);
                        }
                    }
                }

                //setings
                if (index < toggleServerView.Length && toggleServerView[index])
                {
                    float labelWidth = 100;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Name", GUILayout.Width(labelWidth));
                        server.name = EditorGUILayout.TextField(server.name);
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Address", GUILayout.Width(labelWidth));
                        server.hostAddress = EditorGUILayout.TextField(server.hostAddress);
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("WS Address", GUILayout.Width(labelWidth));
                        server.wsAddress = EditorGUILayout.TextField(server.wsAddress);
                    }
                }
            }
        }

        private void RemoveServer(Settings.ServerSetting server)
        {
            List<Settings.ServerSetting> servers = new List<Settings.ServerSetting>(m_Settings.m_Servers);
            int index = servers.IndexOf(server);
            servers.RemoveAt(index);

            List<bool> toggles = new List<bool>(toggleServerView);
            toggles.RemoveAt(index);

            m_Settings.m_Servers = servers.ToArray();
            toggleServerView = toggles.ToArray();

            EditorUtility.SetDirty(target);
        }

        private void AddServer()
        {
            Settings.ServerSetting server = new Settings.ServerSetting();
            server.name = "Server " + m_Settings.m_Servers.Length;
            server.hostAddress = "http://localhost:8080/";
            server.wsAddress = "http://localhost:8080/ws/";

            List<Settings.ServerSetting> servers = new List<Settings.ServerSetting>(m_Settings.m_Servers);
            servers.Add(server);
            m_Settings.m_Servers = servers.ToArray();

            List<bool> toggles = new List<bool>(toggleServerView);
            toggles.Add(true);
            toggleServerView = toggles.ToArray();

            EditorUtility.SetDirty(target);
        }

        private void CentralizedLabel(string text)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(text);
                GUILayout.FlexibleSpace();
            }
        }
    }
}