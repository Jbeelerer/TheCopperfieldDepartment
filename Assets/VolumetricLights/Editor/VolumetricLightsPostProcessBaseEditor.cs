using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace VolumetricLights {

    public class VolumetricLightsPostProcessBaseEditor : Editor {

        SerializedProperty blendMode, brightness, ditherStrength;
        SerializedProperty downscaling, blurPasses, blurDownscaling, blurSpread, blurHDR, blurEdgePreserve, blurEdgeDepthThreshold;

        private void OnEnable() {
            blendMode = serializedObject.FindProperty("blendMode");
            brightness = serializedObject.FindProperty("brightness");
            downscaling = serializedObject.FindProperty("downscaling");
            blurPasses = serializedObject.FindProperty("blurPasses");
            blurDownscaling = serializedObject.FindProperty("blurDownscaling");
            blurSpread = serializedObject.FindProperty("blurSpread");
            blurHDR = serializedObject.FindProperty("blurHDR");
            blurEdgePreserve = serializedObject.FindProperty("blurEdgePreserve");
            blurEdgeDepthThreshold = serializedObject.FindProperty("blurEdgeDepthThreshold");
            ditherStrength = serializedObject.FindProperty("ditherStrength");
        }

        public override void OnInspectorGUI() {

            if (VolumetricLightEditor.lastEditingLight != null) {
                if (GUILayout.Button("<< Back To Last Volumetric Light")) {
                    Selection.SetActiveObjectWithContext(VolumetricLightEditor.lastEditingLight, null);
                    GUIUtility.ExitGUI();
                }
            }

            serializedObject.Update();
            bool beforeTransparents = target is VolumetricLightsPostProcessBeforeTransparents;
            bool newSortOrder = EditorGUILayout.Toggle(new GUIContent("Render Before Transp"), beforeTransparents);
            if (newSortOrder != beforeTransparents) {
                SwitchComponent(newSortOrder);
                EditorGUIUtility.ExitGUI();
            }
            EditorGUILayout.PropertyField(downscaling);
            EditorGUILayout.PropertyField(blurPasses);
            if (blurPasses.intValue > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(blurDownscaling);
                EditorGUILayout.PropertyField(blurSpread);
                EditorGUILayout.PropertyField(blurHDR, new GUIContent("HDR"));
                EditorGUILayout.PropertyField(blurEdgePreserve, new GUIContent("Preserve Edges"));
                if (blurEdgePreserve.boolValue) {
                    EditorGUILayout.PropertyField(blurEdgeDepthThreshold, new GUIContent("Edge Threshold"));
                }
                EditorGUI.indentLevel--;
            }
            if (blurPasses.intValue == 0 && downscaling.floatValue <= 1f) {
                EditorGUILayout.HelpBox("No composition in effect (no downscaling and no blur applied).", MessageType.Info);
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(blendMode);
            EditorGUILayout.PropertyField(brightness);
            EditorGUILayout.PropertyField(ditherStrength);
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();

            VolumetricLightsPostProcessBase component = (VolumetricLightsPostProcessBase)target;
            component.enabled = downscaling.floatValue > 1 || blurPasses.intValue > 0;
        }

        void SwitchComponent(bool beforeTransparents) {
            float pdownscaling = downscaling.floatValue;
            int pblurPasses = blurPasses.intValue;
            float pblurDownscaling = blurDownscaling.floatValue;
            float pblurSpread = blurSpread.floatValue;
            bool pblurHDR = blurHDR.boolValue;
            bool pblurEdgePreserve = blurEdgePreserve.boolValue;
            float pblurEdgeDepthThredhold = blurEdgeDepthThreshold.floatValue;
            int pBlendMode = blendMode.intValue;
            float pBrightness = brightness.floatValue;
            float pDitherStrehgtn = ditherStrength.floatValue;
            VolumetricLightsPostProcessBase post = (VolumetricLightsPostProcessBase)target;
            GameObject o = post.gameObject;
            DestroyImmediate(post);
            if (beforeTransparents) {
                post = o.AddComponent<VolumetricLightsPostProcessBeforeTransparents>();
            } else {
                post = o.AddComponent<VolumetricLightsPostProcess>();
            }
            post.downscaling = pdownscaling;
            post.blurPasses = pblurPasses;
            post.blurDownscaling = pblurDownscaling;
            post.blurSpread = pblurSpread;
            post.blurHDR = pblurHDR;
            post.blurEdgePreserve = pblurEdgePreserve;
            post.blurEdgeDepthThreshold = pblurEdgeDepthThredhold;
            post.blendMode = (BlendMode)pBlendMode;
            post.brightness = pBrightness;
            post.ditherStrength = pDitherStrehgtn;
        }

    }
}