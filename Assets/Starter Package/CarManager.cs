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

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * Spawns a <see cref="CarBehaviour"/> when a plane is tapped.
 */
public class CarManager : MonoBehaviour
{
    public GameObject CarPrefab;
    public ReticleBehaviour Reticle;
    public DrivingSurfaceManager DrivingSurfaceManager;
    public PackageSpawner PackageSpawner;

    public CarBehaviour Car;

    private void Update()
    {
        if (Car != null || !WasTapped() || Reticle == null || Reticle.CurrentPlane == null || CarPrefab == null)
        {
            return;
        }

        var obj = Instantiate(CarPrefab);
        Car = obj.GetComponent<CarBehaviour>();

        if (Car == null)
        {
            Debug.LogError("[AR Driving] CarPrefab is missing a CarBehaviour component.", obj);
            Destroy(obj);
            return;
        }

        Car.Reticle = Reticle;
        Car.transform.position = Reticle.transform.position;
        Debug.Log("[AR Driving] Car spawned on the current AR plane.", Car);

        if (DrivingSurfaceManager != null)
        {
            DrivingSurfaceManager.LockPlane(Reticle.CurrentPlane);
        }

        if (PackageSpawner != null)
        {
            PackageSpawner.SpawnPackage(Reticle.CurrentPlane, Car.transform);
        }
    }

    private bool WasTapped()
    {
#if ENABLE_INPUT_SYSTEM
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        if (Input.touchCount == 0)
        {
            return false;
        }

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
        {
            return false;
        }

        return true;
#else
        return false;
#endif
    }
}
