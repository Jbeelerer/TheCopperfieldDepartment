Shader "Custom/URP/BillboardDustLit"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _LightBoost ("Light Boost", Range(0,3)) = 1.5
        _MinLight ("Minimum Light", Range(0,1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _LightBoost;
            float _MinLight;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                // URP fog
                OUT.fogFactor = ComputeFogFactor(OUT.positionCS.z);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = tex2D(_MainTex, IN.uv) * _Color;

                // Camera-facing normal for billboards
                float3 normalWS = normalize(GetWorldSpaceViewDir(IN.positionWS));

                float3 lighting = 0;

                // ===== MAIN LIGHT =====
                Light mainLight = GetMainLight();
                float3 mainDir = normalize(mainLight.direction);

                float mainNdotL = abs(dot(normalWS, -mainDir));
                float mainLightAmt = max(mainNdotL, _MinLight);

                lighting += mainLight.color * mainLightAmt * mainLight.distanceAttenuation;

                // ===== ADDITIONAL LIGHTS =====
                #ifdef _ADDITIONAL_LIGHTS
                uint count = GetAdditionalLightsCount();
                for (uint i = 0; i < count; i++)
                {
                    Light light = GetAdditionalLight(i, IN.positionWS);
                    float3 dir = normalize(light.direction);
                    float ndotl = abs(dot(normalWS, -dir));
                    float amt = max(ndotl, _MinLight);
                    lighting += light.color * amt * light.distanceAttenuation;
                }
                #endif

                tex.rgb *= lighting * _LightBoost;

                // URP fog application
                tex.rgb = MixFog(tex.rgb, IN.fogFactor);

                return tex;
            }
            ENDHLSL
        }
    }
}