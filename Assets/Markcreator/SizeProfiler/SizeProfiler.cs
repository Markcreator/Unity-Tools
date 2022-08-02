// Made by Markcreator
// https://markcreator.net/

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using System.Linq;

namespace Markcreator.SizeProfiler
{
    public class SizeProfiler : EditorWindow
    {
        public static readonly string version = "1.1";
        private Vector2 scrollPosition = Vector2.zero;

        public static Object target;
        public static List<Asset> assets = new List<Asset>();
        public static Directory dir;
        public static ListMode listMode = ListMode.Directory;

        [MenuItem("GameObject/Size Profiler", false, 0), MenuItem("Markcreator/Size Profiler/Open Menu", false, 1000)]
        public static void OpenMenu()
        {
            SizeProfiler window = (SizeProfiler)GetWindow(typeof(SizeProfiler));
            window.name = "Size Profiler";
            window.titleContent.text = window.name;
            
            window.Start();
        }

        void Start()
        {
            if (Selection.activeObject != null)
            {
                target = Selection.activeObject;
                LoadSize(target);
            }
        }

        void OnEnable()
        {
            Start();
        }

        public void OnGUI()
        {
            // Input group
            GUI.BeginGroup(new Rect(0, 0, position.width, position.height - 4));
            GUILayoutOption[] inputOptions = { GUILayout.Width(position.width - 4), GUILayout.Height(position.height - 8) };
            GUIStyle inputStyle = new GUIStyle();
            inputStyle.padding = new RectOffset(10, 10, 0, 0);
            Rect inputGroup = EditorGUILayout.BeginVertical(inputStyle, inputOptions); // Input box

            GUIStyle h = new GUIStyle(GUI.skin.horizontalScrollbar);
            h.fixedHeight = h.fixedWidth = 0;
            GUIStyle v = new GUIStyle(GUI.skin.verticalScrollbar);
            v.fixedHeight = v.fixedWidth = 0;
            Rect scrollRect = new Rect(0, 0, inputGroup.width, inputGroup.height);
            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, inputGroup.width, position.height), scrollPosition, scrollRect, false, false, h, v);

            GUILayout.Space(10);

            GUIStyle boxPadded = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10), richText = true };
            GUIStyle textPadded = new GUIStyle(GUI.skin.label) { padding = new RectOffset(5, 5, 5, 5), richText = true };
            GUILayout.BeginHorizontal(boxPadded); // Credits box

            GUILayout.BeginVertical();
            GUILayout.Label("Size Profiler v" + version, EditorStyles.boldLabel);
            GUIStyle madeStyle = new GUIStyle(GUI.skin.label) { richText = true };
            madeStyle.normal.textColor = Color.gray;
            if (GUILayout.Button("Made by <color=#81B4FF>Markcreator</color>", madeStyle)) Application.OpenURL("https://markcreator.net/");
            GUILayout.EndVertical();

            GUIStyle bgColor = new GUIStyle();
            Texture2D background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
            background.Apply();
            bgColor.normal.background = background;

            GUILayout.EndHorizontal(); // End Credits box
            GUILayout.Space(5);
            GUILayout.BeginVertical(boxPadded); // Gameobject box

            Object oldTarget = target;
            Object newTarget = EditorGUILayout.ObjectField(new GUIContent("Target", "Target Object"), target, typeof(Object), true);

            if (oldTarget != newTarget && newTarget != null)
            {
                target = newTarget;
                LoadSize(target);
            }

            GUILayout.Space(5);
            listMode = (ListMode)EditorGUILayout.EnumPopup("List Mode", listMode);
            GUILayout.Space(5);
            if (GUILayout.Button("Refresh")) LoadSize(target);

            GUILayout.EndVertical(); // End Object box

            GUILayout.Space(10);

            if (dir != null) {
                GUILayout.Label("Total size: " + EditorUtility.FormatBytes(dir.size));
                GUILayout.Space(10);

                switch (listMode)
                {
                    case ListMode.Directory:
                        dir.PrintEditorRecursive(scrollRect);
                        break;
                    case ListMode.Flat:
                        foreach (Asset asset in assets.OrderBy(i => i.size).Reverse())
                        {
                            asset.PrintEditor(Rect.zero);
                        }
                        break;
                }
            }

            GUILayout.Space(20);
            GUI.EndScrollView();
            EditorGUILayout.EndVertical(); // End Input box
            GUI.EndGroup();
        }

        public void LoadSize(Object target)
        {
            assets.Clear();

            // Texture util
            System.Type textureUtil = System.Reflection.Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
            System.Reflection.MethodInfo getStorageMemorySizeLong = textureUtil.GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (target != null)
            {
                GameObject o = null;
                if (target is GameObject)
                {
                    o = Instantiate((GameObject)target);
                    o.SetActive(false);
                    o.hideFlags = HideFlags.HideAndDontSave;
                }

                foreach (Object dependency in EditorUtility.CollectDependencies(new Object[] { (o ? o : target) }))
                {
                    if (dependency != null)
                    {
                        string path = AssetDatabase.GetAssetPath(dependency);
                        string suffix = "";

                        long dependencySize = 0;
                        if (dependency is Texture)
                        {
                            dependencySize = (long)getStorageMemorySizeLong.Invoke(null, new object[] { dependency });
                        }
                        else if (dependency is Mesh)
                        {
                            //dependencySize = (long)getStorageMemorySizeLong.Invoke(null, new object[] { dependency });
                            Mesh m = ((Mesh)dependency);
                            //dependencySize = Profiler.GetRuntimeMemorySizeLong(m);

                            dependencySize += 12; // Vertex
                            dependencySize += 8; // UVs
                            dependencySize += 4; // Vertex color
                            dependencySize += 12; // Normals
                            dependencySize += 16; // Tangents
                            dependencySize *= m.vertexCount;

                            uint indexCount = 0;
                            for (int i = 0; i < m.subMeshCount; i++) indexCount += 2 * m.GetIndexCount(i); // Indicies
                            dependencySize += indexCount;

                            suffix = "<color=green>(Estimated)</color>";
                        }
                        else
                        {
                            dependencySize = Profiler.GetRuntimeMemorySizeLong(dependency);
                        }

                        if (path.Length > 0)
                        {
                            Asset a = new Asset(path, dependencySize, dependency);
                            a.suffix = suffix;
                            assets.Add(a);
                        }
                    }
                }

                if (o) DestroyImmediate(o);
            }

            dir = BuildDirectory();
        }

        public Directory BuildDirectory()
        {
            Directory assetDir = new Directory();
            foreach (Asset asset in assets)
            {
                string[] subdirs = asset.path.Split('/');
                Directory subdir = assetDir;
                for (int i = 0; i < subdirs.Length; i++) {
                    string dir = subdirs[i];
                    subdir = subdir.AddDirectory(dir);
                }
                subdir.AddAsset(subdirs.Last(), asset);
            }

            return assetDir;
        }
    }

    [System.Serializable]
    public class Directory
    {
        public string path;
        public long size;
        private Directory parent;
        private Dictionary<string, Directory> subdirs = new Dictionary<string, Directory>();

        private bool foldout = false;

        public Directory(Directory parent = null, string path = "", long size = 0, Dictionary<string, Directory> subdirs = null)
        {
            this.parent = parent;
            this.path = path;
            this.size = size;
            if (subdirs != null) this.subdirs = subdirs;
        }

        public Directory AddDirectory(string dir)
        {
            if (!subdirs.ContainsKey(dir))
            {
                subdirs.Add(dir, new Directory(this, dir, 0));
            }
            return subdirs[dir];
        }

        public void AddAsset(string dir, Asset a)
        {
            string totalDir = dir + a.o.GetType();
            if (!subdirs.ContainsKey(totalDir))
            {
                subdirs.Add(totalDir, a);
                AddSizeRecursive(a.size);
            }
        }

        public void AddSizeRecursive(long size)
        {
            this.size += size;
            if (parent != null) parent.AddSizeRecursive(size);
        }

        public void PrintEditorRecursive(Rect rect)
        {
            if (subdirs != null) {
                foreach (Directory dir in subdirs.Values.OrderBy(i => i.size).Reverse())
                {
                    if (dir != null) {
                        if (dir is Asset)
                        {
                            ((Asset)dir).PrintEditor(rect);
                        }
                        else if (dir.foldout = EditorGUILayout.Foldout(dir.foldout, dir.path + " (" + EditorUtility.FormatBytes(dir.size) + ")", true))
                        {
                            EditorGUI.indentLevel++;
                            dir.PrintEditorRecursive(rect);
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            string composite = JsonUtility.ToJson(this);
            foreach (Directory sub in subdirs.Values) composite += sub.ToString();
            return composite;
        }
    }

    public class Asset : Directory
    {
        public Object o;
        public string suffix;

        public Asset(string path, long size, Object o) : base(null, path, size, null)
        {
            this.o = o;
        }

        public void PrintEditor(Rect rect)
        {
            Rect indent = EditorGUI.IndentedRect(rect);
            if (GUILayout.Button("<color=cyan>" + o.name + "</color> <color=grey>(" + o.GetType() + ")</color> " + EditorUtility.FormatBytes(size) + " " + suffix, new GUIStyle(GUI.skin.label) { richText = true, margin = new RectOffset((int) indent.xMin + 10, 0, 0, 0) })) EditorGUIUtility.PingObject(o);
        }
    }

    public enum ListMode
    {
        Directory,
        Flat
    }
}
#endif
