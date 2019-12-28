using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Ogxd {

    public class BlenderImporter : AssetPostprocessor {

        void OnPostprocessModel(GameObject gameObject) {

            if (!assetPath.EndsWith(".blend"))
                return;

            gameObject.transform.localScale = Vector3.one;
            List<GameObject> gameObjects = gameObject.GetChildrenRecursive();
            gameObjects.Add(gameObject);

            foreach (GameObject current in gameObjects) {
                if (current == gameObject)
                    current.transform.rotation = Quaternion.identity;
                else
                    current.transform.Rotate(Vector3.right, 90);
                Vector3 localPosition = current.transform.localPosition;
                current.transform.localPosition = new Vector3(-localPosition.x, localPosition.y, -localPosition.z);
            }

            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            HashSet<Mesh> meshes = new HashSet<Mesh>();
            for (int mf = 0; mf < meshFilters.Length; mf++) {
                meshes.Add(meshFilters[mf].sharedMesh);
            }

            foreach (Mesh mesh in meshes) {
                processMesh(mesh);
            }
        }

        private void processMesh(Mesh mesh) {

            if (!mesh)
                return;

            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = new Vector3(-vertices[i].x, vertices[i].z, vertices[i].y);
            }
            mesh.vertices = vertices;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++) {
                normals[i] = new Vector3(-normals[i].x, normals[i].z, normals[i].y);
            }
            mesh.normals = normals;

            Vector4[] tangents = mesh.tangents;
            for (int i = 0; i < tangents.Length; i++) {
                tangents[i] = new Vector4(-tangents[i].x, tangents[i].z, tangents[i].y, tangents[i].w);
            }
            mesh.tangents = tangents;

            mesh.RecalculateBounds();
        }
    }
}