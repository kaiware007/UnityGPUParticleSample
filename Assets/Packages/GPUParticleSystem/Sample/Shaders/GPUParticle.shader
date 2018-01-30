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
			//StructuredBuffer<uint> _ActiveIndexList;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			// test
			//StructuredBuffer<uint> _ParticleActiveList;
			//StructuredBuffer<uint> _InViewsList;

			//uint GetParticleIndex(int index) {
			//	//#ifdef GPUPARTICLE_CULLING_ON
			//	//	return _InViewsList[index];
			//	//#else
			//	//	return _ParticleActiveList[index];
			//	//#endif

			//	return _InViewsList[index];

			//	//return _ParticleActiveList[index];
			//}

			v2f vert (uint id : SV_VertexID)
			{
				v2f o;
				int index = GetParticleIndex(id);
				o.pos = float4(_Particles[index].position, 1);
				o.uv = float2(0,0);
				o.col = _Particles[index].color;
				o.scale = _Particles[index].isActive ? _Particles[index].scale : 0;
				//o.scale = 1;

				return o;
			}
			
			// ジオメトリシェーダ
			[maxvertexcount(4)]
			void geom(point v2f input[1], inout TriangleStream<v2f> outStream)
			{
				v2f o;

				// 全ての頂点で共通の値を計算しておく
				float4 pos = input[0].pos;
				float4 col = input[0].col;
				o.scale = 0;

				// 四角形になるように頂点を生産
				for (int x = 0; x < 2; x++)
				{
					for (int y = 0; y < 2; y++)
					{
						// ビルボード用の行列
						float4x4 billboardMatrix = UNITY_MATRIX_V;
						billboardMatrix._m03 = billboardMatrix._m13 = billboardMatrix._m23 = billboardMatrix._m33 = 0;

						// テクスチャ座標
						float2 uv = float2(x, y);
						o.uv = uv;

						// 頂点位置を計算
						o.pos = pos + mul(float4((uv * 2 - float2(1, 1)) * input[0].scale, 0, 1), billboardMatrix);
						o.pos = mul(UNITY_MATRIX_VP, o.pos);

						// 色
						o.col = col;

						// ストリームに頂点を追加
						outStream.Append(o);
					}
				}

				// トライアングルストリップを終了
				outStream.RestartStrip();
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.col;
				return col;
				//return fixed4(1, 0, 0, 1);	// test
			}
			ENDCG
		}
	}
}
