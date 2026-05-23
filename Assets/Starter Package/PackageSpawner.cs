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

public class PackageSpawner : MonoBehaviour
{
    public DrivingSurfaceManager DrivingSurfaceManager;
    public PackageBehaviour Package;
    public GameObject PackagePrefab;
    public float SpawnDistanceFromReference = 0.4f;
    public float SurfaceOffset = 0.06f;

    public static Vector3 RandomInTriangle(Vector3 v1, Vector3 v2)
    {
        float u = Random.Range(0.0f, 1.0f);
        float v = Random.Range(0.0f, 1.0f);
        if (v + u > 1)
        {
            v = 1 - v;
            u = 1 - u;
        }

        return (v1 * u) + (v2 * v);
    }

    public static Vector3 FindRandomLocation(ARPlane plane)
    {
        var mesh = plane.GetComponent<ARPlaneMeshVisualizer>().mesh;
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        if (triangles.Length < 3 || vertices.Length == 0)
        {
            return plane.transform.TransformPoint(plane.center);
        }

        var triangle = Random.Range(0, triangles.Length / 3) * 3;
        var v0 = vertices[triangles[triangle]];
        var v1 = vertices[triangles[triangle + 1]];
        var v2 = vertices[triangles[triangle + 2]];
        var randomInTriangle = v0 + RandomInTriangle(v1 - v0, v2 - v0);
        var randomPoint = plane.transform.TransformPoint(randomInTriangle);

        return randomPoint;
    }

    public void SpawnPackage(ARPlane plane)
    {
        SpawnPackage(plane, null);
    }

    public void SpawnPackage(ARPlane plane, Transform referenceTransform)
    {
        if (plane == null || PackagePrefab == null)
        {
            return;
        }

        var packageClone = Instantiate(PackagePrefab);
        var spawnPosition = referenceTransform == null
            ? FindRandomLocation(plane)
            : FindLocationNearReference(plane, referenceTransform);
        packageClone.transform.position = AlignToPlaneSurface(spawnPosition, plane);

        Package = packageClone.GetComponent<PackageBehaviour>();

        if (Package == null)
        {
            Debug.LogError("[AR Driving] PackagePrefab is missing a PackageBehaviour component.", packageClone);
            Destroy(packageClone);
            return;
        }

        Debug.Log("[AR Driving] Package spawned on the locked AR plane.", Package);
    }

    private Vector3 FindLocationNearReference(ARPlane plane, Transform referenceTransform)
    {
        var right = Camera.main != null ? Camera.main.transform.right : referenceTransform.right;
        var offset = Vector3.ProjectOnPlane(right, plane.transform.up).normalized * SpawnDistanceFromReference;
        return referenceTransform.position + offset;
    }

    private Vector3 AlignToPlaneSurface(Vector3 position, ARPlane plane)
    {
        var planeNormal = plane.transform.up;
        var planeCenter = plane.transform.TransformPoint(plane.center);
        var surface = new Plane(planeNormal, planeCenter);
        return position - (planeNormal * surface.GetDistanceToPoint(position)) + (planeNormal * SurfaceOffset);
    }

    private void Update()
    {
        if (DrivingSurfaceManager == null)
        {
            return;
        }

        var lockedPlane = DrivingSurfaceManager.LockedPlane;
        if (lockedPlane != null)
        {
            if (Package == null)
            {
                SpawnPackage(lockedPlane);
            }

            if (Package == null)
            {
                return;
            }

            Package.transform.position = AlignToPlaneSurface(Package.transform.position, lockedPlane);
        }
    }
}
