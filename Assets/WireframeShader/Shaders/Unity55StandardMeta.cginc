// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Based on Unity 5.5.3f1

#ifndef WFS_STANDARD_META_INCLUDED
#define WFS_STANDARD_META_INCLUDED

// Functionality for Standard shader "meta" pass
// (extracts albedo/emission for lightmapper etc.)

// define meta pass before including other files; they have conditions
// on that in some places
#define UNITY_PASS_META 1

#include "UnityCG.cginc"
#include "UnityStandardInput.cginc"
#include "UnityMetaPass.cginc"
#include "Unity55StandardCore.cginc" //WFS

struct v2f_meta
{
	float4 uv		: TEXCOORD0;
	float4 pos		: SV_POSITION;
	WFS_OUTPUT_WPOS(1, 2) //WFS
};

v2f_meta vert_meta (VertexInput v)
{
	v2f_meta o;
	o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
	o.uv = TexCoords(v);
	o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz; o.dist = float3(0,0,0); //WFS
	return o;
}

// Albedo for lightmapping should basically be diffuse color.
// But rough metals (black diffuse) still scatter quite a lot of light around, so
// we want to take some of that into account too.
half3 UnityLightmappingAlbedo (half3 diffuse, half3 specular, half smoothness)
{
	half roughness = SmoothnessToRoughness(smoothness);
	half3 res = diffuse;
	res += specular * roughness * 0.5;
	return res;
}

float4 frag_meta (v2f_meta i) : SV_Target
{
	// we're interested in diffuse & specular colors,
	// and surface roughness to produce final albedo.
	// FragmentCommonData data = UNITY_SETUP_BRDF_INPUT (i.uv);  //WFS
	half4 tangentToWorld[3];  //WFS
	FragmentCommonData data = WFSFrag(i.uv, half3(1,1,1), i.dist, tangentToWorld,  half3(1,1,1)); //WFS

	UnityMetaInput o;
	UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);

	o.Albedo = UnityLightmappingAlbedo (data.diffColor, data.specColor, data.smoothness);
	// o.Emission = Emission(i.uv.xy); // WFS
	o.Emission = WFS_FINALCOLOR(o.Albedo, data) //WFS

	return UnityMetaFragment(o);
}


#endif // WFS_STANDARD_META_INCLUDED