using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo_ProceduralPlacement
{
	[ExecuteInEditMode]
	public class InstancePlacer : MonoBehaviour
	{
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

		[Header("___ Instance A/B material and mesh ___")]

		[SerializeField]
		Material instanceAMaterial;

		[SerializeField]
		Mesh instanceAMesh;

		[SerializeField]
		Material instanceBMaterial;

		[SerializeField]
		Mesh instanceBMesh;

		[Header("___ Collision for Instance B ___")]
		public GameObject instanceBCollider;
		public GameObject collisionParent;
		public GameObject player;

		int resolution = 10;

		[Header("___ Density map choice ___")]
		public DensityChoices densityChoice;

		public enum DensityChoices{
			DitherConstant,
			DitherAlongX,
			DitherBasedOnHeight,
			DitherLayered
		}

		int densityChoiceId;

		[SerializeField, Range(0f, 1f)]
		float ditherConstant = 1;

		public float instanceALayeredDitherFactor = 1;
		public float instanceBLayeredDitherFactor = 1;

		ComputeBuffer positionsBufferA;
		ComputeBuffer positionsBufferB;

		[Header("___ general instance control ___")]
		[SerializeField, Range(0.1f, 0.2f)]
		float spacing = 0.165f;

		[SerializeField, Range(0, 1)]
		float randomFactor = 0;

		public bool useHeightBasedScale;
		public bool invertScale;
		Material terrainMaterial;

		void OnValidate(){
			if (densityChoice == DensityChoices.DitherConstant) {
				densityChoiceId = 0;
			} else if (densityChoice == DensityChoices.DitherAlongX) {
				densityChoiceId = 1;
			} else if (densityChoice == DensityChoices.DitherBasedOnHeight) {
				densityChoiceId = 2;
			} else {
				densityChoiceId = 3;
			}
		}

		void OnEnable()
		{
			int numOfGroups = 8;
			int numOfThreads = 16;
			resolution = numOfGroups * numOfThreads;
			positionsBufferA = new ComputeBuffer(resolution * resolution, 3 * 4);
			positionsBufferB = new ComputeBuffer(resolution * resolution, 3 * 4);

			GameObject terrainObj = GameObject.Find("highResPlane");
			terrainMaterial = terrainObj.GetComponent<MeshRenderer>().sharedMaterial;
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

			int numOfGroups = 8;
			int numOfThreads = 16;
			resolution = numOfGroups * numOfThreads;

			computeShader.SetInt(resolutionId, resolution);
			computeShader.SetFloat(densityId, ditherConstant);
			computeShader.SetFloat("_Spacing", spacing);
			computeShader.SetInt("_DensityChoice", densityChoiceId);
			computeShader.SetFloat(randomFactorId, randomFactor);
			computeShader.SetFloat(tilingId, terrainMaterial.GetFloat("_Tiling"));
			computeShader.SetFloat(octaveId, terrainMaterial.GetFloat("_Octaves"));
			computeShader.SetFloat("_DisplacementAmount", terrainMaterial.GetFloat("_DisplacementAmount"));
			computeShader.SetFloat(growValId, instanceALayeredDitherFactor);
			computeShader.SetFloat(growValId2, instanceBLayeredDitherFactor);

			var kernelIndex = computeShader.FindKernel("CSMain");
			computeShader.SetBuffer(kernelIndex, positionsId, positionsBufferA);
			computeShader.SetBuffer(kernelIndex, positionsId2, positionsBufferB);

			computeShader.Dispatch(kernelIndex, numOfGroups, numOfGroups, 1);

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
						// Debug.Log(Vector3.Distance(Vector3.zero, data[i]));
						if (Vector3.Distance(player.transform.position, data[i]) < 5 )//&& pos != Vector3.zero)
						{
							GameObject col = Instantiate(instanceBCollider, pos, Quaternion.identity);
							col.transform.parent = collisionParent.transform;
						}
					}
					// Debug.Log(collisionParent.transform.childCount); // check actual spawned objs.
					done = true;
				}
			});

			instanceAMaterial.SetBuffer(positionsId, positionsBufferA);
			instanceBMaterial.SetBuffer(positionsId, positionsBufferB);
			instanceAMaterial.SetFloat("_UseScale", (float)(useHeightBasedScale ? 1.0 : 0.0));
			instanceBMaterial.SetFloat("_UseScale", (float)(useHeightBasedScale ? 1.0 : 0.0));
			instanceAMaterial.SetFloat("_InvertScale", (float)(invertScale ? 1.0 : 0.0));
			instanceBMaterial.SetFloat("_InvertScale", (float)(invertScale ? 1.0 : 0.0));
			instanceAMaterial.SetFloat("_DisplacementAmount", terrainMaterial.GetFloat("_DisplacementAmount"));
			instanceBMaterial.SetFloat("_DisplacementAmount", terrainMaterial.GetFloat("_DisplacementAmount"));
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