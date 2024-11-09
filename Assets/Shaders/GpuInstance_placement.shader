Shader "Demo/GpuInstance_placement"
{
    Properties
    {
        _Tiling("_Tiling", Float) = 0.1
        _Octaves("_Octaves", Float) = 1
        _DisplacementAmount("_DisplacementAmount", Float) = 1
        _Color ("Color", COLOR) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                //float4 pos: POSITION;
                float4 color : COLOR0;
            };

            uniform float4x4 _ObjectToWorld;
            StructuredBuffer<float3> _Positions;

            float _Tiling;
            float _Octaves;
            float _DisplacementAmount;
            float4 _Color;

            
            float2 get_cos_sin(float a){
                float c = cos(radians(a));
                float s = sin(radians(a));
                return float2(c, s);
            }

            float4x4 get_Rx(float x) {
                float2 cs = get_cos_sin(x);
                float c = cs.x;
                float s = cs.y;
                float4x4 m_Rx = {1, 0, 0, 0,
                                 0, c, s, 0,
                                 0,-s, c, 0,
                                 0, 0, 0, 1};
                return m_Rx;
            }
            float4x4 get_Ry(float y) {
                float2 cs = get_cos_sin(y);
                float c = cs.x;
                float s = cs.y;
                float4x4 m_Ry = {c, 0,-s, 0,
                                0, 1, 0, 0,
                                s, 0, c, 0,
                                0, 0, 0, 1};
                return m_Ry;
            }
            float4x4 get_Rz(float z) {
                float2 cs = get_cos_sin(z);
                float c = cs.x;
                float s = cs.y;
                float4x4 m_Rz = {c, s, 0, 0,
                                -s,c, 0, 0,
                                0, 0, 1, 0,
                                0, 0, 0, 1};
                return m_Rz;
            }
            float4x4 get_R(float x, float y, float z) {
                float4x4 m_x = get_Rx(x);
                float4x4 m_y = get_Ry(y);
                float4x4 m_z = get_Rz(z);
                return mul(m_y, mul(m_x, m_z));
            }

            // https://forum.unity.com/threads/problem-with-lookat-vertex-shader-model-space.384903/
            float4x4 get_lookat(float x, float y, float z){
                float3 lookvec = normalize(float3(x, y, z)); 
                float3 up = float3(0, 1, 0);
 
                // Create LookAt matrix
                float3 zaxis = lookvec;
                float3 xaxis = normalize(cross(up, zaxis));
                float3 yaxis = cross(zaxis, xaxis);
 
                float4x4 lookatMatrix = {
                    xaxis.x,            yaxis.x,            zaxis.x,       0,
                    xaxis.y,            yaxis.y,            zaxis.y,       0,
                    xaxis.z,            yaxis.z,            zaxis.z,       0,
                    0, 0, 0,  1
                };
               
                return lookatMatrix;
            }

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                v2f o;
                uint instanceID = GetIndirectInstanceID(svInstanceID);
                  
                float3 p = _Positions[instanceID]; // position from the buffer
                
                float s = 1-p.y;
                float3 S = float3(s,s,s);
               
                float4x4 m_S = {S.x,0, 0, 0,
                                0, S.y,0, 0,
                                0, 0, S.z,0,
                                0, 0, 0, 1};
                float dist = 10;
                float freq = 10;
                float strength = 10;
                float wave = sin(_Time*((p.x+dist)*freq)) *strength;

                float4x4 m_R = get_lookat(0,0,1);
                // the lookat function is looking at z dir in this case. then the y will be naturally up

                float4x4 m = mul(unity_ObjectToWorld, mul(m_R, m_S));

                float4 wpos = mul(m, v.vertex) + float4(p.x, p.y*10, p.z, 0); // use object space

                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = _Color * lerp(0.25,1,v.vertex.y); // green color based on local height of the mesh
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
