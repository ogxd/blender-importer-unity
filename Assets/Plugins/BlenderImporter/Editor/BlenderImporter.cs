using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Ogxd {

    public enum AXIS
    {
        X = 0,
        Y = 1,
        Z = 2,
    }

    public enum DIRECTION
    {
        Positive = 1,
        Negative = -1,
    }

    public class BlenderImporter : AssetPostprocessor {

        private AXIS rightAxis;
        private DIRECTION rightDirection;

        private AXIS upAxis;
        private DIRECTION upDirection;

        private AXIS forwardAxis;
        private DIRECTION forwardDirection;

        private int inversion;

        private void OnPostprocessModel(GameObject gameObject) {

            return;

            switch (Path.GetExtension(assetPath).ToLower()) {
                //case ".fbx":
                case ".blend":
                    rightAxis = AXIS.X;
                    rightDirection = DIRECTION.Positive;
                    upAxis = AXIS.Y;
                    upDirection = DIRECTION.Positive;
                    forwardAxis = AXIS.Z;
                    forwardDirection = DIRECTION.Positive;
                    break;
                default:
                    return;
            }

            if (upAxis == forwardAxis || upAxis == rightAxis || rightAxis == forwardAxis) {
                throw new System.Exception("Forward, Up, and Right axis must be different !");
            }

            inversion = (AreAllAxisOrdered() ? 1 : -1) * (int)rightDirection * (int)upDirection * (int)forwardDirection;

            gameObject.transform.localScale = Vector3.one;
            List<GameObject> gameObjects = gameObject.GetChildrenRecursive();
            gameObjects.Add(gameObject);

            // Transform positions
            foreach (GameObject current in gameObjects) {
                TransformTransform(current.transform);
            }

            // Transform meshes
            var meshes = new HashSet<Mesh>();
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

        private bool AreAxisOrdered(AXIS a, AXIS b)
        {
            if (a == AXIS.Z)
                return b == AXIS.X;
            return (int)b == (int)a + 1;
        }

        private bool AreAllAxisOrdered()
        {
            return AreAxisOrdered(upAxis, forwardAxis) &&
                   AreAxisOrdered(forwardAxis, rightAxis) &&
                   AreAxisOrdered(rightAxis, upAxis);
        }

        private void TransformMesh(Mesh mesh) {

            if (!mesh)
                return;

            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = TransformVector3(vertices[i]);
            }
            mesh.vertices = vertices;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++) {
                normals[i] = TransformVector3(normals[i]);
            }
            mesh.normals = normals;

            Vector4[] tangents = mesh.tangents;
            for (int i = 0; i < tangents.Length; i++) {
                tangents[i] = TransformVector4(tangents[i]);
            }
            mesh.tangents = tangents;

            // For animations
            Matrix4x4[] bindPoses = mesh.bindposes;
            for (int i = 0; i < bindPoses.Length; i++) {
                bindPoses[i] = TransformMatrix(bindPoses[i]);
            }
            mesh.bindposes = bindPoses;

            if (inversion == -1) {
                int[] indices = mesh.triangles;
                for (int i = 0; i < indices.Length; i+=3) {
                    int swap = indices[i + 1];
                    indices[i] = indices[i];
                    indices[i + 1] = indices[i + 2];
                    indices[i + 2] = swap;
                }
                mesh.triangles = indices;
            }

            mesh.RecalculateBounds();
        }

        private Vector3 TransformVector3(Vector3 vector)
        {
            return new Vector3((int)rightDirection * vector[(int)rightAxis], (int)upDirection * vector[(int)upAxis], (int)forwardDirection * vector[(int)forwardAxis]);
        }

        private Vector4 TransformVector4(Vector4 vector)
        {
            return new Vector4((int)rightDirection * vector[(int)rightAxis], (int)upDirection * vector[(int)upAxis], (int)forwardDirection * vector[(int)forwardAxis], vector.w);
        }

        private void TransformTransform(Transform transform)
        {
            GetTRS(transform.localToWorldMatrix, out Vector3 t, out Quaternion r, out Vector3 s);

            TransformTRS(ref t, ref r, ref s);

            // Assign TRS to transform
            transform.localPosition = t;
            transform.localRotation = r;
            transform.localScale = s;
        }

        private Matrix4x4 TransformMatrix(Matrix4x4 matrix)
        {
            GetTRS(matrix, out Vector3 t, out Quaternion r, out Vector3 s);

            TransformTRS(ref t, ref r, ref s);

            matrix = Matrix4x4.identity;
            matrix.SetTRS(t, r, s);

            return matrix;
        }

        private void TransformTRS(ref Vector3 t, ref Quaternion r, ref Vector3 s)
        {
            t = new Vector3((int)rightDirection * t[(int)rightAxis], (int)upDirection * t[(int)upAxis], (int)forwardDirection * t[(int)forwardAxis]);
            r = new Quaternion((int)rightDirection * r[(int)rightAxis], (int)upDirection * r[(int)upAxis], (int)forwardDirection * r[(int)forwardAxis], inversion * r.w);
            s = new Vector3(s[(int)rightAxis], s[(int)upAxis], s[(int)forwardAxis]);
        }

        private float GetDeterminant(Matrix4x4 matrix)
        {
            return matrix.m00 * (matrix.m11 * matrix.m22 - matrix.m12 * matrix.m21) -
                   matrix.m10 * (matrix.m01 * matrix.m22 - matrix.m02 * matrix.m21) +
                   matrix.m20 * (matrix.m01 * matrix.m12 - matrix.m02 * matrix.m11);
        }

        private void GetTRS(Matrix4x4 matrix, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            float det = GetDeterminant(matrix);

            // Scale
            scale.x = matrix.MultiplyVector(new Vector3(1, 0, 0)).magnitude;
            scale.y = matrix.MultiplyVector(new Vector3(0, 1, 0)).magnitude;
            scale.z = matrix.MultiplyVector(new Vector3(0, 0, 1)).magnitude;
            scale = (det < 0) ? -scale : scale;

            // Rotation
            Matrix4x4 rotationMatrix = matrix;
            rotationMatrix.m03 = rotationMatrix.m13 = rotationMatrix.m23 = 0f;
            rotationMatrix = rotationMatrix * new Matrix4x4 { m00 = 1f / scale.x, m11 = 1f / scale.y, m22 = 1f / scale.z, m33 = 1 };
            rotation = Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));

            // Position
            translation = matrix.GetColumn(3);
        }
    }
}