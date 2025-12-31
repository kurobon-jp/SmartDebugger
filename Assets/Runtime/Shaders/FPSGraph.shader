Shader "UI/FPSGraph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BackgroundColor ("Background Color", Color) = (0,0,0,0.15)
        _BarWidth ("Bar Width (UV)", Float) = 0.8
        _LineWidth ("Line Width (UV)", Float) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _BackgroundColor;
            float _BarWidth;
            float _LineWidth;
            int _SampleCount;
            float4 _Samples[256];

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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.uv.x * (_SampleCount - 1);
                int idx = (int)x;
                float barH = _Samples[idx].x;
                float warn = saturate(_Samples[idx].y);
                fixed4 col = _BackgroundColor;

                // ==== 棒グラフ ====
                if (i.uv.y <= barH)
                {
                    fixed3 baseCol = lerp(
                        fixed3(0.1, 0.4, 1), // Blue
                        fixed3(1, 0.92, 0.016), // Yellow
                        saturate(warn * 2)
                    );

                    baseCol = lerp(
                        baseCol,
                        fixed3(1.0, 0.2, 0.2), // Red
                        saturate(warn - 0.5) * 2
                    );

                    float alpha = saturate(i.uv.y / max(barH, 1e-4));
                    col = fixed4(baseCol, alpha);
                }

                int idx1 = min(idx + 1, _SampleCount - 1);
                float t = frac(x);

                float targetH0 = _Samples[idx].z;
                float targetH1 = _Samples[idx1].z;
                float targetH = lerp(targetH0, targetH1, t);

                // ==== Target FPS Line ====
                float dist = abs(i.uv.y - targetH);
                float alpha = saturate(1 - dist / _LineWidth);
                if (alpha > 0)
                {
                    col = fixed4(1, 1, 1, alpha); // White line
                }

                return col;
            }
            ENDCG
        }
    }
}