using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Ogxd {

    public class BlenderImporter : AssetPostprocessor {

        private const bool INVERT_HANDEDNESS = true;
        private const bool SWITCH_UP_AXIS = true;

        private void OnPostprocessModel(GameObject gameObject) {

            if (!assetPath.EndsWith(".blend"))
                return;

            gameObject.transform.localScale = Vector3.one;
            List<GameObject> gameObjects = gameObject.GetChildrenRecursive();
            gameObjects.Add(gameObject);

            // Transform positions
            foreach (GameObject current in gameObjects) {
                var matrix = TransformExtensions.GetLocalMatrix(current.transform, false, false, 1f);
                TransformExtensions.SetFromLocalMatrix(current.transform, matrix, INVERT_HANDEDNESS, SWITCH_UP_AXIS, 1f);
            }

            // Transform meshes
            HashSet<Mesh> meshes = new HashSet<Mesh>();
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            for (int mf = 0; mf < meshFilters.Length; mf++) {
                meshes.Add(meshFilters[mf].sharedMesh);
            }
            SkinnedMeshRenderer[] skmrndrs = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int mf = 0; mf < skmrndrs.Length; mf++) {
                meshes.Add(skmrndrs[mf].sharedMesh);
            }
            foreach (Mesh mesh in meshes) {
                TransformMesh(mesh);
            }
        }

        private void TransformMesh(Mesh mesh) {

            if (!mesh)
                return;

            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = TransformExtensions.TransformVector(vertices[i], INVERT_HANDEDNESS, SWITCH_UP_AXIS);
            }
            mesh.vertices = vertices;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++) {
                normals[i] = TransformExtensions.TransformVector(normals[i], INVERT_HANDEDNESS, SWITCH_UP_AXIS);
            }
            mesh.normals = normals;

            Vector4[] tangents = mesh.tangents;
            for (int i = 0; i < tangents.Length; i++) {
                tangents[i] = TransformExtensions.TransformVector(tangents[i], INVERT_HANDEDNESS, SWITCH_UP_AXIS);
            }
            mesh.tangents = tangents;

            // For animations
            Matrix4x4[] bindPoses = mesh.bindposes;
            for (int i = 0; i < bindPoses.Length; i++) {
                bindPoses[i] = TransformExtensions.TransformMatrix(bindPoses[i], INVERT_HANDEDNESS, SWITCH_UP_AXIS, 1f);
            }
            mesh.bindposes = bindPoses;

            mesh.RecalculateBounds();
        }
    }
}