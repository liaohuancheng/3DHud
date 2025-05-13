Shader "Custom/AvatarShader"
{
    Properties
    {
        _MainTex ("Avatar Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // 用于硬边缘透明
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // 标准透明混合
        ZWrite Off // 关闭深度写入（避免透明物体遮挡问题）

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _AlphaCutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - _AlphaCutoff); // 根据Alpha值裁剪像素
                return col;
            }
            ENDCG
        }
    }
}