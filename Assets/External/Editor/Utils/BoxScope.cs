using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoxScope : System.IDisposable
{
    readonly bool indent;
    static GUIStyle boxScopeStyle;
    public static GUIStyle BoxScopeStyle
    {
        get
        {
            if (boxScopeStyle == null)
            {
                boxScopeStyle = new GUIStyle(EditorStyles.helpBox);
                RectOffset p = boxScopeStyle.padding;
                p.right += 6;
                p.top += 1;
                p.left += 3;
                p.bottom += 2;
            }

            return boxScopeStyle;
        }
    }

    public BoxScope(bool indent = false)
    {
        this.indent = indent;
        EditorGUILayout.BeginVertical(BoxScopeStyle);

        if (indent)
            EditorGUI.indentLevel++;
    }

    public void Dispose()
    {
        if (indent)
            EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
    }
}