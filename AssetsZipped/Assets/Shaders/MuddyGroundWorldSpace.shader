Shader "Custom/MuddyGroundWorldSpace"
{
    Properties
    {
        _BaseMap ("Albedo (Dirt)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BaseColor ("Color Tint", Color) = (0.68, 0.52, 0.34, 1)
        _Tiling ("World Tiling", Float) = 0.3
        _Smoothness ("Smoothness", Range(0,1)) = 0.08
        _BumpScale ("Normal Strength", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);    SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _Tiling;
                float  _Smoothness;
                float  _BumpScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS  = posInputs.positionCS;
                OUT.positionWS   = posInputs.positionWS;
                OUT.normalWS     = normInputs.normalWS;
                OUT.tangentWS    = normInputs.tangentWS;
                OUT.bitangentWS  = normInputs.bitangentWS;
                OUT.fogFactor    = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // World-space XZ UV — completely ignores per-tile UV mapping
                float2 worldUV = IN.positionWS.xz * _Tiling;

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, worldUV);
                albedo.rgb *= _BaseColor.rgb;

                // Sample normal map
                half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, worldUV);
                half3 normalTS = UnpackNormalScale(normalSample, _BumpScale);

                // Transform normal from tangent to world space
                float3x3 TBN = float3x3(
                    normalize(IN.tangentWS),
                    normalize(IN.bitangentWS),
                    normalize(IN.normalWS)
                );
                float3 normalWS = normalize(mul(normalTS, TBN));

                // Lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS   = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord    = IN.fogFactor;
                inputData.bakedGI     = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.shadowMask  = SAMPLE_SHADOWMASK(float2(0,0));

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo      = albedo.rgb;
                surfaceData.alpha       = 1.0;
                surfaceData.metallic    = 0.0;
                surfaceData.smoothness  = _Smoothness;
                surfaceData.normalTS    = normalTS;
                surfaceData.occlusion   = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, IN.fogFactor);
                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Depth only pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
