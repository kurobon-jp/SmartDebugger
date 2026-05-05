Shader "SmartDebug/CpuGraph"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // UI（RawImage）での描画を想定
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha // 半透明合成
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_base_ui
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // C#から渡すデータ
            uniform int _SampleCount;
            uniform int _SampleOffset;
            uniform float4 _Samples[256]; // x:Main, y:Render, z:GPU, w:Margin
            uniform float _MaxTime; // グラフの縦軸の最大値 (ミリ秒)
            uniform float _TargetTime; // ターゲット横線の位置 (ミリ秒)
            fixed4 _LineColor;

            v2f vert(appdata_base_ui v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. フレームのインデックスを計算 (0-99)
                float rawIndex = i.uv.x * (_SampleCount - 1);
                int index = (_SampleOffset + 1 + (int)rawIndex) % _SampleCount;
                // --- 隙間の計算を追加 ---
                // 各棒の幅の中での相対位置 (0.0 ～ 1.0) を取得
                float localX = frac(rawIndex);
                float margin = 0.2; // 隙間の割合 (0.2 なら 20% が隙間)
                fixed4 col = fixed4(0, 0, 0, 0);
                float val = i.uv.y; // 現在のピクセルの高さ(0-1)
                // localX が margin より小さい場合は「隙間」として描画
                // または、両端に隙間を入れるなら (localX < margin || localX > 1.0 - margin)

                float barMask = smoothstep(margin, margin + 0.05, localX);
                // if (localX >= margin && localX <= 1.0 - margin)
                {
                    // X軸(0-1)をデータ配列のインデックス(0-99)に変換
                    // int index = (int)(i.uv.x * 99);

                    // データを取得し、グラフの高さ(0-1)に正規化
                    float4 rawData = _Samples[index];

                    // Stackの計算（下から順に積み上げる）
                    float s = rawData.x / _MaxTime; // Scripts
                    float r = rawData.y / _MaxTime; // Render
                    float p = rawData.z / _MaxTime; // Physics
                    float o = rawData.w / _MaxTime; // Others


                    // --- 1. 背景（半透明黒）で初期化 ---


                    // --- 2. 積み上げ棒グラフの描画（下から順に判定） ---
                    // ※Profilerの見た目に合わせて、上に行くほど手前の色にする
                    if (val < o)
                        col = fixed4(0.378, 0.382, 0.018, 1); // 灰: Others
                    else if (val < o + p)
                        col = fixed4(0.769, 0.424, 0.000, 1); // 黄: Physics
                    else if (val < o + p + s)
                        col = fixed4(0.204, 0.533, 0.655, 1); // 緑: Scripts
                    else if (val < o + p + s + r)
                        col = fixed4(0.184, 0.624, 0.000, 1); // 青: Render

                    // if (val < s)
                    //     col = fixed4(0.204, 0.533, 0.655, 1); // 緑: Scripts
                    // else if (val < s + r)
                    //     col = fixed4(0.482, 0.620, 0.020, 1); // 青: Render
                    // else if (val < s + r + p)
                    //     col = fixed4(0.769, 0.424, 0.000, 1); // 黄: Physics
                    // else if (val < s + r + p + o)
                    //     col = fixed4(0.478, 0.482, 0.118, 1); // 灰: Others
                }
                // --- 3. ターゲット横線の描画（最前面） ---
                // float targetY = _TargetTime / _MaxTime;

                // ジャギーを減らすために smoothstep を使用
                // float lineThickness = 0.005; // 線の太さの半分
                // float lineSoftness = 0.002; // 線のエッジのボケ
                //
                // float lineA = smoothstep(targetY - lineThickness - lineSoftness, targetY - lineThickness, val);
                // float lineB = smoothstep(targetY + lineThickness, targetY + lineThickness + lineSoftness, val);
                // float lineMask = lineA * (1.0 - lineB);
                //
                // // 線の色をアルファ合成
                // col = lerp(col, _LineColor, lineMask * _LineColor.a);
                return lerp(fixed4(0, 0, 0, 0), col, barMask);
                // return col;
            }
            ENDCG
        }
    }
}