Shader "Custom/TextImageAtlas"
{
    Properties
    {
        _MainTex ("Atlas Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Type1 ("Render Type (0=文字 1=图片)", Int) = 0
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "DisableBatching"="False" 
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            int _Type;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(int, _Type1)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                int type = UNITY_ACCESS_INSTANCED_PROP(Props, _Type1);
                
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 文字部分使用Alpha通道，图片部分使用RGB
                if (type == 0) {
                    col.rgb = fixed3(1,1,1); // 文字颜色由顶点色控制
                }
                
                return col;
            }
            ENDCG
        }
    }
}