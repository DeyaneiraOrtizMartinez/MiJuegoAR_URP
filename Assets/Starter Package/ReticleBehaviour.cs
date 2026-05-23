/*
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReticleBehaviour : MonoBehaviour
{
    public GameObject Child;
    public DrivingSurfaceManager DrivingSurfaceManager;
    public ARPlane CurrentPlane;

    // Start is called before the first frame update
    private void Start()
    {
        if (transform.childCount > 0)
        {
            Child = transform.GetChild(0).gameObject;
        }
    }

    private void Update()
    {
        // Validación de seguridad para evitar NullReferenceException en Unity 6
        if (DrivingSurfaceManager == null || DrivingSurfaceManager.RaycastManager == null || Camera.main == null)
        {
            return;
        }

        // Determina el centro de la pantalla
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        
        // Realiza la prueba de impacto (Raycast)
        DrivingSurfaceManager.RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

        CurrentPlane = null;
        ARRaycastHit? hit = null;

        if (hits.Count > 0)
        {
            // If you don't have a locked plane already...
            var lockedPlane = DrivingSurfaceManager.LockedPlane;
            hit = lockedPlane == null
                // ... use the first hit in `hits`.
                ? hits[0]
                // Otherwise use the locked plane, if it's there.
                : hits.SingleOrDefault(x => x.trackableId == lockedPlane.trackableId);
        }

        if (hit.HasValue)
        {
            CurrentPlane = DrivingSurfaceManager.PlaneManager.GetPlane(hit.Value.trackableId);
            
            // Mueve la retícula sumando un pequeño desfase de 2 centímetros (0.02f) hacia arriba 
            // en el eje Y para evitar que se superponga o se hunda en el suelo arenoso.
            transform.position = hit.Value.pose.position + new Vector3(0f, 0.02f, 0f);
        }

        if (Child != null)
        {
            Child.SetActive(CurrentPlane != null);
        }
    }
}