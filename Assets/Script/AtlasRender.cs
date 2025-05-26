// AtlasTextureRenderer.cs
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class AtlasRender : MonoBehaviour
{
    public TextureData atlasData;
    public string textureName;

    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer meshRenderer;
    private static readonly int MainTexSt1 = Shader.PropertyToID("_Atlas_ST1");

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

        meshRenderer.GetPropertyBlock(propertyBlock);
        // propertyBlock.SetTexture("_MainTex", atlasData.atlasTexture);
        // propertyBlock.SetVector("_MainTex_ST", new Vector4(uvRect.width, uvRect.height, uvRect.x, uvRect.y));
        propertyBlock.SetVector(MainTexSt1, new Vector4(uvRect.width, uvRect.height, uvRect.x, uvRect.y));
        meshRenderer.SetPropertyBlock(propertyBlock);
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