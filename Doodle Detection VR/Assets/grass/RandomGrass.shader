Shader "Grass/RandomGrass"
{
    Properties
    {
        _ColorBase("ColorBase", Color) = (1, 1, 1, 1)
        _ColorTip("ColorTip", Color) = (1, 1, 1, 1)
        _AreaSize("AreaSize", Float) = 100
        _Rotation("Rotation", Float) = 0
        _WindBendAmound("WindBendAmound", Float) = 0
        _WindBendFrequency("WindBendFrequency", Float) = 0
        _WindNoise("WindNoiseTexture", 2D) = "white" {}
        _WindNoiseBendAmound("WindBendAmound", Float) = 0
        _WindNoiseBendFrequency("WindBendFrequency", Float) = 0
        _HeightNoiseTex("HeightNoise", 2D) = "white" {}
        _HeightNoiseAmp("HeightNoiseAmplitude", Float) = 1
        _GrassPosY("GrassPositionY", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Back
            ZTest Less
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            float4 _ColorBase;
            float4 _ColorTip;
            float _AreaSize;
            float _Rotation;
            float _WindBendAmound;
            float _WindBendFrequency;
            float _WindNoiseBendAmound;
            float _WindNoiseBendFrequency;
            sampler2D _WindNoise;
            float4 _WindNoise_ST;
            sampler2D _HeightNoiseTex;
            float _HeightNoiseAmp;
            float _GrassPosY;

            StructuredBuffer<float3> PositionBuffer;

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 normal : normal;
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 color : COLOR;
            };

            float4 RotateAroundYInDegrees(float4 vertex, float degrees)
            {
                float alpha = degrees * 3.14159 / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }


            half3 ApplySingleDirectLight(Light light, half3 normal, half3 albedo)
            {
                half directDiffuse = dot(normal, light.direction) * 0.5 + 0.5;
                half3 lighting = light.color * (light.shadowAttenuation * light.distanceAttenuation);
                // add some light to avoid black color
                directDiffuse = clamp(directDiffuse, 0.15, 1);
                lighting = clamp(lighting, 0.3, 1);
                half3 result = (albedo * directDiffuse) * lighting;
                return result;
            }

            half3 ApplySingleDirectLightNoClamp(Light light, half3 normal, half3 albedo)
            {
                half directDiffuse = dot(normal, light.direction) * 0.5 + 0.5;
                half3 lighting = light.color * (light.shadowAttenuation * light.distanceAttenuation);
                // add some light to avoid black color
                directDiffuse = clamp(directDiffuse, 0, 1);
                lighting = clamp(lighting, 0, 1);
                half3 result = (albedo * directDiffuse) * lighting;
                return result;
            }

            v2f vert(VertexData v, uint instanceID : SV_InstanceID)
            {
                half sizeFactor = 0.1;
                float3 instancePosition = PositionBuffer[instanceID];
                // move grass up to match the noise tex
                float4 heightNoise = tex2Dlod(_HeightNoiseTex, float4(instancePosition.xz, 0, 0));
                float displaced = heightNoise.r * _HeightNoiseAmp + _GrassPosY;
                instancePosition.y = -displaced;
                // add -0.5 to both axles to center the grass
                instancePosition.xz += float2(-0.5, -0.5);
                instancePosition.xz *= _AreaSize;

                float3 vertexWorldPosition = RotateAroundYInDegrees(v.vertex, -_Rotation) + instancePosition;
                float2 windUV = vertexWorldPosition.xz * _WindNoise_ST.xy + _WindNoise_ST.zw + normalize(half3(1, 0, 0) * _WindNoiseBendFrequency * _Time.y);
                float2 noise = (tex2Dlod(_WindNoise, float4(windUV, 0, 0)).xy * 2 - 1) * length(half3(1, 0, 0));
                vertexWorldPosition.x += noise.r * (vertexWorldPosition.y + displaced) * _WindNoiseBendAmound;
                vertexWorldPosition.x += sin((vertexWorldPosition.x/15) + _Time * _WindBendFrequency) * (vertexWorldPosition.y + displaced) * _WindBendAmound;

                v2f o;

                float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;//UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector
                //o.vertex = mul(UNITY_MATRIX_VP, float4(vertexWorldPosition, 10));
                o.vertex = TransformWorldToHClip(vertexWorldPosition * sizeFactor);
                //o.normal = v.normal;
                //o.uv = v.uv;
                //float positionWorldSpace = v.vertex + vertexWorldPosition;

                Light mainLight;
                //#if _MAIN_LIGHT_SHADOWS
                    mainLight = GetMainLight(TransformWorldToShadowCoord(vertexWorldPosition * sizeFactor));
                //#else
                    //mainLight = GetMainLight();
                //#endif

                half3 randomAdditionToNormal = sin(instancePosition.x * 82.32523 + instancePosition.z);
                //half3 randomAdditionToNormal = sin(vertexWorldPosition.y * 82.32523);
                half3 normal = normalize(half3(0, 1, 0) + randomAdditionToNormal - cameraTransformForwardWS * 0.5);
                half3 albedo = lerp(_ColorBase, _ColorTip, clamp(v.vertex.y, 0, 1));
                half3 lightingResult = SampleSH(0) * albedo;
                lightingResult = ApplySingleDirectLight(mainLight, normal, albedo);

                //#if _ADDITIONAL_LIGHTS
                    int additionalLightsCount = GetAdditionalLightsCount();
                    for (int i = 0; i < additionalLightsCount; i++) {
                        Light light = GetAdditionalLight(i, vertexWorldPosition * sizeFactor);
                        lightingResult += ApplySingleDirectLightNoClamp(light, normal, albedo);
                    }
                //#endif

                float fogFactor = ComputeFogFactor(o.vertex.z);
                o.color = MixFog(lightingResult, fogFactor);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return half4(i.color, 1);
            }
            ENDHLSL
        }
    }
}
