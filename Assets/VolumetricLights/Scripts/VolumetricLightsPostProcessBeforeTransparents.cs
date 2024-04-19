//------------------------------------------------------------------------------------------------------------------
// Volumetric Lights
// Created by Kronnect
//------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricLights {

    public class VolumetricLightsPostProcessBeforeTransparents : VolumetricLightsPostProcessBase {

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            AddRenderPasses(source, destination);
        }
    }
}

