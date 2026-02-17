Shader "Custom/TriplanarUnlit"
{
    Properties
    {
        _MainTex ("Texture (Tileable)", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 3.0
        _Blend ("Blend Sharpness", Range(1, 16)) = 4.0

        _Color ("Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Tiling;
            float _Blend;
            fixed4 _Color;
            float _Brightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.worldNormal);
                float3 an = abs(n);

                // Blend weights
                an = pow(an, _Blend);
                an /= (an.x + an.y + an.z + 1e-5);

                // World-aligned UVs
                float2 uvX = i.worldPos.zy * _Tiling;
                float2 uvY = i.worldPos.xz * _Tiling;
                float2 uvZ = i.worldPos.xy * _Tiling;

                fixed4 xCol = tex2D(_MainTex, uvX);
                fixed4 yCol = tex2D(_MainTex, uvY);
                fixed4 zCol = tex2D(_MainTex, uvZ);

                fixed4 col = xCol * an.x + yCol * an.y + zCol * an.z;

                // Tint + brightness control
                col.rgb *= _Color.rgb;
                col.rgb *= _Brightness;
                col.a *= _Color.a;

                return col;
            }
            ENDCG
        }
    }
}
