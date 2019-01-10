using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILogin : UIWindow
{
    private static UILogin m_Instance;
    public static UILogin Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = UIManager.InstantiateUI<UILogin>("UI/LoginCanvas");
            }
            return m_Instance;
        }
    }
    
    [Header("Properties")]
    [SerializeField] private TMP_InputField m_UsernameField;
    [SerializeField] private TMP_InputField m_PasswordField;
    [SerializeField] private TextMeshProUGUI m_pErrorText;
    [SerializeField] private Button m_LoginButton;
    [SerializeField] private Button m_CloseButton;
    
    protected override void Awake()
    {
        base.Awake();

        if (m_Instance == null)
            m_Instance = this;

        m_LoginButton.onClick.AddListener(OnClickLogin);
        m_CloseButton.onClick.AddListener(OnClickClose);
    }

    protected override void OnShowStart()
    {
        base.OnShowStart();

        m_UsernameField.text = "";
        m_PasswordField.text = "";
        m_pErrorText.text = "";
    }

    private void OnClickLogin()
    {
        if (string.IsNullOrEmpty(m_UsernameField.text))
        {
            m_pErrorText.text = "Error: Empty username";
            return;
        }

        if (string.IsNullOrEmpty(m_PasswordField.text))
        {
            m_pErrorText.text = "Error: Empty password";
            return;
        }

        LoginManager.SendLogin(m_UsernameField.text, Md5(m_PasswordField.text), LoginCallback);
    }

    private void LoginCallback(LoginManager.LoginResponse response, int result)
    {
        if(result == 200)
        {
            m_pErrorText.text = "Login Succesful";
        }
        else
        {
            m_pErrorText.text = "Error: " + result;
        }
    }

    private void OnClickClose()
    {
        Hide();
    }
    

    public string Md5(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}
