#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TransformCopy : EditorWindow
{
    private const string NAME = "TransformCopy";
    private const string VERSION = "1.1";
    private const string PRODUCT_URL = "https://github.com/Markcreator/Unity-Tools";
    private Vector2 scrollPosition = Vector2.zero;
    private GameObject from;
    private GameObject to;

    private static GUIStyle boxPadded;


    [MenuItem("Markcreator/" + NAME, false, 100)]
    public static void ShowWindow()
    {
        TransformCopy window = (TransformCopy)GetWindow(typeof(TransformCopy));
        window.name = NAME;
        window.titleContent.text = window.name;
    }

    void OnGUI()
    {
        PrintHeader();

        GUILayout.BeginVertical(boxPadded); // Gameobject box

        EditorGUI.BeginChangeCheck();
        GameObject newFrom = (GameObject)EditorGUILayout.ObjectField(new GUIContent("From", "GameObject to copy all child transforms from"), from, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
            from = newFrom;

        EditorGUI.BeginChangeCheck();
        GameObject newTo = (GameObject)EditorGUILayout.ObjectField(new GUIContent("To", "GameObject to copy all child transforms to"), to, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
            to = newTo;

        GUILayout.EndVertical(); // End Object box

        GUILayout.Space(10);

        if (from && to)
        {
            if (GUILayout.Button("Apply"))
            {
                Dictionary<string, TransformData> transforms = new Dictionary<string, TransformData>();
                int applied = 0;

                foreach (Transform t in from.GetComponentsInChildren<Transform>(true))
                {
                    string path = t.GetPath(from.transform);

                    if (transforms.ContainsKey(path))
                    {
                        Debug.LogError($"GameObject has duplicate transform names for '{path}'. Ignoring.");
                        continue;
                    }

                    transforms.Add(path, new TransformData(t));
                }

                int group = Undo.GetCurrentGroup();
                foreach (Transform t in to.GetComponentsInChildren<Transform>(true))
                {
                    string path = t.GetPath(to.transform);
                    if (!transforms.ContainsKey(path))
                        continue;

                    var transform = transforms[path];
                    Undo.RecordObject(t, "Applied Transform");
                    transform.ApplyTo(t);
                    EditorUtility.SetDirty(t);
                    applied++;
                }
                Undo.CollapseUndoOperations(group);

                EditorUtility.DisplayDialog(NAME, $"Found {transforms.Count} transforms in '{from.name}' to potentially copy. Copied {applied} to '{to.name}'.", "OK");
            }
        }

        PrintFooter();
    }

    private void PrintHeader()
    {
        if (boxPadded == null)
            boxPadded = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10), richText = true };

        // Input group
        GUI.BeginGroup(new Rect(0, 0, position.width, position.height - 4));
        GUILayoutOption[] inputOptions = { GUILayout.Width(position.width - 4), GUILayout.Height(position.height - 8) };
        GUIStyle inputStyle = new GUIStyle();
        inputStyle.padding = new RectOffset(10, 10, 0, 0);
        Rect inputGroup = EditorGUILayout.BeginVertical(inputStyle, inputOptions); // Input box

        GUIStyle h = new GUIStyle(GUI.skin.horizontalScrollbar);
        GUIStyle v = new GUIStyle(GUI.skin.verticalScrollbar);
        v.fixedHeight = v.fixedWidth = h.fixedHeight = h.fixedWidth = 0;
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, inputGroup.width, position.height), scrollPosition, new Rect(0, 0, inputGroup.width, inputGroup.height), false, false, h, v);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(boxPadded); // Credits box

        GUILayout.BeginVertical();
        GUILayout.Label($"{NAME} v{VERSION}", EditorStyles.boldLabel);
        GUIStyle madeStyle = new GUIStyle(GUI.skin.label) { richText = true };
        madeStyle.normal.textColor = Color.gray;
        if (GUILayout.Button("Made by <color=#81B4FF>Markcreator</color>", madeStyle))
            Application.OpenURL("https://markcreator.net/");
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(100));
        if (GUILayout.Button("Website"))
            Application.OpenURL(PRODUCT_URL);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal(); // End Credits box
        GUILayout.Space(5);
    }

    private void PrintFooter()
    {
        GUILayout.Space(100); // Extra scrolling space
        GUI.EndScrollView();
        EditorGUILayout.EndVertical(); // End Input box
        GUI.EndGroup();
    }
}

internal static class TransformExtension
{
    internal static string GetPath(this Transform current, Transform relative = null)
    {
        if (current.parent == null || (relative && current.parent.name.Equals(relative.name)))
            return current.name;
        return current.parent.GetPath(relative) + "/" + current.name;
    }
}

[Serializable]
internal class TransformData
{
    public Vector3 LocalPosition = Vector3.zero;
    public Vector3 LocalEulerRotation = Vector3.zero;
    public Vector3 LocalScale = Vector3.one;
    public TransformData() { }

    public TransformData(Transform transform)
    {
        LocalPosition = transform.localPosition;
        LocalEulerRotation = transform.localEulerAngles;
        LocalScale = transform.localScale;
    }

    public void ApplyTo(Transform transform)
    {
        transform.localPosition = LocalPosition;
        transform.localEulerAngles = LocalEulerRotation;
        transform.localScale = LocalScale;
    }
}
#endif
