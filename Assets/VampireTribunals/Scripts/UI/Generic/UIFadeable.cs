using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup)), DisallowMultipleComponent]
public class UIFadeable : MonoBehaviour
{
    private CanvasGroup m_CanvasGroup;
    private bool m_IsComplete;
    private int m_TweenId;

    public bool isComplete { get { return m_IsComplete; } }
    public CanvasGroup canvasGroup { get { return m_CanvasGroup; } }

    private void Awake()
    {
        m_CanvasGroup = this.GetComponent<CanvasGroup>();
        m_IsComplete = true;
    }

    public void Alpha(float alpha, float duration, System.Action onComplete, LeanTweenType easeType)
    {
        m_IsComplete = false;
        LeanTween.cancel(m_TweenId);
        m_TweenId = LeanTween.value(m_CanvasGroup.alpha, alpha, duration)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                m_IsComplete = true;
                if (onComplete != null) onComplete.Invoke();
            })
            .uniqueId;
    }

    public void Alpha(float alpha, float duration, System.Action onComplete)
    {
        Alpha(alpha, duration, onComplete, LeanTweenType.easeOutCubic);
    }

    public void Alpha(float alpha, float duration, LeanTweenType easeType)
    {
        Alpha(alpha, duration, null, easeType);
    }

    public void Alpha(float alpha, float duration)
    {
        Alpha(alpha, duration, null, LeanTweenType.easeOutCubic);
    }
}
