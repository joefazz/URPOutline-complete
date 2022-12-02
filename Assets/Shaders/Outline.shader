Shader "RenderObjects/Outline"
{
    Properties
    {
        _OutlineColor ("OutlineColor", Color) = (0.0, 0.0, 0.0, 1.0)
        _Scale ("Scale", float) = 1
        _DepthThreshold ("DepthThreshold", float) = 1
        _NormalThreshold ("NormalThreshold", float) = 1
        _DepthNormalThreshold ("DepthNormalThreshold", float) = 1
        _DepthNormalThresholdScale ("DepthNormalThresholdScale", float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderingPipeline"="UniversalPipeline"
        }
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "OutlinePass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes
            {
                float4 positionHCS : POSITION;
                float2 uv : TEXCOORD0;
                float3 positionVS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionVS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);

                o.positionCS = float4(input.positionHCS.xy, 0.0, 1.0);
                o.uv = (o.positionCS.xy + 1.0) * 0.5;
                o.positionVS = mul(unity_MatrixInvP, o.positionCS);

                // If we're on a Direct3D like platform
                #if UNITY_UV_STARTS_AT_TOP
                    o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
                    o.positionVS.xy = input.positionVS.xy * 2 - 1;
                #endif
                
                
                return o;
            }

            float4 _OutlineColor;
            float _Scale;
            float _DepthThreshold;
            float _NormalThreshold;
            float _DepthNormalThreshold;
            float _DepthNormalThresholdScale;

            TEXTURE2D(_CameraOpaqueTexture);
            TEXTURE2D(_CameraDepthTexture);

            SAMPLER(sampler_CameraOpaqueTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _CameraOpaqueTexture_TexelSize;

            half4 frag(Attributes input) : SV_Target
            {
                float halfScaleFloor = floor(_Scale * 0.5);
                float halfScaleCeil = ceil(_Scale * 0.5);

                float2 bottomLeftUV = input.uv - float2(_CameraOpaqueTexture_TexelSize.x,
                    _CameraOpaqueTexture_TexelSize.y) * halfScaleFloor;
                float2 topRightUV = input.uv + float2(_CameraOpaqueTexture_TexelSize.x,
                    _CameraOpaqueTexture_TexelSize.y) * halfScaleCeil;
                float2 bottomRightUV = input.uv + float2(_CameraOpaqueTexture_TexelSize.x * halfScaleCeil,
                    -_CameraOpaqueTexture_TexelSize.y * halfScaleFloor);
                float2 topLeftUV = input.uv + float2(-_CameraOpaqueTexture_TexelSize.x * halfScaleFloor,
                    _CameraOpaqueTexture_TexelSize.y * halfScaleCeil);
                
                float depth0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomLeftUV).r;
                float depth1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topRightUV).r;
                float depth2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomRightUV).r;
                float depth3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topLeftUV).r;

                // Convert the Normals from ObjectSpace to ViewSpace
                float3 normal0 = normalize(mul(UNITY_MATRIX_MV, SampleSceneNormals(bottomLeftUV)));
                float3 normal1 = normalize(mul(UNITY_MATRIX_MV, SampleSceneNormals(topRightUV)));
                float3 normal2 = normalize(mul(UNITY_MATRIX_MV, SampleSceneNormals(bottomRightUV)));
                float3 normal3 = normalize(mul(UNITY_MATRIX_MV, SampleSceneNormals(topLeftUV)));

                float3 normalFiniteDifference0 = normal1 - normal0;
                float3 normalFiniteDifference1 = normal3 - normal2;

                float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
                edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;

                float depthFiniteDifference0 = depth1 - depth0;
                float depthFiniteDifference1 = depth3 - depth2;

                float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;

                float3 viewNormal = normal0 * 2 - 1;

                float NdotV = 1 - dot(viewNormal, -input.positionVS);

                float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
                float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;
                
                float depthThreshold = _DepthThreshold * depth0 * normalThreshold;
                edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

                float edge = max(edgeDepth, edgeNormal);

                clip(edge - 0.05);
                
                return float4(edge.xxx, 1.0) * _OutlineColor;
            }
            ENDHLSL
        }
    }
}