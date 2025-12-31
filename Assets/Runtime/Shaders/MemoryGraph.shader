Shader "UI/MemoryGraph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineWidth ("Line Width (UV)", Float) = 0.01
        _MonoLineColor ("Mono Line Color", Color) = (0.3, 0.9, 1)
        _TotalLineColor ("Total Line Color", Color) = (1, 0.6, 0.2)
        _BackgroundColor ("Background Color", Color) = (0,0,0,0.2)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
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

            fixed4 _MonoLineColor;
            fixed4 _TotalLineColor;
            fixed4 _BackgroundColor;
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
                int idx1 = min(idx + 1, _SampleCount - 1);

                float t = frac(x);
                float mono0 = _Samples[idx].x;
                float mono1 = _Samples[idx1].x;
                float total0 = _Samples[idx].y;
                float total1 = _Samples[idx1].y;

                float monoH = lerp(mono0, mono1, t);
                float totalH = lerp(total0, total1, t);

                float distMono = abs(i.uv.y - monoH);
                float distTotal = abs(i.uv.y - totalH);

                float aMono = saturate(1 - distMono / _LineWidth);
                float aTotal = saturate(1 - distTotal / _LineWidth);

                float alpha = max(aMono, aTotal);
                if (alpha <= 0) return _BackgroundColor;
                fixed3 col =
                    aMono > aTotal
                        ? _MonoLineColor
                        : _TotalLineColor;

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}