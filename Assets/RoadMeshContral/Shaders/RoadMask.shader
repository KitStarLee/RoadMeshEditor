// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/RoadMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex ("Mask", 2D) = "white" {}
		_CameraParams("CameraParams",Vector)=(0,0,10,10)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD01;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float4 _CameraParams;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 world_pos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float2 camera_pos = _CameraParams.xy;
				float2 map_size =_CameraParams.zw;
				float2 rel_pos = world_pos.xz - camera_pos;
				rel_pos.x = clamp(rel_pos.x,-map_size.x*0.5f,map_size.x*0.5f)/map_size.x;
				rel_pos.y = clamp(rel_pos.y,-map_size.y*0.5f,map_size.y*0.5f)/map_size.y;

				float2 uv2 = rel_pos + float2(0.5f,0.5f);
				o.uv2 = TRANSFORM_TEX(uv2, _MaskTex);

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed col2 = tex2D(_MaskTex, i.uv2);
				col.a=1-col2;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
