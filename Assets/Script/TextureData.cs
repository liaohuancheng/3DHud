// TextureData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TextureData", menuName = "Custom/Texture Data")]
public class TextureData : ScriptableObject
{
    [System.Serializable]
    public class TextureInfo
    {
        public string name;
        public Rect uvRect; // x,y 是偏移量，width,height 是缩放
        
        // 方便获取的辅助属性
        public Vector2 Offset => uvRect.position;
        public Vector2 Scale => uvRect.size;
    }

    public Texture2D atlasTexture;
    public TextureInfo[] textureInfos;

    public Rect GetUVRect(string textureName)
    {
        foreach (var info in textureInfos)
        {
            if (info.name == textureName)
                return info.uvRect;
        }
        return new Rect(0, 0, 1, 1);
    }
}
