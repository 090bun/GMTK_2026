Shader "Hidden/GlitchPostProcess"
{
    Properties
    {
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "GlitchFullScreenPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _GlitchIntensity;

            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float2 Hash21(float p)
            {
                return float2(Hash11(p), Hash11(p + 17.17));
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float intensity = saturate(_GlitchIntensity);

                if (intensity <= 0.0001)
                {
                    return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0);
                }

                float time = _Time.y;

                // 水平色塊隨機位移
                float blockCount = 24.0;
                float blockId = floor(uv.y * blockCount + floor(time * 12.0));
                float blockRand = Hash11(blockId);
                float shift = (blockRand - 0.5) * 0.12 * intensity;
                shift *= step(1.0 - intensity * 0.9, blockRand);

                float2 glitchedUv = uv;
                glitchedUv.x = saturate(uv.x + shift);

                // RGB 色差分離
                float split = 0.01 * intensity;
                float r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, saturate(glitchedUv + float2(split, 0)), 0).r;
                float g = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, glitchedUv, 0).g;
                float b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, saturate(glitchedUv - float2(split, 0)), 0).b;
                float3 col = float3(r, g, b);

                // 掃描線
                float scanline = sin(uv.y * 800.0 + time * 20.0) * 0.5 + 0.5;
                col *= lerp(1.0, 0.85 + scanline * 0.15, intensity);

                // 隨機白雜訊閃爍
                float noiseSeed = floor(time * 30.0);
                float2 noiseUv = floor(uv * 96.0);
                float noise = Hash11(noiseSeed + noiseUv.x * 13.0 + noiseUv.y * 7.0);
                float noiseFlash = step(1.0 - 0.06 * intensity, noise);
                col = lerp(col, float3(1, 1, 1), noiseFlash * intensity);

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
