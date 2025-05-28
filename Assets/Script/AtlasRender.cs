// AtlasTextureRenderer.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
public class AtlasRender : MonoBehaviour
{
    public TextureData atlasData;
    public string textureName;

    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer meshRenderer;
    private static readonly int MainTexSt1 = Shader.PropertyToID("_Atlas_ST1");
    public Material material;
    private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();
    private static readonly int Type1 = Shader.PropertyToID("_Type1");

    void Start()
    {
        ApplyTexture(textureName);
    }

    public void ApplyTexture(string newTextureName)
    {
        if (atlasData == null)
        {
            Debug.LogError("Atlas data not assigned!");
            return;
        }

        textureName = newTextureName;
        Rect uvRect = atlasData.GetUVRect(textureName);

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
        
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = CreateMaterialVariant(newTextureName);
    }
    
    private Material CreateMaterialVariant(string newTextureName)
    {
        if(_materialCache.ContainsKey(textureName))
            return _materialCache[newTextureName];
            
        Material newMat = new Material(material);
        newMat.name = meshRenderer.name;
        Rect uvRect = atlasData.GetUVRect(newTextureName);
        newMat.SetVector(MainTexSt1, new Vector4(uvRect.width, uvRect.height, uvRect.x, uvRect.y));
        newMat.SetFloat(Type1, 0);
        newMat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        newMat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        newMat.SetFloat("_BlendOp", (float)BlendOp.Add);
        _materialCache[newTextureName] = newMat;
        return newMat;
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        if (atlasData != null && !string.IsNullOrEmpty(textureName))
        {
            ApplyTexture(textureName);
        }
    }
    #endif
}