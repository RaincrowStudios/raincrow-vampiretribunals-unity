using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWindow
{
    bool isOpen { get; }
    void Show();
    void Hide();
}
