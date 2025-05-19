// TextureAtlasCreator.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class TextureAtlasCreator : EditorWindow
{
    private List<Texture2D> texturesToMerge = new List<Texture2D>();
    private Texture2D atlasTexture;
    private string atlasName = "NewAtlas";
    private int padding = 2;
    private Vector2Int atlasSize = new Vector2Int(1024, 1024);
    private bool forceSquare = false;
    private TextureData textureData;

    [MenuItem("Tools/Texture Atlas Creator")]
    public static void ShowWindow()
    {
        GetWindow<TextureAtlasCreator>("Texture Atlas Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Texture Atlas Settings", EditorStyles.boldLabel);
        atlasName = EditorGUILayout.TextField("Atlas Name", atlasName);
        padding = EditorGUILayout.IntField("Padding", padding);
        forceSquare = EditorGUILayout.Toggle("Force Square", forceSquare);
        atlasSize = EditorGUILayout.Vector2IntField("Atlas Size", atlasSize);

        GUILayout.Space(10);
        GUILayout.Label("Textures to Merge", EditorStyles.boldLabel);

        // 添加/删除纹理列表
        for (int i = 0; i < texturesToMerge.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            texturesToMerge[i] = (Texture2D)EditorGUILayout.ObjectField(texturesToMerge[i], typeof(Texture2D), false);
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                texturesToMerge.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Texture"))
        {
            texturesToMerge.Add(null);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Create Atlas", GUILayout.Height(30)))
        {
            CreateAtlas();
        }

        if (atlasTexture != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Preview", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(atlasTexture, typeof(Texture2D), false);
        }
    }

    void CreateAtlas()
    {
        // 验证输入
        if (texturesToMerge == null || texturesToMerge.Count == 0)
        {
            Debug.LogError("No textures to merge!");
            return;
        }

        // 过滤掉空纹理
        var validTextures = texturesToMerge.Where(x => x != null).ToList();
        if (validTextures.Count == 0)
        {
            Debug.LogError("No valid textures to merge!");
            return;
        }

        // 确保所有纹理可读
        foreach (var tex in validTextures)
        {
            MakeTextureReadable(tex);
        }

        // 创建临时可读纹理副本（避免修改原始纹理）
        List<Texture2D> readableTextures = new List<Texture2D>();
        foreach (var tex in validTextures)
        {
            readableTextures.Add(CreateReadableCopy(tex));
        }

        // 创建图集
        Texture2D newAtlas = new Texture2D(atlasSize.x, atlasSize.y, TextureFormat.RGBA32, false, false);
        Rect[] rects = newAtlas.PackTextures(readableTextures.ToArray(), padding, atlasSize.x, forceSquare);

        // 创建TextureData
        textureData = ScriptableObject.CreateInstance<TextureData>();
        textureData.atlasTexture = newAtlas;
        textureData.textureInfos = new TextureData.TextureInfo[validTextures.Count];

        for (int i = 0; i < validTextures.Count; i++)
        {
            textureData.textureInfos[i] = new TextureData.TextureInfo
            {
                name = validTextures[i].name,
                uvRect = rects[i]
            };
        }

        // 保存资产
        string folderPath = "Assets/TextureAtlases";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string atlasPath = $"{folderPath}/{atlasName}.png";
        newAtlas.Apply();
        byte[] bytes = newAtlas.EncodeToPNG();
        File.WriteAllBytes(atlasPath, bytes);

        string dataPath = $"{folderPath}/{atlasName}_Data.asset";
        AssetDatabase.CreateAsset(textureData, dataPath);

        // 刷新并重新导入
        AssetDatabase.Refresh();

        // 设置图集导入设置
        TextureImporter atlasImporter = (TextureImporter)TextureImporter.GetAtPath(atlasPath);
        if (atlasImporter != null)
        {
            atlasImporter.textureType = TextureImporterType.Default;
            atlasImporter.isReadable = true; // 保持图集可读以便后续使用
            atlasImporter.mipmapEnabled = false;
            atlasImporter.SaveAndReimport();
        }

        // 更新预览
        atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

        Debug.Log($"Atlas created at {atlasPath}");
        Debug.Log($"TextureData created at {dataPath}");
    
    }

    private void MakeTextureReadable(Texture2D texture)
    {
        if (texture == null) return;

        string path = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(path)) return;

        TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
        if (ti == null) return;

        if (!ti.isReadable)
        {
            ti.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }
    }

    private Texture2D CreateReadableCopy(Texture2D source)
    {
        // 如果纹理已经是可读的，直接返回
        if (source.isReadable) return source;

        // 创建临时RenderTexture
        RenderTexture tmp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        // 将原始纹理复制到RenderTexture
        Graphics.Blit(source, tmp);

        // 激活RenderTexture
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        // 创建新的可读纹理
        Texture2D readableTexture = new Texture2D(source.width, source.height);
        readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTexture.Apply();

        // 恢复设置
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return readableTexture;
    }
}