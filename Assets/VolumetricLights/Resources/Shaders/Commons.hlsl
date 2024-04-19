#ifndef VOLUMETRIC_LIGHTS_COMMONS
#define VOLUMETRIC_LIGHTS_COMMONS

#include "UnityCG.cginc"
#include "Options.hlsl"
#include "CommonsSRP.hlsl"

#define UNITY_SAMPLE_SCREENSPACE_TEXTURE_LOD(tex, uv, lod) SAMPLE_RAW_DEPTH_TEXTURE_LOD(tex, float4(uv, 0, lod))

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
float4 _CameraDepthTexture_ST;
#ifndef SHADER_API_PS4
CBUFFER_START(UnityPerMaterial)
#endif

float4 _ConeTipData, _ConeAxis;
float4 _ExtraGeoData;
float3 _BoundsCenter, _BoundsExtents;
float3 _MeshBoundsCenter, _MeshBoundsExtents;
float4 _ToLightDir;

float jitter;
float _NoiseScale, _NoiseStrength, _NoiseFinalMultiplier, _Border, _DistanceFallOff;
float3 _FallOff;
half4 _Color;
float4 _AreaExtents;

float4 _RayMarchSettings;
int _RayMarchMaxSteps;
float4 _WindDirection;
half4 _LightColor;
half  _Density;
float _NearClipDistance;
half3 _DirectLightData;
float _Downscaling;

#ifndef SHADER_API_PS4
CBUFFER_END
#endif

sampler3D _NoiseTex;

#define FOG_STEPPING _RayMarchSettings.x
#define DITHERING _RayMarchSettings.y
#define JITTERING _RayMarchSettings.z
#define MIN_STEPPING _RayMarchSettings.w

#define DIRECT_LIGHT_MULTIPLIER _DirectLightData.x
#define DIRECT_LIGHT_SMOOTH_SAMPLES _DirectLightData.y
#define DIRECT_LIGHT_SMOOTH_RADIUS _DirectLightData.z

#if VL_CAST_DIRECT_LIGHT_ADDITIVE || VL_CAST_DIRECT_LIGHT_BLEND
    #define VL_CAST_DIRECT_LIGHT 1
#else
    #define VL_CAST_DIRECT_LIGHT 0
#endif
float SampleSceneDepth(float2 uv) {
    return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(uv, _CameraDepthTexture_ST)); // tested to work with multi-pass, single pass stereo and single pass instanced
}


inline float GetLinearEyeDepth(float2 uv) {
    float rawDepth = SampleSceneDepth(uv);
	float sceneLinearDepth = LinearEyeDepth(rawDepth);
    #if defined(ORTHO_SUPPORT)
        #if UNITY_REVERSED_Z
              rawDepth = 1.0 - rawDepth;
        #endif
        float orthoDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
        sceneLinearDepth = lerp(sceneLinearDepth, orthoDepth, unity_OrthoParams.w);
    #endif
    return sceneLinearDepth;
}

#if defined(STEREO_CUBEMAP_RENDER_ON)

sampler2D_float _ODSWorldTexture;

void ClampRayDepth(float3 rayStart, float2 uv, inout float t1) {
	float3 wpos = tex2D(_ODSWorldTexture, uv);
    float z = distance(rayStart, wpos.xyz);
    t1 = min(t1, z);
}

#else

void ClampRayDepth(float3 rayStart, float2 uv, inout float t1) {

    float vz = GetLinearEyeDepth(uv);
    #if defined(ORTHO_SUPPORT)
        if (unity_OrthoParams.w) {
            t1 = min(t1, vz);
            return;
        }
    #endif
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    float2 suv = uv;
    #if UNITY_SINGLE_PASS_STEREO
        // If Single-Pass Stereo mode is active, transform the
        // coordinates to get the correct output UV for the current eye.
        float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
        suv = (suv - scaleOffset.zw) / scaleOffset.xy;
    #endif
    float3 vpos = float3((suv * 2 - 1) / p11_22, -1) * vz;
    float4 wpos = mul(unity_CameraToWorld, float4(vpos, 1));

    float z = distance(rayStart, wpos.xyz);
    t1 = min(t1, z);
}

#endif

#if VL_CAST_DIRECT_LIGHT

    UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture0);
    float4 _CameraGBufferTexture0_ST;
    UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture1);
    UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture2);

    struct GBufferData {
        half3 albedo, specular, normal;
    };

    half3 SampleSceneNormals(float2 uv) {
        uv = UnityStereoScreenSpaceUVAdjust(uv, _CameraGBufferTexture0_ST);
        float4 normals = UNITY_SAMPLE_SCREENSPACE_TEXTURE_LOD(_CameraGBufferTexture2, uv, 0);
        float3 normalWS = normals.xyz * 2.0 - 1.0;
        return normalWS;
    }


    void GetGBufferData(float2 uv, out GBufferData gbuffer) {
        uv = UnityStereoScreenSpaceUVAdjust(uv, _CameraGBufferTexture0_ST);
        gbuffer.albedo = UNITY_SAMPLE_SCREENSPACE_TEXTURE_LOD(_CameraGBufferTexture0, uv, 0);
        gbuffer.specular = UNITY_SAMPLE_SCREENSPACE_TEXTURE_LOD(_CameraGBufferTexture1, uv, 0).rgb;
        gbuffer.normal = SampleSceneNormals(uv);
    }

#endif

#endif // VOLUMETRIC_LIGHTS_COMMONS

