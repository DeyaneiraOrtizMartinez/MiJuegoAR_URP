/*
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{
    public ARCameraManager ARCameraManager;
    public Light Light;

    private void Awake()
    {
        Light = GetComponent<Light>();

        if (ARCameraManager == null && Camera.main != null)
        {
            ARCameraManager = Camera.main.GetComponent<ARCameraManager>();
        }
    }

    private void OnEnable()
    {
        if (ARCameraManager != null)
        {
            ARCameraManager.frameReceived += FrameReceived;
        }
        else
        {
            Debug.LogWarning("[AR Lighting] No ARCameraManager assigned for light estimation.", this);
        }
    }

    private void OnDisable()
    {
        if (ARCameraManager != null)
        {
            ARCameraManager.frameReceived -= FrameReceived;
        }
    }

    private void FrameReceived(ARCameraFrameEventArgs args)
    {
        var lightEstimation = args.lightEstimation;

        if (lightEstimation.averageBrightness.HasValue)
        {
            Light.intensity = lightEstimation.averageBrightness.Value;
        }

        if (lightEstimation.averageColorTemperature.HasValue)
        {
            Light.colorTemperature = lightEstimation.averageColorTemperature.Value;
        }

        if (lightEstimation.colorCorrection.HasValue)
        {
            Light.color = lightEstimation.colorCorrection.Value;
        }

        if (lightEstimation.mainLightDirection.HasValue)
        {
            Light.transform.rotation = Quaternion.LookRotation(lightEstimation.mainLightDirection.Value);
        }

        if (lightEstimation.mainLightColor.HasValue)
        {
            Light.color = lightEstimation.mainLightColor.Value;
        }

        if (lightEstimation.mainLightIntensityLumens.HasValue && lightEstimation.averageMainLightBrightness.HasValue)
        {
            Light.intensity = lightEstimation.averageMainLightBrightness.Value;
        }

        if (lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = lightEstimation.ambientSphericalHarmonics.Value;
        }
    }
}
