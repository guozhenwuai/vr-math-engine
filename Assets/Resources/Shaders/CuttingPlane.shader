
Shader "MathModel/CuttingPlane"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
	}

		SubShader
		{
			Tags{ "Queue" = "Transparent-2" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
			Cull off

			Pass
		{
			Blend SrcAlpha One
			ZWrite On
			CGPROGRAM
#include "Lighting.cginc"  
			fixed4 _Color;

		struct v2f
		{
			float4 pos : SV_POSITION;
			float3 worldNormal : TEXCOORD0;
			float3 lightDir : TEXCOORD1;
		};

		v2f vert(appdata_full v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			float diffuse = abs(dot(i.worldNormal, i.lightDir));

			float4 ambient = UNITY_LIGHTMODEL_AMBIENT;

			// view independent term
			float4 viewIndependent = _Color * (ambient + _LightColor0 * diffuse);
			return viewIndependent;
		}

#pragma vertex vert  
#pragma fragment frag     
			ENDCG
		}
		}

			FallBack "Diffuse"
}