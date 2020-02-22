using UnityEngine;

namespace Ogxd {

    public static class TransformExtensions
    {
        private static void GetTRS(this Matrix4x4 matrix, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            float det = matrix.GetDeterminant();

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

        private static float GetDeterminant(this Matrix4x4 matrix)
        {
            return matrix.m00 * (matrix.m11 * matrix.m22 - matrix.m12 * matrix.m21) -
                   matrix.m10 * (matrix.m01 * matrix.m22 - matrix.m02 * matrix.m21) +
                   matrix.m20 * (matrix.m01 * matrix.m12 - matrix.m02 * matrix.m11);
        }

        public static void SetFromLocalMatrix(this Transform transform, Matrix4x4 matrix, bool invertHandedness, bool switchUpAxis, float scaleFactor)
        {
            matrix.GetTRS(out Vector3 t, out Quaternion r, out Vector3 s);

            // Apply scale factor
            t *= scaleFactor;

            // If matrix is from Z-up system, we convert the TRS to match Unity's Y-up system
            if (switchUpAxis) {
                t = new Vector3(t.x, t.z, t.y);
                r = new Quaternion(r.x, r.z, r.y, -r.w);
                s = new Vector3(s.x, s.z, s.y);
            }

            // If matrix is from Left-Handed system, we convert the TRS to match Unity's Right-Handed system
            if (invertHandedness) {
                t = new Vector3(-t.x, t.y, t.z);
                r = new Quaternion(-r.x, r.y, r.z, -r.w);
            }

            // Assign local TRS to transform
            transform.localPosition = t;
            transform.localRotation = r;
            transform.localScale = s;
        }

        public static Matrix4x4 GetLocalMatrix(this Transform transform, bool invertHandedness, bool switchUpAxis, float scaleFactor)
        {
            Vector3 t = transform.localPosition;
            Quaternion r = transform.localRotation;
            Vector3 s = transform.localScale;

            t *= scaleFactor;

            // If matrix should is for Z-up system, we convert the TRS from Unity's Y-up system
            if (switchUpAxis) {
                t = new Vector3(t.x, t.z, t.y);
                r = new Quaternion(r.x, r.z, r.y, -r.w);
                s = new Vector3(s.x, s.z, s.y);
            }

            // If matrix should is for Left-Handed system, we convert the TRS from Unity's Right-Handed system
            if (invertHandedness) {
                t = new Vector3(-t.x, t.y, t.z);
                r = new Quaternion(-r.x, r.y, r.z, -r.w);
            }

            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(t, r, s);

            return matrix;
        }

        public static Matrix4x4 GetWorldMatrix(this Transform transform, bool invertHandedness, bool switchUpAxis, float scaleFactor)
        {
            Vector3 t = transform.position;
            Quaternion r = transform.rotation;
            Vector3 s = transform.lossyScale;

            t *= scaleFactor;

            // If matrix should is for Z-up system, we convert the TRS from Unity's Y-up system
            if (switchUpAxis) {
                t = new Vector3(t.x, t.z, t.y);
                r = new Quaternion(r.x, r.z, r.y, -r.w);
                s = new Vector3(s.x, s.z, s.y);
            }

            // If matrix should is for Left-Handed system, we convert the TRS from Unity's Right-Handed system
            if (invertHandedness) {
                t = new Vector3(-t.x, t.y, t.z);
                r = new Quaternion(-r.x, r.y, r.z, -r.w);
            }

            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetTRS(t, r, s);

            return matrix;
        }



    }
}