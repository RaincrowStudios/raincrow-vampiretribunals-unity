using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ServerAPI
{
    public class TestServerAPI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_Endpoint;
        [SerializeField] private TMP_InputField m_Data;
        [SerializeField] private TMP_InputField m_Method;
        [SerializeField] private TMP_Text m_Debug;
        [SerializeField] private Button m_SendButton;

        private void Awake()
        {
            m_SendButton.onClick.AddListener(OnClickSend);
        }

        private void OnClickSend()
        {
            m_Debug.text = "sending: " + API.settings.hostAddres + m_Endpoint.text;
            API.Request(m_Endpoint.text, m_Data.text, m_Method.text, (string response, int result) =>
            {
                string debug = "result: " + result + ". response:\n" + response;
                m_Debug.text = debug;
            });
        }
    }
}