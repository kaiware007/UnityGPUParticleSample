Shader "Unlit/GPUParticle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		ZWrite Off
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "GPUParticleCommon.cginc"

			#pragma shader_feature GPUPARTICLE_CULLING_ON

			struct ParticleData
			{
				bool isActive;      // 有効フラグ
				float3 position;    // 座標
				float3 velocity;    // 加速度
				float4 color;       // 色
				float duration;     // 生存時間
				float scale;        // サイズ
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float scale : TEXCOORD1;
			};

			StructuredBuffer<ParticleData> _Particles;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Scale;

			v2f vert(uint iid: SV_InstanceID)
			{
				v2f o;
				int index = GetParticleIndex(iid);
				o.pos = float4(_Particles[index].position, 1);
				o.uv = float2(0,0);
				o.col = _Particles[index].color;
				o.scale = _Particles[index].isActive ? _Particles[index].scale * _Scale : 0;
				
				return o;
			}
			
			[maxvertexcount(4)]
			void geom(point v2f input[1], inout TriangleStream<v2f> outStream)
			{
				v2f o;

				float4 pos = input[0].pos;
				float4 col = input[0].col;
				o.scale = 0;

				float4x4 billboardMatrix = UNITY_MATRIX_V;
				billboardMatrix._m03 = billboardMatrix._m13 = billboardMatrix._m23 = billboardMatrix._m33 = 0;

				for (int x = 0; x < 2; x++)
				{
					for (int y = 0; y < 2; y++)
					{
						float2 uv = float2(x, y);
						o.uv = uv;

						o.pos = pos + mul(float4((uv * 2 - float2(1, 1)) * input[0].scale, 0, 1), billboardMatrix);
						o.pos = mul(UNITY_MATRIX_VP, o.pos);

						o.col = col;

						outStream.Append(o);
					}
				}

				outStream.RestartStrip();
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * i.col;
				return col;
			}
			ENDCG
		}
	}
}
