using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo_ProceduralPlacement
{
	[ExecuteInEditMode]
	public class InstancePlacer : MonoBehaviour
	{
		const int maxResolution = 1000;

		static readonly int
			positionsId = Shader.PropertyToID("_Positions"),
			positionsId2 = Shader.PropertyToID("_Positions2"),
			resolutionId = Shader.PropertyToID("_Resolution"),
			densityId = Shader.PropertyToID("_Density"),
			densityId2 = Shader.PropertyToID("_Density2"),
			tilingId = Shader.PropertyToID("_Tiling"),
			octaveId = Shader.PropertyToID("_Octaves"),
			growValId = Shader.PropertyToID("_GrowValue"),
			growValId2 = Shader.PropertyToID("_GrowValue2"),
			randomFactorId = Shader.PropertyToID("_RandomFactor");

		[SerializeField]
		ComputeShader computeShader;

		[SerializeField]
		Material instanceAMaterial;

		[SerializeField]
		Mesh instanceAMesh;

		[SerializeField]
		Material instanceBMaterial;

		[SerializeField]
		Mesh instanceBMesh;

		public GameObject instanceBCollider;
		public GameObject collisionParent;

		[SerializeField, Range(10, maxResolution)]
		int resolution = 10;

		[SerializeField, Range(0.1f, 10)]
		float density = 1;

		[SerializeField, Range(0, 1)]
		float randomFactor = 0;

		public float tiling = 0.1f;
		public float octaves = 5;
		public float grow = 1;
		public float grow2 = 1;

		ComputeBuffer positionsBufferA;
		ComputeBuffer positionsBufferB;

		void OnEnable()
		{
			positionsBufferA = new ComputeBuffer(resolution * resolution, 3 * 4);
			positionsBufferB = new ComputeBuffer(resolution * resolution, 3 * 4);
		}

		void OnDisable()
		{
			positionsBufferA.Release();
			positionsBufferA = null;
			positionsBufferB.Release();
			positionsBufferB = null;
		}

		void Update()
		{
			UpdateFunctionOnGPU();
		}

		bool done = false;

		public void UpdateFunctionOnGPU()
		{
			computeShader.SetInt(resolutionId, resolution);
			computeShader.SetFloat(densityId, density);
			computeShader.SetFloat(randomFactorId, randomFactor);
			computeShader.SetFloat(tilingId, tiling);
			computeShader.SetFloat(octaveId, octaves);
			computeShader.SetFloat(growValId, grow);
			computeShader.SetFloat(growValId2, grow2);

			var kernelIndex = computeShader.FindKernel("CSMain");
			computeShader.SetBuffer(kernelIndex, positionsId, positionsBufferA);
			computeShader.SetBuffer(kernelIndex, positionsId2, positionsBufferB);

			int groups = Mathf.FloorToInt(resolution / 8f); // ceilToInt got some issues, use floor for now.
			computeShader.Dispatch(kernelIndex, groups, groups, 1);

			// async request positionbuffer2, the result will be returned after few frames
			UnityEngine.Rendering.AsyncGPUReadback.Request(positionsBufferB, r =>
			{

				// checking hasError and if it's done
				// make sure it's one time execution.
				if (r.hasError == false && !done)
				{
					var data = r.GetData<Vector3>();

					// loop entire data TODO: see if there is way to reduce the range
					for (int i = 0; i < data.Length; i++)
					{
						Vector3 pos = (Vector3)data[i];
						// set condition to only spawn what's needed
						Debug.Log(Vector3.Distance(Vector3.zero, data[i]));
						if (Vector3.Distance(Vector3.zero, data[i]) < 3 )//&& pos != Vector3.zero)
						{
							pos.y *= 10;
							GameObject col = Instantiate(instanceBCollider, pos, Quaternion.identity);
							col.transform.parent = collisionParent.transform;
						}
					}
					Debug.Log(collisionParent.transform.childCount); // check actual spawned objs.
					done = true;
				}
			});

			instanceAMaterial.SetBuffer(positionsId, positionsBufferA);
			instanceBMaterial.SetBuffer(positionsId, positionsBufferB);
			var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));

			Graphics.DrawMeshInstancedProcedural(
				instanceAMesh, 0, instanceAMaterial, bounds, resolution * resolution
			);
			Graphics.DrawMeshInstancedProcedural(
				instanceBMesh, 0, instanceBMaterial, bounds, resolution * resolution
			);
		}
	}
}