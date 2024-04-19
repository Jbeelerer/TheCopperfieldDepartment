//------------------------------------------------------------------------------------------------------------------
// Volumetric Lights
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricLights {

    [ExecuteAlways]
    [ImageEffectAllowedInSceneView]
    [HelpURL("https://kronnect.com/guides/volumetric-lights-2-builtin-new-feature-volumetric-lights-post-process/")]
    public abstract class VolumetricLightsPostProcessBase : MonoBehaviour {

        static class ShaderParams {
            public static int LightBuffer = Shader.PropertyToID("_LightBuffer");
            public static int MainTex = Shader.PropertyToID("_MainTex");
            public static int BlurRT = Shader.PropertyToID("_BlurTex");
            public static int BlurRT2 = Shader.PropertyToID("_BlurTex2");
            public static int BlendDest = Shader.PropertyToID("_BlendDest");
            public static int BlendSrc = Shader.PropertyToID("_BlendSrc");
            public static int BlendOp = Shader.PropertyToID("_BlendOp");
            public static int MiscData = Shader.PropertyToID("_MiscData");
            public static int ForcedInvisible = Shader.PropertyToID("_ForcedInvisible");
            public static int DownsampledDepth = Shader.PropertyToID("_DownsampledDepth");
            public static int BlueNoiseTexture = Shader.PropertyToID("_BlueNoise");
            public static int BlurScale = Shader.PropertyToID("_BlurScale");
            public static int Downscaling = Shader.PropertyToID("_Downscaling");

            public const string SKW_DITHER = "DITHER";
            public const string SKW_EDGE_PRESERVE = "EDGE_PRESERVE";
            public const string SKW_EDGE_PRESERVE_UPSCALING = "EDGE_PRESERVE_UPSCALING";
        }

        static int GetScaledSize(int size, float factor) {
            size = (int)(size / factor);
            size /= 2;
            if (size < 1)
                size = 1;
            return size * 2;
        }

        class VolumetricLightsRenderPass {

            const string m_ProfilerTag = "Volumetric Lights Post Process Rendering";
            const string m_LightBufferName = "_LightBuffer";

            VolumetricLightsPostProcessBase settings;
            RenderTargetIdentifier m_LightBuffer;
            CommandBuffer cmd;

            enum Pass {
                BlurHorizontal = 0,
                BlurVertical = 1,
                BlurVerticalAndBlend = 2,
                Blend = 3,
                DownscaleDepth = 4,
                BlurVerticalFinal = 5
            }

            Material blurMat, copyMat;

            public void Cleanup() {
                DestroyImmediate(blurMat);
                Shader.SetGlobalInt(ShaderParams.ForcedInvisible, 0);
            }

            public void Setup(VolumetricLightsPostProcessBase settings, Shader blurShader, Shader copyShader) {
                this.settings = settings;
                if (cmd == null) {
                    cmd = new CommandBuffer();
                    cmd.name = m_ProfilerTag;
                }
                if (blurMat == null) {
                    blurMat = new Material(blurShader);
                    Texture2D noiseTex = Resources.Load<Texture2D>("Textures/blueNoiseVL128");
                    blurMat.SetTexture(ShaderParams.BlueNoiseTexture, noiseTex);
                }
                if (copyMat == null) {
                    copyMat = new Material(copyShader);
                }

                switch (settings.blendMode) {
                    case BlendMode.Additive:
                        blurMat.SetInt(ShaderParams.BlendOp, (int)UnityEngine.Rendering.BlendOp.Add);
                        blurMat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.One);
                        blurMat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.One);
                        break;
                    case BlendMode.Blend:
                        blurMat.SetInt(ShaderParams.BlendOp, (int)UnityEngine.Rendering.BlendOp.Add);
                        blurMat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.One);
                        blurMat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        break;
                    case BlendMode.PreMultiply:
                        blurMat.SetInt(ShaderParams.BlendOp, (int)UnityEngine.Rendering.BlendOp.Add);
                        blurMat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        blurMat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.One);
                        break;
                    case BlendMode.Substractive:
                        blurMat.SetInt(ShaderParams.BlendOp, (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                        blurMat.SetInt(ShaderParams.BlendSrc, (int)UnityEngine.Rendering.BlendMode.One);
                        blurMat.SetInt(ShaderParams.BlendDest, (int)UnityEngine.Rendering.BlendMode.One);
                        break;
                }
                blurMat.SetVector(ShaderParams.MiscData, new Vector4(settings.ditherStrength * 0.1f, settings.brightness, settings.blurEdgeDepthThreshold, 0));
                if (settings.ditherStrength > 0) {
                    blurMat.EnableKeyword(ShaderParams.SKW_DITHER);
                } else {
                    blurMat.DisableKeyword(ShaderParams.SKW_DITHER);
                }
                blurMat.DisableKeyword(ShaderParams.SKW_EDGE_PRESERVE);
                blurMat.DisableKeyword(ShaderParams.SKW_EDGE_PRESERVE_UPSCALING);
                if (settings.blurPasses > 0 && settings.blurEdgePreserve) {
                    blurMat.EnableKeyword(settings.downscaling > 1f ? ShaderParams.SKW_EDGE_PRESERVE_UPSCALING : ShaderParams.SKW_EDGE_PRESERVE);
                }
                if (cmd == null) {
                    cmd = new CommandBuffer();
                    cmd.name = "m_ProfilerTag";
                }
            }



            public void Execute(RenderTexture source, RenderTexture destination) {

                if (!settings.isActive) {
                    Graphics.Blit(source, destination, copyMat, 0);
                    return;
                }

                cmd.Clear();
                cmd.SetGlobalInt(ShaderParams.ForcedInvisible, 0);

                RenderTextureDescriptor lightBufferDesc = source.descriptor;
                lightBufferDesc.width = GetScaledSize(lightBufferDesc.width, settings.downscaling); ;
                lightBufferDesc.height = GetScaledSize(lightBufferDesc.height, settings.downscaling); ;
                lightBufferDesc.depthBufferBits = 0;
                lightBufferDesc.useMipMap = false;
                lightBufferDesc.msaaSamples = 1;
                RenderTextureDescriptor rtBlurDesc = lightBufferDesc;

                cmd.GetTemporaryRT(ShaderParams.LightBuffer, lightBufferDesc, FilterMode.Bilinear);

                RenderTargetIdentifier lightBuffer = new RenderTargetIdentifier(ShaderParams.LightBuffer, 0, CubemapFace.Unknown, -1);
                cmd.SetRenderTarget(lightBuffer);
                cmd.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));

                foreach (VolumetricLight vl in VolumetricLight.volumetricLights) {
                    if (vl != null) {
                        cmd.DrawRenderer(vl.meshRenderer, vl.meshRenderer.sharedMaterial);
                    }
                }

                cmd.SetGlobalInt(ShaderParams.ForcedInvisible, 1);


                rtBlurDesc.colorFormat = settings.blurHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;

                bool usingDownscaling = settings.downscaling > 1f;
                if (usingDownscaling) {
                    RenderTextureDescriptor rtDownscaledDepth = rtBlurDesc;
                    rtDownscaledDepth.colorFormat = RenderTextureFormat.RHalf;
                    cmd.GetTemporaryRT(ShaderParams.DownsampledDepth, rtDownscaledDepth, FilterMode.Bilinear);
                    FullScreenBlit(cmd, source, ShaderParams.DownsampledDepth, blurMat, (int)Pass.DownscaleDepth);
                }

                if (settings.blurPasses < 1) {
                    cmd.SetGlobalFloat(ShaderParams.BlurScale, settings.blurSpread);
                    FullScreenBlit(cmd, ShaderParams.LightBuffer, source, blurMat, (int)Pass.Blend);
                } else {
                    rtBlurDesc.width = GetScaledSize(rtBlurDesc.width, settings.blurDownscaling);
                    rtBlurDesc.height = GetScaledSize(rtBlurDesc.height, settings.blurDownscaling);
                    cmd.GetTemporaryRT(ShaderParams.BlurRT, rtBlurDesc, FilterMode.Bilinear);
                    cmd.GetTemporaryRT(ShaderParams.BlurRT2, rtBlurDesc, FilterMode.Bilinear);
                    cmd.SetGlobalFloat(ShaderParams.BlurScale, settings.blurSpread * settings.blurDownscaling);
                    FullScreenBlit(cmd, ShaderParams.LightBuffer, ShaderParams.BlurRT, blurMat, (int)Pass.BlurHorizontal);
                    cmd.SetGlobalFloat(ShaderParams.BlurScale, settings.blurSpread);
                    for (int k = 0; k < settings.blurPasses - 1; k++) {
                        FullScreenBlit(cmd, ShaderParams.BlurRT, ShaderParams.BlurRT2, blurMat, (int)Pass.BlurVertical);
                        FullScreenBlit(cmd, ShaderParams.BlurRT2, ShaderParams.BlurRT, blurMat, (int)Pass.BlurHorizontal);
                    }
                    if (usingDownscaling) {
                        FullScreenBlit(cmd, ShaderParams.BlurRT, ShaderParams.BlurRT2, blurMat, (int)Pass.BlurVerticalFinal);
                        FullScreenBlit(cmd, ShaderParams.BlurRT2, source, blurMat, (int)Pass.Blend);
                    } else {
                        FullScreenBlit(cmd, ShaderParams.BlurRT, source, blurMat, (int)Pass.BlurVerticalAndBlend);
                    }

                    cmd.ReleaseTemporaryRT(ShaderParams.BlurRT2);
                    cmd.ReleaseTemporaryRT(ShaderParams.BlurRT);
                }
                cmd.ReleaseTemporaryRT(ShaderParams.LightBuffer);
                if (usingDownscaling) {
                    cmd.ReleaseTemporaryRT(ShaderParams.DownsampledDepth);
                }

                Graphics.ExecuteCommandBuffer(cmd);

                Graphics.Blit(source, destination, copyMat, 0);

            }



            static Mesh _fullScreenMesh;

            Mesh fullscreenMesh {
                get {
                    if (_fullScreenMesh != null) {
                        return _fullScreenMesh;
                    }
                    float num = 1f;
                    float num2 = 0f;
                    Mesh val = new Mesh();
                    _fullScreenMesh = val;
                    _fullScreenMesh.SetVertices(new List<Vector3> {
            new Vector3 (-1f, -1f, 0f),
            new Vector3 (-1f, 1f, 0f),
            new Vector3 (1f, -1f, 0f),
            new Vector3 (1f, 1f, 0f)
        });
                    _fullScreenMesh.SetUVs(0, new List<Vector2> {
            new Vector2 (0f, num2),
            new Vector2 (0f, num),
            new Vector2 (1f, num2),
            new Vector2 (1f, num)
        });
                    _fullScreenMesh.SetIndices(new int[6] { 0, 1, 2, 2, 1, 3 }, (MeshTopology)0, 0, false);
                    _fullScreenMesh.UploadMeshData(true);
                    return _fullScreenMesh;
                }
            }

            void FullScreenBlit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex) {
                destination = new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1);
                cmd.SetRenderTarget(destination);
                cmd.SetGlobalTexture(ShaderParams.MainTex, source);
                cmd.DrawMesh(fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

        }

        [SerializeField, HideInInspector]
        Shader blurShader;

        [SerializeField, HideInInspector]
        Shader copyShader;
        VolumetricLightsRenderPass vlRenderPass;
        public static bool installed;

        public BlendMode blendMode = BlendMode.Additive;

        [Range(1, 4)]
        public float downscaling = 1;
        [Range(0, 4)]
        public int blurPasses = 1;
        [Range(1, 4)]
        public float blurDownscaling = 1;
        [Range(0.1f, 4)]
        public float blurSpread = 1f;
        [Tooltip("Uses 32 bit floating point pixel format for rendering & blur fog volumes.")]
        public bool blurHDR = true;
        [Tooltip("Enable to use an edge-aware blur.")]
        public bool blurEdgePreserve;
        [Tooltip("Bilateral filter edge detection threshold.")]
        public float blurEdgeDepthThreshold = 0.001f;
        public float brightness = 1f;

        [Range(0, 0.2f)]
        public float ditherStrength;

        void OnEnable() {
            if (vlRenderPass == null) {
                vlRenderPass = new VolumetricLightsRenderPass();
            }
            blurShader = Shader.Find("Hidden/VolumetricLights/Blur");
            copyShader = Shader.Find("Hidden/VolumetricLights/CopyExact");
        }

        void OnDisable() {
            installed = false;
            if (vlRenderPass != null) {
                vlRenderPass.Cleanup();
            }
            Shader.SetGlobalFloat(ShaderParams.Downscaling, 0);
        }

        private void OnValidate() {
            brightness = Mathf.Max(0, brightness);
            ditherStrength = Mathf.Clamp(ditherStrength, 0, 0.2f);
            blurEdgeDepthThreshold = Mathf.Max(0.0001f, blurEdgeDepthThreshold);
        }

        public bool isActive => (downscaling > 1f || blurPasses > 0) && VolumetricLight.volumetricLights.Count > 0;

        void OnPreRender() {
            Shader.SetGlobalInt(ShaderParams.ForcedInvisible, isActive ? 1 : 0);
            Shader.SetGlobalFloat(ShaderParams.Downscaling, downscaling - 1f);
        }

        public void AddRenderPasses(RenderTexture source, RenderTexture destination) {
            vlRenderPass.Setup(this, blurShader, copyShader);
            vlRenderPass.Execute(source, destination);
            installed = true;
        }
    }
}
