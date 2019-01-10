using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    //test
    [SerializeField] private UnityEngine.UI.Button m_Login;

    private void Awake()
    {
        m_Login.onClick.AddListener(() => UILogin.Instance.Show());
    }
}
