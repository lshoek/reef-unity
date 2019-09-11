Shader "Custom/DepthCopy"
{
	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Extensions.cginc"
	#pragma vertex VERT
	#pragma fragment FRAG
	ENDCG

	Properties
	{
		_DepthTex("Base (RGB)", 2D) = "white" {}
		_SurfaceCutoff0("Surface Cutoff 0", Float) = 0.05
		_SurfaceCutoff1("Surface Cutoff 1", Float) = 0.05
	}

	SubShader
	{
		Cull Off 
		ZWrite Off 
		ZTest Always

		Pass
		{
			CGPROGRAM
			uniform sampler2D _DepthTex;
			uniform float _SurfaceCutoff0;
			uniform float _SurfaceCutoff1;

			struct appdata
			{
				float4 vertex : POSITION;
				half4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half4 uv : TEXCOORD0;
			};

			float map(float value, float min1, float max1, float min2, float max2)
			{
				float perc = (value - min1) / (max1 - min1);
				float result = perc * (max2 - min2) + min2;
				return result;
			}
			
			v2f VERT (appdata IN)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.uv.x = inv(OUT.uv.x);
				return OUT;
			}

			fixed4 FRAG (v2f i) : SV_Target
			{
				// float d;
				// float3 n;

				// DecodeDepthNormal(tex2D(_DepthNormalsTex, i.uv), d, n);

				// float3 forward = float3(0, 0, 1);

				// d = (dot(forward, n) < _SurfaceCutoff) ? d : 0;
				// d = (d < 1.0) ? d : 0;

				//return inv(d * 2.0);

				fixed depth = SAMPLE_DEPTH_TEXTURE_PROJ(_DepthTex, i.uv);
				depth = map(depth, _SurfaceCutoff0, _SurfaceCutoff1, 0.0, 1.0);
				return depth;
			}
			ENDCG
		}
	}
		
	Fallback "Diffuse" 
}
