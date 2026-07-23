Shader "Custom/URP2DSpriteOutline"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1,1,1,1)

        [Toggle] _UseOutline("Enable Outline", Float) = 1
        _OutlineColor("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 1.0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            // 使用傳統的 Alpha 混合模式，相容性最高
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float4 color      : COLOR;
                    float2 uv         : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 color      : COLOR;
                    float2 uv         : TEXCOORD0;
                };

                Texture2D _MainTex;
                SamplerState sampler_MainTex;
                float4 _MainTex_TexelSize;

                CBUFFER_START(UnityPerMaterial)
                    float4 _Color;
                    float _UseOutline;
                    float4 _OutlineColor;
                    float _OutlineWidth;
                CBUFFER_END

                Varyings vert(Attributes input)
                {
                    Varyings output;
                    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                    output.uv = input.uv;
                    output.color = input.color * _Color;
                    return output;
                }

                float4 frag(Varyings input) : SV_Target
                {
                    // 採樣原貼圖與頂點顏色
                    float4 col = _MainTex.Sample(sampler_MainTex, input.uv) * input.color;

                    // 如果開關未開啟，直接輸出原圖
                    if (_UseOutline <= 0.0)
                    {
                        return col;
                    }

                    // 根據像素大小計算採樣偏移量
                    float2 texelSize = _MainTex_TexelSize.xy * _OutlineWidth;

                    // 採樣上下左右四周的 Alpha 值
                    float alphaUp = _MainTex.Sample(sampler_MainTex, input.uv + float2(0, texelSize.y)).a;
                    float alphaDown = _MainTex.Sample(sampler_MainTex, input.uv - float2(0, texelSize.y)).a;
                    float alphaLeft = _MainTex.Sample(sampler_MainTex, input.uv - float2(texelSize.x, 0)).a;
                    float alphaRight = _MainTex.Sample(sampler_MainTex, input.uv + float2(texelSize.x, 0)).a;

                    // 取四周最大的 Alpha
                    float maxAlpha = max(max(alphaUp, alphaDown), max(alphaLeft, alphaRight));

                    // 當前像素透明 (col.a 近似於 0)，但四周有顏色的地方就是邊緣
                    float outlineMask = maxAlpha - col.a;
                    outlineMask = saturate(outlineMask);

                    // 計算最後顏色：原本顏色與外框顏色混合
                    float3 finalColor = lerp(col.rgb, _OutlineColor.rgb, outlineMask);
                    float finalAlpha = max(col.a, outlineMask * _OutlineColor.a);

                    return float4(finalColor, finalAlpha);
                }
                ENDHLSL
            }
        }
}