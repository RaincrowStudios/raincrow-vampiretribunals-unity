using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILogin : MonoBehaviour, IWindow
{
    public static UILogin Instance { get; private set; }

    [Header("Animation")]
    [SerializeField] private UIFadeable m_FadeComponent;
    [SerializeField] private UIScalable m_ScaleComponent;

    [Header("Properties")]
    [SerializeField] private TMP_InputField m_UsernameField;
    [SerializeField] private TMP_InputField m_PasswordField;
    [SerializeField] private TextMeshProUGUI m_pErrorText;
    [SerializeField] private Button m_LoginButton;

    public bool isOpen { get { return m_FadeComponent.gameObject.activeSelf; } }

    private void Awake()
    {
        Instance = this;
        m_LoginButton.onClick.AddListener(OnClickLogin);

        m_FadeComponent.canvasGroup.alpha = 0;
        m_FadeComponent.canvasGroup.interactable = false;
        m_FadeComponent.gameObject.SetActive(false);
        m_ScaleComponent.transform.localScale = Vector3.zero;
        OnHideComplete();
    }

#if UNITY_EDITOR
    private void Update()
    {
        //if (Input.GetKeyDown(A))
    }
#endif

    private void OnClickLogin()
    {

    }

    public void Show()
    {
        m_FadeComponent.gameObject.SetActive(true);
        m_FadeComponent.Alpha(1f, 0.5f, OnShowComplete);
        m_ScaleComponent.Scale(1f, 0.25f);
    }

    private void OnShowComplete()
    {
        m_FadeComponent.canvasGroup.interactable = true;
    }

    public void Hide()
    {
        m_FadeComponent.Alpha(0f, 0.25f, OnShowComplete);
        m_ScaleComponent.Scale(1f, 0.25f);
    }

    private void OnHideComplete()
    {
        m_FadeComponent.gameObject.SetActive(false);
    }
}
