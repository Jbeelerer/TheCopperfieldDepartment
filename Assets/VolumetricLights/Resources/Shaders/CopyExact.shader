Shader "Hidden/VolumetricLights/CopyExact"
{
Properties
{
    _MainTex("", any) = "" {}
}
SubShader
{
    ZWrite Off ZTest Always Blend Off Cull Off

    HLSLINCLUDE
    #pragma target 3.0
    #pragma prefer_hlslcc gles
    #pragma exclude_renderers d3d11_9x
    ENDHLSL

  Pass { // 0
      Name "Copy Exact"
	  HLSLPROGRAM
      #pragma vertex VertBlit
      #pragma fragment FragCopyExact
      #include "Blur.hlsl"
      ENDHLSL

  }

}
}
