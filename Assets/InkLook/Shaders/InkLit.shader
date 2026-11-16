Shader "Ink/InkLit"
{
    Properties
    {
        _ToneLight("Light tone", Color) = (0.50, 0.49, 0.46, 1)
        _ToneMid("Mid tone", Color) = (0.23, 0.22, 0.20, 1)
        _ToneShade("Shade tone", Color) = (0.085, 0.08, 0.072, 1)
        _Step1("Shade / mid threshold", Range(0, 1)) = 0.42
        _Step2("Mid / light threshold", Range(0, 1)) = 0.70
        _StepSoft("Step softness", Range(0.001, 0.3)) = 0.025
        _EdgeWobble("Brush wobble", Range(0, 1)) = 0.18
        _NoiseScale("Wobble scale", Float) = 1.2
        _DryBrush("Dry brush", Range(0, 1)) = 0.4
        _DryScale("Dry brush scale (x, y)", Vector) = (0.5, 7, 0, 0)
        _Fade("Fade to paper", Range(0, 1)) = 0
        _Mist("Mist strength", Range(0, 1)) = 0
        _MistTop("Mist top (world Y)", Float) = -8
        _MistRange("Mist range", Float) = 7
        _Bleed("Bleed (wet ink)", Range(0, 1)) = 0
        _Flash("Hit flash", Range(0, 1)) = 0
        [Toggle] _ReceiveFog("Depth fade (world Z)", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _ToneLight;
            half4 _ToneMid;
            half4 _ToneShade;
            half _Step1;
            half _Step2;
            half _StepSoft;
            half _EdgeWobble;
            float _NoiseScale;
            half _DryBrush;
            float4 _DryScale;
            half _Fade;
            half _Mist;
            float _MistTop;
            float _MistRange;
            half _Bleed;
            half _Flash;
            half _ReceiveFog;
            half _Cull;
        CBUFFER_END

        float4 _InkPaperColor; // linear rgb, set by InkWorld
        float4 _InkFogZ;       // x = start, y = 1 / range, z = max
        TEXTURE2D(_InkGrainTex);  // r = bristle streaks, g = blotchy wash, b = paper fibre
        SAMPLER(sampler_InkGrainTex);

        float ink_hash(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float ink_vnoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = ink_hash(i);
            float b = ink_hash(i + float2(1, 0));
            float c = ink_hash(i + float2(0, 1));
            float d = ink_hash(i + float2(1, 1));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        float ink_fbm(float2 p)
        {
            float v = 0.0;
            float a = 0.5;
            for (int i = 0; i < 4; i++)
            {
                v += a * ink_vnoise(p);
                p = p * 2.03 + 17.1;
                a *= 0.5;
            }
            return v;
        }

        // Wet ink erodes: used by every pass so eroded parts cast no shadow and get no outline.
        void InkBleedClip(float3 positionWS)
        {
            if (_Bleed > 0.001)
            {
                float e = ink_fbm(positionWS.xy * 14.0 + float2(0.0, _Time.y * 0.7));
                clip(e + 0.25 - _Bleed * 0.62);
            }
        }

        struct InkAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct InkVaryings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        InkVaryings InkVert(InkAttributes v)
        {
            InkVaryings o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.positionCS = TransformWorldToHClip(o.positionWS);
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex InkVert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            half4 frag(InkVaryings i, bool isFront : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                InkBleedClip(i.positionWS);

                float3 n = normalize(i.normalWS) * (isFront ? 1.0 : -1.0);
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                float ndl = dot(n, mainLight.direction) * 0.5 + 0.5;
                float lit = ndl * lerp(0.62, 1.0, mainLight.shadowAttenuation);

                // Noise in a mostly screen-facing plane so the brush texture reads in a side-on view.
                float2 np = i.positionWS.xy + i.positionWS.zz * float2(0.37, 0.61);
                float4 grain = SAMPLE_TEXTURE2D(_InkGrainTex, sampler_InkGrainTex, np * float2(_DryScale.x * 0.12, _DryScale.y * 0.03));
                lit += (ink_fbm(np * _NoiseScale) - 0.47) * _EdgeWobble + (grain.g - 0.5) * 0.10;

                float t1 = smoothstep(_Step1 - _StepSoft, _Step1 + _StepSoft, lit);
                float t2 = smoothstep(_Step2 - _StepSoft, _Step2 + _StepSoft, lit);
                float3 col = lerp(_ToneShade.rgb, _ToneMid.rgb, t1);
                col = lerp(col, _ToneLight.rgb, t2);

                // Dry brush: streaks where the bristles ran out of ink.
                float dry = smoothstep(0.52, 0.80, grain.r) * _DryBrush;
                float3 dryCol = lerp(_ToneMid.rgb, _ToneLight.rgb, 0.5 + 0.5 * t1);
                col = lerp(col, dryCol, dry * (1.0 - t2));

                // Wet ink runs pale.
                col = lerp(col, lerp(col, _InkPaperColor.rgb, 0.72), _Bleed);

                // Atmosphere: depth layers and low mist both fade to bare paper.
                float fogZ = saturate((i.positionWS.z - _InkFogZ.x) * _InkFogZ.y) * _InkFogZ.z * _ReceiveFog;
                float mist = saturate((_MistTop - i.positionWS.y) / max(_MistRange, 0.001));
                mist = mist * mist * (3.0 - 2.0 * mist);
                float mistNoise = ink_fbm(np * 0.15 + float2(_Time.y * 0.02, 0.0));
                mist = saturate(mist * (0.75 + 0.6 * mistNoise)) * _Mist;
                float fade = 1.0 - (1.0 - fogZ) * (1.0 - mist) * (1.0 - _Fade);
                col = lerp(col, _InkPaperColor.rgb, saturate(fade));

                col = lerp(col, _InkPaperColor.rgb, _Flash);
                return half4(col, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _LightDirection;

            InkVaryings vert(InkAttributes v)
            {
                InkVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(o.positionWS, o.normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS = positionCS;
                return o;
            }

            half4 frag(InkVaryings i) : SV_Target
            {
                InkBleedClip(i.positionWS);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex InkVert
            #pragma fragment frag
            #pragma multi_compile_instancing

            half4 frag(InkVaryings i) : SV_Target
            {
                InkBleedClip(i.positionWS);
                return i.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex InkVert
            #pragma fragment frag
            #pragma multi_compile_instancing

            half4 frag(InkVaryings i, bool isFront : SV_IsFrontFace) : SV_Target
            {
                InkBleedClip(i.positionWS);
                float3 n = normalize(i.normalWS) * (isFront ? 1.0 : -1.0);
                return half4(n, 0.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
