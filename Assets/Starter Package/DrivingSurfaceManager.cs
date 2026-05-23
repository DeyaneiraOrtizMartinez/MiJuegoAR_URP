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
using UnityEngine.XR.ARFoundation;

public class DrivingSurfaceManager : MonoBehaviour
{
    public ARPlaneManager PlaneManager;
    public ARRaycastManager RaycastManager;
    public ARPlane LockedPlane;

    [Tooltip("When enabled, only the locked driving plane remains visible after placing the car.")]
    public bool HideOtherPlanesAfterLock;

    public void LockPlane(ARPlane keepPlane)
    {
        if (keepPlane == null)
        {
            return;
        }

        EnsureManagers();

        if (PlaneManager == null)
        {
            return;
        }

        var arPlane = keepPlane.GetComponent<ARPlane>();
        LockedPlane = arPlane;

        if (!HideOtherPlanesAfterLock)
        {
            return;
        }

        foreach (var plane in PlaneManager.trackables)
        {
            plane.gameObject.SetActive(plane == arPlane);
        }

        PlaneManager.trackablesChanged.AddListener(DisableNewPlanes);
    }

    private void Awake()
    {
        EnsureManagers();
    }

    private void OnDisable()
    {
        if (PlaneManager != null)
        {
            PlaneManager.trackablesChanged.RemoveListener(DisableNewPlanes);
        }
    }

    private void Update()
    {
        if (LockedPlane?.subsumedBy != null)
        {
            LockedPlane = LockedPlane.subsumedBy;
        }
    }

    private void EnsureManagers()
    {
        if (PlaneManager == null)
        {
            PlaneManager = GetComponent<ARPlaneManager>();
        }

        if (RaycastManager == null)
        {
            RaycastManager = GetComponent<ARRaycastManager>();
        }
    }

    private void DisableNewPlanes(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var plane in args.added)
        {
            plane.gameObject.SetActive(false);
        }
    }
}
