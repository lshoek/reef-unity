Shader "Custom/FilterDepthNormals"
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
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			uniform sampler2D _DepthTex;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
			};
			
			v2f VERT (appdata_base IN)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(IN.vertex);
				OUT.normal = IN.normal;
				return OUT;
			}

			fixed4 FRAG (v2f i) : SV_Target
			{
				float3 forward = mul((float3x3)unity_CameraToWorld, float3(0,0,-1));
				float diff = dot(forward, i.normal);
				if (diff > 0.5) return 0;
				return 1.0;
			}
			ENDCG
		}
	}
		
	Fallback "Diffuse" 
}
