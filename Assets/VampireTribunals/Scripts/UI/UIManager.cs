using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Manager to control how many instances of UI should be available and
    which ones should be prioritized.

    Using this method to instantiate the UIs on demand should help with load times
    and also keep the scenes clean.
*/
public class UIManager : MonoBehaviour
{
    private static UIManager m_Instance;
    private static UIManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new GameObject("UIManager").AddComponent<UIManager>();
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }


    private Dictionary<System.Type, UIWindow> m_AvailableUIs = new Dictionary<System.Type, UIWindow>();
    public static int sortOrder = 0;

    public static T InstantiateUI<T>(string path) where T : UIWindow
    {
        T prefab = Resources.Load<UIWindow>(path).GetComponent<T>();
        T instance = Instantiate<T>(prefab);
        Instance.OnInstantiateWindow(instance);
        return instance;
    }

    private void OnInstantiateWindow(UIWindow window)
    {
        m_AvailableUIs.Add(window.GetType(), window);
    }
}
