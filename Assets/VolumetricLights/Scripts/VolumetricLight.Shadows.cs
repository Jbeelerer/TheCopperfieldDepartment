//------------------------------------------------------------------------------------------------------------------
// Volumetric Lights
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricLights {

    public partial class VolumetricLight : MonoBehaviour {

        #region Shadow support

        const string SHADOW_CAM_NAME = "OcclusionCam";
        const string m_TranspShader = "Hidden/VolumetricLights/TransparentMultiply";
        const string m_TranspDepthOnlyShader = "Hidden/VolumetricLights/TransparentDepthWrite";

        Camera cam;
        RenderTexture rt;
        int camStartFrameCount;
        Vector3 lastCamPos;
        Quaternion lastCamRot;
        bool usesReversedZBuffer;
        static Matrix4x4 textureScaleAndBias;
        Matrix4x4 shadowMatrix;
        bool camTransformChanged;
        bool shouldOrientToCamera;
        Shader depthShader;
        RenderTexture shadowCubemap;
        readonly static Vector3[] camFaceDirections = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        Material copyDepthIntoCubemap;
        int currentCubemapFace;

        Material transpOverrideMat, transpDepthOnlyMat;
        Material[] transpOverrideMaterials;
        RenderTexture translucentMap;
        CommandBuffer cmdTransp;
        Plane[] frustumPlanes;

        bool usesCubemap { get { return shadowBakeMode != ShadowBakeMode.HalfSphere && generatedType == LightType.Point; } }
        bool usesTranslucency { get { return shadowTranslucency && (generatedType == LightType.Spot || generatedType == LightType.Rectangle || generatedType == LightType.Disc); } }

        void CheckShadowCam() {
            if (cam == null) {
                Transform childCam = transform.Find(SHADOW_CAM_NAME);
                if (childCam != null) {
                    cam = childCam.GetComponent<Camera>();
                    if (cam == null) {
                        // corrupted cam object?
                        DestroyImmediate(childCam.gameObject);
                    }
                }
            }
            SetupCamRenderingProperties();
        }

        void ShadowsDisable() {
            if (cam != null) {
                cam.enabled = false;
            }
        }

        void ShadowsDispose() {
            if (cam != null) {
                cam.targetTexture = null;
                cam.enabled = false;
            }
            DisposeRTs();
        }

        void DisposeRTs() {
            if (rt != null) {
                rt.Release();
                DestroyImmediate(rt);
            }
            if (shadowCubemap != null) {
                shadowCubemap.Release();
                DestroyImmediate(shadowCubemap);
            }
            if (translucentMap != null) {
                translucentMap.Release();
                DestroyImmediate(translucentMap);
            }
            if (transpOverrideMaterials != null) {
                for (int k = 0; k < transpOverrideMaterials.Length; k++) {
                    DestroyImmediate(transpOverrideMaterials[k]);
                }
            }
        }

        void ShadowsSupportCheck() {

            bool usesCookie = cookieTexture != null && lightComp.type == LightType.Spot;
            if (!enableShadows && !usesCookie) {
                ShadowsDispose();
                return;
            }

            usesReversedZBuffer = SystemInfo.usesReversedZBuffer;

            // Setup texture scale and bias matrix
            textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;

            if (cam == null) {
                Transform childCam = transform.Find(SHADOW_CAM_NAME);
                if (childCam != null) {
                    cam = childCam.GetComponent<Camera>();
                    if (cam == null) {
                        // corrupted cam object?
                        DestroyImmediate(childCam.gameObject);
                    }
                }
                if (cam == null) {
                    GameObject camObj = new GameObject(SHADOW_CAM_NAME, typeof(Camera));
                    camObj.transform.SetParent(transform, false);
                    cam = camObj.GetComponent<Camera>();
                    cam.depthTextureMode = DepthTextureMode.None;
                    cam.clearFlags = CameraClearFlags.Depth;
                    cam.allowHDR = false;
                    cam.allowMSAA = false;
                }

                cam.stereoTargetEye = StereoTargetEyeMask.None;
            }

            SetupCamRenderingProperties();

            cam.nearClipPlane = shadowNearDistance;
            cam.orthographicSize = Mathf.Max(generatedAreaWidth, generatedAreaHeight);

            // custom properties per light type
            switch (generatedType) {
                case LightType.Spot:
                    cam.transform.localRotation = Quaternion.identity;
                    cam.orthographic = false;
                    cam.fieldOfView = generatedSpotAngle;
                    break;

                case LightType.Point:
                    cam.orthographic = false;
                    if (shadowBakeMode != ShadowBakeMode.HalfSphere) {
                        cam.fieldOfView = 90f;
                    } else {
                        cam.fieldOfView = 160f;
                    }
                    break;

                case LightType.Area:
                case LightType.Disc:
                    cam.transform.localRotation = Quaternion.identity;
                    cam.orthographic = true;
                    cam.orthographicSize *= generatedAreaFrustumMultiplier;
                    break;
            }


            RenderTextureFormat expectedRTFormat = shadowUseDefaultRTFormat ? RenderTextureFormat.Default : RenderTextureFormat.Depth;
            if (rt != null && (rt.width != (int)shadowResolution || expectedRTFormat != rt.format)) {
                if (cam.targetTexture == rt) {
                    cam.targetTexture = null;
                }
                DisposeRTs();
            }

            if (rt == null) {
                if (shadowUseDefaultRTFormat) {
                    rt = new RenderTexture((int)shadowResolution, (int)shadowResolution, 16);
                } else {
                    rt = new RenderTexture((int)shadowResolution, (int)shadowResolution, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
                }
                rt.antiAliasing = 1;
                rt.useMipMap = false;
            }

            if (usesTranslucency && translucentMap == null) {
                translucentMap = new RenderTexture((int)shadowResolution, (int)shadowResolution, 0, RenderTextureFormat.ARGBHalf);
                translucentMap.antiAliasing = 1;
                translucentMap.useMipMap = false;
            }

            if (usesCubemap && shadowCubemap == null) {
                shadowCubemap = new RenderTexture((int)shadowResolution, (int)shadowResolution, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
                shadowCubemap.dimension = TextureDimension.Cube;
                shadowCubemap.antiAliasing = 1;
                shadowCubemap.useMipMap = false;
            }

            fogMat.SetVector(ShaderParams.ShadowIntensity, new Vector4(shadowIntensity, 1f - shadowIntensity, 0, 0));
            fogMat.SetVector(ShaderParams.ShadowColor, shadowColor);

            if ((shadowCullingMask & 2) != 0) {
                shadowCullingMask &= ~2; // exclude transparent FX layer
            }

            cam.cullingMask = shadowCullingMask;
            cam.targetTexture = rt;

            if (enableShadows) {
                shouldOrientToCamera = true;
                ScheduleShadowCapture();
            } else {
                cam.enabled = false;
            }
        }

        void SetupCamRenderingProperties() {
            if (cam == null) return;

            cam.renderingPath = RenderingPath.Forward;
            cam.depthTextureMode = DepthTextureMode.None;
            cam.clearFlags = CameraClearFlags.Depth;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.stereoTargetEye = StereoTargetEyeMask.None;

            // optimize renderers
            if (depthShader == null) {
                depthShader = Shader.Find("Hidden/VolumetricLights/DepthOnly");
            }
            if (shadowOptimizeShadowCasters) {
                cam.SetReplacementShader(depthShader, null);
            } else {
                cam.ResetReplacementShader();
            }
        }

        /// <summary>
        /// Updates shadows on this volumetric light
        /// </summary>
        public void ScheduleShadowCapture() {
            if (cam == null) return;

            if (usesCubemap) {
                if (copyDepthIntoCubemap == null) {
                    copyDepthIntoCubemap = new Material(Shader.Find("Hidden/VolumetricLights/CopyDepthIntoCubemap"));
                }
                copyDepthIntoCubemap.SetVector(ShaderParams.LightPos, cam.transform.position);
                RenderTexture active = RenderTexture.active;

                int renderFaceCount = shadowBakeMode == ShadowBakeMode.CubemapOneFacePerFrame && shadowBakeInterval == ShadowBakeInterval.EveryFrame ? 1 : 6;
                for (int k = 0; k < renderFaceCount; k++) {
                    int cubemapFace = currentCubemapFace % 6;
                    cam.transform.forward = camFaceDirections[cubemapFace];
                    cam.Render();
                    copyDepthIntoCubemap.SetMatrix(ShaderParams.InvVPMatrix, cam.cameraToWorldMatrix * GL.GetGPUProjectionMatrix(cam.projectionMatrix, false).inverse);
                    copyDepthIntoCubemap.SetTexture(ShaderParams.ShadowTexture, rt, RenderTextureSubElement.Depth);
                    Graphics.SetRenderTarget(shadowCubemap, 0, (CubemapFace)cubemapFace);
                    Graphics.Blit(rt, copyDepthIntoCubemap);

                    currentCubemapFace++;
                }
                cam.enabled = false;
                RenderTexture.active = active;

                fogMat.SetTexture(ShaderParams.ShadowCubemap, shadowCubemap);
                if (enableDustParticles && particleMaterial != null) {
                    particleMaterial.SetTexture(ShaderParams.ShadowCubemap, shadowCubemap);
                }
                if (!fogMat.IsKeywordEnabled(ShaderParams.SKW_SHADOWS_CUBEMAP)) {
                    fogMat.EnableKeyword(ShaderParams.SKW_SHADOWS_CUBEMAP);
                }
            } else {
                cam.enabled = true;
                camStartFrameCount = Time.frameCount;
                string shadowKeyword = usesTranslucency ? ShaderParams.SKW_SHADOWS_TRANSLUCENCY : ShaderParams.SKW_SHADOWS;
                if (!fogMat.IsKeywordEnabled(shadowKeyword)) {
                    fogMat.EnableKeyword(shadowKeyword);
                }

                if (usesTranslucency) {
                    CaptureTransparents();
                }
            }
        }


        void SetupShadowMatrix() {

            if (usesCubemap) return;

            ComputeShadowTransform(cam.projectionMatrix, cam.worldToCameraMatrix);

            fogMat.SetMatrix(ShaderParams.ShadowMatrix, shadowMatrix);

            UnityEngine.Rendering.RenderTextureSubElement rtSubElement = shadowUseDefaultRTFormat ? UnityEngine.Rendering.RenderTextureSubElement.Depth : UnityEngine.Rendering.RenderTextureSubElement.Default;
            fogMat.SetTexture(ShaderParams.ShadowTexture, cam.targetTexture, rtSubElement);
            fogMat.SetTexture(ShaderParams.TranslucencyTexture, translucentMap);
            if (enableDustParticles && particleMaterial != null) {
                particleMaterial.SetMatrix(ShaderParams.ShadowMatrix, shadowMatrix);
                particleMaterial.SetTexture(ShaderParams.ShadowTexture, cam.targetTexture, rtSubElement);
                particleMaterial.SetVector(ShaderParams.ShadowIntensity, new Vector4(shadowIntensity, 1f - shadowIntensity, 0, 0));
                particleMaterial.SetTexture(ShaderParams.TranslucencyTexture, translucentMap);
            }
        }


        void ShadowsUpdate() {

            bool usesCookie = cookieTexture != null && lightComp.type == LightType.Spot;
            if (!enableShadows && !usesCookie) return;

            if (cam == null) return;

            int frameCount = Time.frameCount;
            bool safeTime = frameCount - camStartFrameCount > 5;
            if (!meshRenderer.isVisible && safeTime) {
                if (cam.enabled) {
                    ShadowsDisable();
                }
                return;
            }

            Transform camTransform = cam.transform;
            cam.farClipPlane = generatedRange;
            if (generatedType == LightType.Point) {
                if (shadowBakeMode != ShadowBakeMode.HalfSphere) {
                } else if (shadowOrientation == ShadowOrientation.ToCamera) {
                    if (enableShadows && mainCamera != null) {
                        // if it's a point light, check if the orientation is target camera and if the angle has changed too much force a shadow update
                        if (shadowBakeInterval != ShadowBakeInterval.EveryFrame) {
                            if (Vector3.Angle(camTransform.forward, mainCamera.position - lastCamPos) > 45) {
                                shouldOrientToCamera = true;
                                ScheduleShadowCapture();
                            }
                        }
                        if (shouldOrientToCamera || shadowBakeInterval == ShadowBakeInterval.EveryFrame) {
                            shouldOrientToCamera = false;
                            camTransform.LookAt(mainCamera.position);
                        }
                    }
                } else {
                    camTransform.forward = shadowDirection;
                }
            }

            camTransformChanged = false;
            if (lastCamPos != camTransform.position || lastCamRot != camTransform.rotation) {
                camTransformChanged = true;
                lastCamPos = camTransform.position;
                lastCamRot = camTransform.rotation;
            }

            if (enableShadows) {
                ShadowCamUpdate();
            }

            if (camTransformChanged || usesCookie || cam.enabled) {
                SetupShadowMatrix();
            }

        }

        void ShadowCamUpdate() {
            if (shadowAutoToggle) {
                float maxDistSqr = shadowDistanceDeactivation * shadowDistanceDeactivation;
                if (distanceToCameraSqr > maxDistSqr) {
                    if (cam.enabled) {
                        ShadowsDisable();
                        if (fogMat.IsKeywordEnabled(ShaderParams.SKW_SHADOWS)) {
                            fogMat.DisableKeyword(ShaderParams.SKW_SHADOWS);
                        }
                        if (fogMat.IsKeywordEnabled(ShaderParams.SKW_SHADOWS_TRANSLUCENCY)) {
                            fogMat.DisableKeyword(ShaderParams.SKW_SHADOWS_TRANSLUCENCY);
                        }
                        if (fogMat.IsKeywordEnabled(ShaderParams.SKW_SHADOWS_CUBEMAP)) {
                            fogMat.DisableKeyword(ShaderParams.SKW_SHADOWS_CUBEMAP);
                        }
                    }
                    return;
                }
            }

            if (shadowBakeInterval == ShadowBakeInterval.OnStart) {
                if (!cam.enabled && camTransformChanged) {
                    ScheduleShadowCapture();
                } else if (Application.isPlaying && Time.frameCount > camStartFrameCount + 1) {
                    cam.enabled = false;
                }
            } else if (!cam.enabled) {
                ScheduleShadowCapture();
            }
        }

        void ComputeShadowTransform(Matrix4x4 proj, Matrix4x4 view) {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (usesReversedZBuffer) {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            // Apply texture scale and offset to save a MAD in shader.
            shadowMatrix = textureScaleAndBias * worldToShadow;
        }


        bool needsTranslucencyTextureCleanUp;

        void CaptureTransparents() {

            int renderersCount = VolumetricLightsTranslucency.objects.Count;
            if (renderersCount == 0) {
                if (!needsTranslucencyTextureCleanUp) return;
                needsTranslucencyTextureCleanUp = false;
            } else {
                needsTranslucencyTextureCleanUp = true;
            }

            if (transpOverrideMat == null) {
                Shader transpShader = Shader.Find(m_TranspShader);
                transpOverrideMat = new Material(transpShader);
            }

            if (transpDepthOnlyMat == null) {
                Shader transpDepthOnlyShader = Shader.Find(m_TranspDepthOnlyShader);
                transpDepthOnlyMat = new Material(transpDepthOnlyShader);
            }

            if (transpOverrideMaterials == null || transpOverrideMaterials.Length < renderersCount) {
                transpOverrideMaterials = new Material[renderersCount];
            }


            if (cmdTransp == null) {
                cmdTransp = new CommandBuffer();
            } else {
                cmdTransp.Clear();
            }

            RenderTexture active = RenderTexture.active;

            cmdTransp.SetRenderTarget(translucentMap);
            cmdTransp.ClearRenderTarget(false, true, Color.white);
            cmdTransp.SetViewProjectionMatrices(cam.worldToCameraMatrix, cam.projectionMatrix);
            if (frustumPlanes == null || frustumPlanes.Length != 6) {
                frustumPlanes = new Plane[6];
            }
            GeometryUtility.CalculateFrustumPlanes(cam, frustumPlanes);

            float intensity = 10f * shadowTranslucencyIntensity / (brightness + 0.0001f);
            for (int k = 0; k < renderersCount; k++) {
                VolumetricLightsTranslucency transpObject = VolumetricLightsTranslucency.objects[k];
                if (transpObject == null) continue;
                if (transpObject.intensityMultiplier <= 0) continue;
                if (transpObject.theRenderer == null || !GeometryUtility.TestPlanesAABB(frustumPlanes, transpObject.theRenderer.bounds)) continue;

                Material mat = transpObject.theRenderer.sharedMaterial;
                if (mat == null) continue;

                if (transpObject.preserveOriginalShader) {
                    cmdTransp.DrawRenderer(transpObject.theRenderer, mat);
                    cmdTransp.DrawRenderer(transpObject.theRenderer, transpDepthOnlyMat);
                } else {
                    if (!mat.HasProperty(ShaderParams.MainTex)) continue;
                    if (transpOverrideMaterials[k] == null) {
                        transpOverrideMaterials[k] = Instantiate(transpOverrideMat);
                    }
                    Material overrideMaterial = transpOverrideMaterials[k];
                    overrideMaterial.SetTexture(ShaderParams.MainTex, mat.GetTexture(ShaderParams.MainTex));
                    if (mat.HasProperty(ShaderParams.Color)) {
                        overrideMaterial.SetColor(ShaderParams.Color, mat.GetColor(ShaderParams.Color));
                    }
                    overrideMaterial.SetVector(ShaderParams.TranslucencyIntensity, new Vector4(intensity * transpObject.intensityMultiplier, shadowTranslucencyBlend, 0, 0));
                    Vector2 textureScale = mat.GetTextureScale(ShaderParams.MainTex);
                    Vector2 textureOffset = mat.GetTextureOffset(ShaderParams.MainTex);
                    overrideMaterial.SetVector(ShaderParams.MainTex_ST, new Vector4(textureScale.x, textureScale.y, textureOffset.x, textureOffset.y));
                    cmdTransp.DrawRenderer(transpObject.theRenderer, overrideMaterial);
                }
            }
            Graphics.ExecuteCommandBuffer(cmdTransp);
            RenderTexture.active = active;

        }

        #endregion

    }


}
