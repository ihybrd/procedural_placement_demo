using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo_ProceduralPlacement
{
    public class TerrainCollision : MonoBehaviour
    {
        public Shader shader;

        [Range(2, 256)]
        public int resolution = 10;

        [SerializeField, HideInInspector]
        MeshFilter meshFilter;
        TerrainNoise terrainFace;

        private void OnValidate()
        {
            Initialize();
            GenerateMesh();
            GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
        }

        void Initialize()
        {
            if (meshFilter == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(shader);
                meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();
            }

            terrainFace = new TerrainNoise(meshFilter.sharedMesh, resolution, Vector3.up);
        }

        void GenerateMesh()
        {
            terrainFace.ConstructMesh();
        }
    }
}