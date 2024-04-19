#ifndef RGI_COMMON_SRP_TRANSLATORS
#define RGI_COMMON_SRP_TRANSLATORS

	// Copyright 2022 Kronnect - All Rights Reserved.
	
    // URP to built-in translators
    #define TEXTURE2D_X(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
    #define TEXTURE2D(tex) UNITY_DECLARE_TEX2D_NOSAMPLER(tex)
    #define SAMPLE_TEXTURE2D_X_LOD(tex, sampler, uv, level) tex.SampleLevel(sampler, uv, level)
    #define SAMPLE_TEXTURE2D_X(tex, sampler, uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, uv)
    #define SAMPLE_TEXTURE2D_LOD(tex, sampler, uv, level) tex.SampleLevel(sampler, uv, level)
    #define TEXTURECUBE(cube) TextureCube cube
    #define SAMPLERCUBE(cube) samplerCUBE cube
    #define SAMPLER(sampler) SamplerState sampler
    #define SAMPLE_TEXTURECUBE_LOD(cube, sampler, dir, level) cube.SampleLevel(sampler, dir, level)

    #define _GBuffer0 _CameraGBufferTexture0
    #define _GBuffer1 _CameraGBufferTexture1
    #define _MotionVectorTexture _CameraMotionVectorsTexture

    SamplerState sampler_LinearClamp;
    SamplerState sampler_PointClamp;
    SamplerState sampler_PointRepeat;

    #define DecodeHDREnvironment(s,d) DecodeHDR(s, d)

#endif // RGI_COMMON_SRP_TRANSLATORS