Shader "Custom/TextImageAtlas"
{
    Properties
    {
        _MainTex ("Atlas Texture", 2D) = "white" {}
        _MainTex_ST ("Texture ST", Vector) = (1,1,0,0)
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Type1 ("Render Type (0=文字 1=图片)", Float) = 0
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
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
            #pragma enable_d3d11_debug_symbols
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
CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Type1;
CBUFFER_END
            // UNITY_INSTANCING_BUFFER_START(Props)
            // UNITY_DEFINE_INSTANCED_PROP(float, _Type1)
            // UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST1)
            // UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
    float4 st = _MainTex_ST;
                // float4 st = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST1);
                o.uv = v.uv * st.xy + st.zw; // 应用实例化的ST变换
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                // int type = UNITY_ACCESS_INSTANCED_PROP(Props, _Type1);
                
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 文字部分使用Alpha通道，图片部分使用RGB
                if (_Type1 == 0) {
                    col.rgb = fixed3(1,1,1); // 文字颜色由顶点色控制
                }
                
                return col;
            }
            ENDCG
        }
    }
}