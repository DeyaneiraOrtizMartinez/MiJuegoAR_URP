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
    public float SurfaceOffset = 0.02f;

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
        Package = packageClone.GetComponent<PackageBehaviour>();

        if (Package == null)
        {
            Debug.LogError("[AR Driving] PackagePrefab is missing a PackageBehaviour component.", packageClone);
            Destroy(packageClone);
            return;
        }

        PlacePackageOnPlane(Package, spawnPosition, plane);
        Debug.Log("[AR Driving] Package spawned on the locked AR plane.", Package);
    }

    private Vector3 FindLocationNearReference(ARPlane plane, Transform referenceTransform)
    {
        var right = Camera.main != null ? Camera.main.transform.right : referenceTransform.right;
        var offset = Vector3.ProjectOnPlane(right, plane.transform.up).normalized * SpawnDistanceFromReference;
        return referenceTransform.position + offset;
    }

    private void PlacePackageOnPlane(PackageBehaviour package, Vector3 targetPosition, ARPlane plane)
    {
        var planeNormal = plane.transform.up;
        var planeCenter = plane.transform.TransformPoint(plane.center);
        var surface = new Plane(planeNormal, planeCenter);
        var surfacePosition = targetPosition - (planeNormal * surface.GetDistanceToPoint(targetPosition));

        package.transform.position = surfacePosition;
        var lowestPoint = FindLowestRendererPoint(package.gameObject, planeNormal);
        var lift = -surface.GetDistanceToPoint(lowestPoint) + SurfaceOffset;
        package.transform.position += planeNormal * lift;
    }

    private Vector3 FindLowestRendererPoint(GameObject root, Vector3 planeNormal)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return root.transform.position;
        }

        var lowestPoint = renderers[0].bounds.center;
        var lowestDistance = Vector3.Dot(lowestPoint, planeNormal);

        foreach (var renderer in renderers)
        {
            var bounds = renderer.bounds;
            for (var x = -1; x <= 1; x += 2)
            {
                for (var y = -1; y <= 1; y += 2)
                {
                    for (var z = -1; z <= 1; z += 2)
                    {
                        var point = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                        var distance = Vector3.Dot(point, planeNormal);

                        if (distance < lowestDistance)
                        {
                            lowestDistance = distance;
                            lowestPoint = point;
                        }
                    }
                }
            }
        }

        return lowestPoint;
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

            PlacePackageOnPlane(Package, Package.transform.position, lockedPlane);
        }
    }
}
