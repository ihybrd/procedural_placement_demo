// This shader fills the mesh shape with a color predefined in the code.
Shader "Demo/GpuTerrain"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _Tiling ("Tiling", Range(0.1, 50)) = 0.1
        _Octaves ("Octaves", Range(1, 10)) = 1
        _DisplacementAmount ("DisplacementAmount", Range(0.01, 20)) = 1
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _Tiling;
            float _Octaves;
            float _DisplacementAmount;

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "noise.hlsl"

            // vertex input structure
            struct Attributes
            {
                float4 positionOS   : POSITION;
	            float3 normal : NORMAL;
	            float2 uv : TEXCOORD0;
	            float4 color : COLOR;               
            };

            // vertex output && frag input
            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD2;
                float4 color : COLOR;
	            float3 normal : NORMAL;
	            float2 uv : TEXCOORD0;
            };

            float get_height(float2 pos) {
                float noise = 0;
                for (float i = 0; i < _Octaves; i ++) {
                    float a = pow(2, i);
                    noise += perlinNoise(pos, _Tiling * a)/a;
                }
                noise = (noise + 1)/2; // convert from -1,1 to 0,1
                return noise;
            }

            // https://www.youtube.com/watch?v=izsMr5Pyk2g
            float3 get_normal(float3 pos, float offset) {
                float2 pos1 = pos.xz + float2(offset, 0);
                float2 pos2 = pos.xz + float2(0, offset);
                float y1 = get_height(pos1)*10;
                float y2 = get_height(pos2)*10;

                return normalize(cross(float3(pos1.x, y1, pos1.y) - pos, float3(pos2.x, y2, pos2.y) - pos));
            }

            // vertex shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = mul (unity_ObjectToWorld, float4 (IN.positionOS.xyz, 1.0)).xyz;

                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                float noise = get_height(positionWS.xz);

                IN.positionOS.y += noise*_DisplacementAmount;

                OUT.positionWS = positionWS;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = noise;
		        OUT.normal = get_normal(IN.positionOS.xyz, 0.01);
		        OUT.uv = IN.uv;

                return OUT;
            }

            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                float d = IN.color;
                //max(dot(IN.normal, -normalize(_MainLightPosition.xyz)), 0) * noise;

                return float4(d, d, d, 1);
            }
            ENDHLSL
        }
    }
}