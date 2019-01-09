using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ServerAPI
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : Editor
    {
        private Settings settings;

        private void OnEnable()
        {
            settings = target as Settings;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}