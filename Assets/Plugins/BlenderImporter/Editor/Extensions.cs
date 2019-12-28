using System.Collections.Generic;
using UnityEngine;

namespace Ogxd {

    public static class Extensions {

        public static Mesh GetMesh(this GameObject gameObject) {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter)
                return meshFilter.sharedMesh;
            else
                return null;
        }

        public static Material[] GetMaterials(this GameObject gameObject) {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer)
                return renderer.sharedMaterials;
            else
                return new Material[0];
        }

        public static List<GameObject> GetChildrenRecursive(this GameObject gameObject) {
            List<GameObject> gameObjects = new List<GameObject>();
            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(gameObject);
            while (queue.Count != 0) {
                GameObject current = queue.Dequeue();
                foreach (Transform transform in current.transform) {
                    queue.Enqueue(transform.gameObject);
                    gameObjects.Add(transform.gameObject);
                }
            }
            return gameObjects;
        }
    }
}