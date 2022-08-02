// Made by Markcreator
// https://markcreator.net/

#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class VRCTextureCompressor
{
    private static readonly string name = "VRCTextureCompressor";
    private static readonly string prefix = "[" + name + "] ";

    static VRCTextureCompressor()
    {
        System.Type builderType = GetSDKBuilderType();

        if (builderType != null)
        {
            try
            {
                System.Action<GameObject> originalTest = (System.Action<GameObject>)builderType.GetField("RunExportAndTestAvatarBlueprint").GetValue(null);
                System.Action<GameObject> hookedTest = delegate (GameObject o) { CompressGameobject(o); } + originalTest;
                System.Action<GameObject> originalUpload = (System.Action<GameObject>)builderType.GetField("RunExportAndUploadAvatarBlueprint").GetValue(null);
                System.Action<GameObject> hookedUpload = delegate (GameObject o) { CompressGameobject(o); } + originalUpload;
                builderType.GetField("RunExportAndTestAvatarBlueprint").SetValue(null, hookedTest);
                builderType.GetField("RunExportAndUploadAvatarBlueprint").SetValue(null, hookedUpload);

                Debug.Log(prefix + "Hooked into VRChat Avatar SDK");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        else
        {
            Debug.LogWarning(prefix + "Could not hook into VRChat Avatar SDK (SDK missing?)");
        }
    }

    [MenuItem("Markcreator/VRCTextureCompressor/Compress selected Gameobject", false, 1000)]
    private static void CompressSelectedGameobject()
    {
        CompressGameobject(Selection.activeGameObject);
    }

    private static void CompressGameobject(GameObject o)
    {
        int made = CompressGameobject(o, Pass.Execution, CompressGameobject(o, Pass.Discovery));
        Debug.Log(prefix + "Compressed " + made + " new textures");
    }

    private static int CompressGameobject(GameObject o, Pass pass, int todo = 0)
    {
        int made = 0;
        
        foreach (Renderer r in o.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat)
                {
                    foreach (string textureProperty in mat.GetTexturePropertyNames())
                    {
                        Texture tex;
                        if (tex = mat.GetTexture(textureProperty))
                        {
                            string path = AssetDatabase.GetAssetPath(tex);

                            if (path != null && path.Length > 0)
                            {
                                AssetImporter importer = AssetImporter.GetAtPath(path);

                                if (importer != null && importer is TextureImporter textureImporter)
                                {
                                    bool changed = false;

                                    if (textureImporter.crunchedCompression != true)
                                    {
                                        if (pass == Pass.Execution) textureImporter.crunchedCompression = true;
                                        changed = true;
                                    }
                                    if (textureImporter.textureCompression != TextureImporterCompression.Compressed)
                                    {
                                        if (pass == Pass.Execution) textureImporter.textureCompression = TextureImporterCompression.Compressed;
                                        changed = true;
                                    }
                                    if (textureImporter.compressionQuality != 100)
                                    {
                                        if (pass == Pass.Execution) textureImporter.compressionQuality = 100;
                                        changed = true;
                                    }

                                    if (changed)
                                    {
                                        switch (pass)
                                        {
                                            case Pass.Discovery:
                                                todo++;
                                                break;
                                            case Pass.Execution:
                                                made++;
                                                EditorUtility.DisplayProgressBar(name, "Compressing " + tex.name + " (" + made + "/" + todo + ")", (float)made / todo);

                                                EditorUtility.SetDirty(importer);
                                                importer.SaveAndReimport();
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
        return todo;
    }

    [MenuItem("Markcreator/VRCTextureCompressor/Danger zone/Decompress All Textures", false, 1000)]
    private static void DecompressTextures()
    {
        int made = DecompressTextures(Pass.Execution, DecompressTextures(Pass.Discovery));
        Debug.Log(prefix + "Decompressed " + made + " textures");
    }

    private static int DecompressTextures(Pass pass, int todo = 0)
    {
        int made = 0;

        foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
        {
            if (Path.GetExtension(assetPath).Length > 0 && assetPath.StartsWith("Assets"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);

                if (importer is TextureImporter textureImporter)
                {
                    if (textureImporter.crunchedCompression || textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        switch (pass)
                        {
                            case Pass.Discovery:
                                todo++;
                                break;
                            case Pass.Execution:
                                made++;
                                EditorUtility.DisplayProgressBar(name, "Uncompressing " + assetPath, (float) made / todo);
                                Debug.Log(prefix + "Uncompressed " + assetPath);

                                textureImporter.crunchedCompression = false;
                                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

                                EditorUtility.SetDirty(importer);
                                textureImporter.SaveAndReimport();
                                break;
                        }
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
        return todo;
    }

    private static System.Type GetSDKBuilderType()
    {
        Assembly assembly = null;
        System.Type type = null;
        try
        {
            assembly = Assembly.Load("VRCSDKBase-Editor");
            type = assembly.GetType("VRC.SDKBase.Editor.VRC_SdkBuilder");
        }
        catch (System.Exception) { }

        return type;
    }

    private enum Pass
    {
        Discovery,
        Execution,
    }
}
#endif
