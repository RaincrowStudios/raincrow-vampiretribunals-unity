using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIScalable : MonoBehaviour
{
    private RectTransform m_Transform;
    private bool m_IsComplete;
    private int m_TweenId;

    public bool isComplete { get { return m_IsComplete; } }
    public new RectTransform transform { get { if(m_Transform == null) m_Transform = this.GetComponent<RectTransform>(); return m_Transform; } }

    private void Awake()
    {
        m_Transform = this.GetComponent<RectTransform>();
        m_IsComplete = true;
    }

    public void Scale(Vector2 scale, float duration, System.Action onComplete, LeanTweenType easeType)
    {
        m_IsComplete = false;
        LeanTween.cancel(m_TweenId);
        m_TweenId = LeanTween.scale(m_Transform, scale, duration)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                m_IsComplete = true;
                if (onComplete != null) onComplete.Invoke();
            })
            .uniqueId;
    }

    public void Scale(Vector2 scale, float duration, System.Action onComplete)
    {
        Scale(scale, duration, onComplete, LeanTweenType.linear);
    }

    public void Scale(Vector2 scale, float duration, LeanTweenType easeType)
    {
        Scale(scale, duration, null, easeType);
    }

    public void Scale(Vector2 scale, float duration)
    {
        Scale(scale, duration, null, LeanTweenType.linear);
    }

    public void Scale(float scale, float duration, System.Action onComplete, LeanTweenType easeType)
    {
        Scale(new Vector2(scale, scale), duration, onComplete, easeType);
    }

    public void Scale(float scale, float duration, System.Action onComplete)
    {
        Scale(new Vector2(scale, scale), duration, onComplete, LeanTweenType.linear);
    }

    public void Scale(float scale, float duration, LeanTweenType easeType)
    {
        Scale(new Vector2(scale, scale), duration, null, easeType);
    }

    public void Scale(float scale, float duration)
    {
        Scale(new Vector2(scale, scale), duration, null, LeanTweenType.linear);
    }
}
