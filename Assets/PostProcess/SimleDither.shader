Shader "Dithered/SimpleDither"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,1)) = 1
        [IntRange]_ColorLevels ("Color Levels", Range(2,16)) = 6
        _DitherStrength ("Dither Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Intensity;
            float _ColorLevels;
            float _DitherStrength;

            static const float bayer[16] =
            {
                0, 8, 2, 10,
                12, 4, 14, 6,
                3, 11, 1, 9,
                15, 7, 13, 5
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                int x = (int)fmod(i.pos.x, 4.0);
                int y = (int)fmod(i.pos.y, 4.0);
                float threshold = bayer[x + y * 4] / 16.0;
                threshold = (threshold - 0.5) * _DitherStrength + 0.5;

                float levels = _ColorLevels - 1.0;
                float3 dithered = floor(col.rgb * levels + threshold) / levels;

                float3 final = lerp(col.rgb, dithered, _Intensity);
                return fixed4(final, col.a);
            }
            ENDCG
        }
    }
}