using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIWindow : MonoBehaviour
{
    [Header("UIWindow")]
    [SerializeField] protected Canvas m_Canvas;
    [SerializeField] protected UIFadeable m_FadeComponent;
    [SerializeField] protected UIScalable m_ScaleComponent;

    private bool m_IsOpen;
    public bool isOpen { get { return m_IsOpen; } }
    public Canvas canvas { get { return m_Canvas; } }


    protected virtual void Awake()
    {
        m_FadeComponent.canvasGroup.alpha = 0;
        m_FadeComponent.gameObject.SetActive(false);
        m_ScaleComponent.transform.localScale = Vector3.zero;
    }

    private void OnValidate()
    {
        if (m_Canvas == null)  m_Canvas = GetComponentInChildren<Canvas>();
        if (m_FadeComponent == null) m_FadeComponent = GetComponentInChildren<UIFadeable>();
        if (m_ScaleComponent == null) m_ScaleComponent = GetComponentInChildren<UIScalable>();
    }


    /// <summary>
    /// Setup the sorting order and animate the window.
    /// </summary>
    public void Show()
    {
        if (m_IsOpen) return;
        m_IsOpen = true;

        System.Action onHide = () =>
        {
            canvas.sortingOrder = ++UIManager.sortOrder;

            OnShowStart();
            ShowAnimation(OnShowComplete);
        };

        //if a hide animation is running, hide first, then show
        if(m_FadeComponent.gameObject.activeSelf)
        {
            HideAnimation(onHide);
        }
        else
        {
            onHide.Invoke();
        }
    }

    protected virtual void ShowAnimation(System.Action onComplete)
    {
        m_FadeComponent.gameObject.SetActive(true);
        m_FadeComponent.Alpha(1f, .75f);
        m_ScaleComponent.Scale(1f, 0.25f, onComplete);
    }

    /// <summary>
    /// Invoked right before Show() is called. Use this for initialization
    /// </summary>
    protected virtual void OnShowStart() { }

    /// <summary>
    /// Invoke when the Show animation is finished.
    /// </summary>
    protected virtual void OnShowComplete()
    {
    }


    /// <summary>
    /// Animate the window and reset the sorting order.
    /// </summary>
    public void Hide()
    {
        if (m_IsOpen == false) return;
        m_IsOpen = false;

        UIManager.sortOrder--;

        OnHideStart();
        HideAnimation(OnHideComplete);
    }

    protected virtual void HideAnimation(System.Action onComplete)
    {
        float duration = m_FadeComponent.canvasGroup.alpha;
        m_FadeComponent.Alpha(0f, 0.25f * duration);
        m_ScaleComponent.Scale(0f, 0.4f * duration, OnHideComplete);
    }

    /// <summary>
    /// Invoked as soon as Hide() is called.
    /// </summary>
    protected virtual void OnHideStart()
    {

    }

    /// <summary>
    /// Invoke when the Hide animation is finished.
    /// </summary>
    protected virtual void OnHideComplete()
    {
        m_FadeComponent.gameObject.SetActive(false);
    }
}
