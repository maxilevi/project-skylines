// -------------------------------------------------------------------
//  Helper functions and macros used in many wireframe shaders
// -------------------------------------------------------------------

#ifndef WFS_CG_INCLUDED
#define WFS_CG_INCLUDED


// -------------------------------------------------------------------
//  Macros

#ifdef _GLOW_ON
	// Lerp 3 times with factors w.x, w.y and w.z
	#define LERP3(c0, c1, w) lerp(c0, lerp(c0, lerp(c0, c1, w.x), w.y), w.z)
#endif


// FWIDTH_DIST: used for screen space and anti-aliasing (AA)
#ifdef _WSTYLE_DEFAULT
	#define FWIDTH_DIST(dist) fwidth(dist)
#else
	#define FWIDTH_DIST(dist) half3(0, 0, 0)
#endif


// Distance from point p0 to line p1-p2
#define DISTTOLINE(p0,p1,p2) length(cross(p0 - p1, p0 - p2)) / length(p2 - p1)
#define DISTTOLINE_LEN(p0,p1,p2,l) length(cross(p0 - p1, p0 - p2)) / l


#ifndef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
	#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 
#endif

#ifdef UNITY_PACK_WORLDPOS_WITH_TANGENT
	#undef UNITY_PACK_WORLDPOS_WITH_TANGENT
#endif


// WFS_FINALCOLOR
#ifdef _EMISSION
	#define WFS_FINALCOLOR_EMISSION(col, s) col.rgb += s.emission;
#else
	#define WFS_FINALCOLOR_EMISSION(col, s)
#endif
#ifdef _WLIGHT_UNLIT
	#ifdef WFS_PASS_FORWARDADD
		#define WFS_FINALCOLOR(col, s)  col *= s.wire; 
	#elif defined(WFS_PASS_META)
		#define WFS_FINALCOLOR(col, s)  s.emission; col.rgb = lerp(s.albedo, col.rgb, s.wire);
	#else
		#define WFS_FINALCOLOR(col, s)  WFS_FINALCOLOR_EMISSION(col, s) col.rgb = lerp(s.albedo, col.rgb, s.wire);
	#endif
#else
	#ifdef WFS_PASS_META
		#define WFS_FINALCOLOR(col, s)  s.emission;
	#else
		#define WFS_FINALCOLOR(col, s)  WFS_FINALCOLOR_EMISSION(col, s)
	#endif
#endif


// WFS_OCCLUDE
#ifdef _WLIGHT_OVERLAY
	#define WFS_OCCLUDE(occlusion, s) occlusion = lerp(1.0, occlusion, s.wire);
#else
	#define WFS_OCCLUDE(occlusion, s)
#endif


// VertexOutput Data
#if defined(WFS_PASS_FORWARDBASE)
	#define WFS_VERTOUT VertexOutputForwardBase
#elif defined(WFS_PASS_FORWARDADD)
	#define WFS_VERTOUT VertexOutputForwardAdd
#elif defined(WFS_PASS_DEFERRED)
	#define WFS_VERTOUT VertexOutputDeferred
#elif defined(WFS_PASS_SHADOWCASTER)
	#define WFS_VERTOUT VertexOutputShadowCaster
#elif defined(WFS_PASS_META)
	#define WFS_VERTOUT v2f_meta
#else
	#define WFS_VERTOUT v2f
#endif


// tangentToWorldAndPackedData
#if defined(WFS_PASS_FORWARDBASE) || defined(WFS_PASS_DEFERRED)
	#if UNITY_VERSION >= 560
		#define TNGNTTOWRLD tangentToWorldAndPackedData
	#else
		#define TNGNTTOWRLD tangentToWorldAndParallax
	#endif
#elif defined(WFS_PASS_FORWARDADD)
	#define TNGNTTOWRLD tangentToWorldAndLightDir
#elif defined(WFS_DIFFUSE) || defined(WFS_VCOLOR)
	#define TNGNTTOWRLD tSpace
#endif


// FRAGMENT_SETUP
#if defined(WFS_PASS_FORWARDBASE) || defined(WFS_PASS_DEFERRED) || defined(WFS_PASS_FORWARDBASE)
	#undef FRAGMENT_SETUP
	#define FRAGMENT_SETUP(x) FragmentCommonData x = \
		WFSFrag(i.tex, i.eyeVec, i.dist, i.TNGNTTOWRLD, i.posWorld);

	#undef FRAGMENT_SETUP_FWDADD
	#define FRAGMENT_SETUP_FWDADD(x) FragmentCommonData x = \
		WFSFrag(i.tex, i.eyeVec, i.dist, i.tangentToWorldAndLightDir, i.posWorld);
#endif

// -------------------------------------------------------------------
//  Material property declarations

// Wire
sampler2D _WTex;
half4 _WColor;
half _WTransparency;
float _WThickness;
#if !defined(_WLIGHT_UNLIT) && !defined(WFS_PROJECTOR)
	half _WEmission;
	half _WGloss;
	half _WMetal;
#endif
float _WParam;
sampler2D _WMask;
half _WInvert;
float _AASmooth;

// Glow
#ifdef _GLOW_ON
	half4 _GColor;
	half _GEmission;
	float _GDist;
	float _GPower;
#endif

// Fade
#ifdef _FADE_ON
	float _FDist;
	float _FPow;
	half _FMode;
#endif

// -------------------------------------------------------------------
//  Fragment functions

// Handles wire thickness, wire threshold and mask
inline void WFSWire(float3 dist, float2 uv, out half mask, out float3 thickness, out float wire) {
	thickness = float3(_WThickness, _WThickness, _WThickness);
	float3 fwidth_dist = FWIDTH_DIST(dist);
	#if defined(_MODE_SCREEN) && defined(_WSTYLE_DEFAULT)
		thickness *= fwidth_dist * 50.0;
	#endif

	// Distance field
	#if defined(_WSTYLE_DEFAULT)
		float3 df = dist - thickness;

		#ifdef _AA_ON
			df /= _AASmooth * fwidth_dist + 1e-6;
			wire = min(df.x, min(df.y, df.z));
			wire = smoothstep(0.0, 1.0, wire + 0.5);
		#else
			wire = min(df.x, min(df.y, df.z));
			wire = step(0.0, wire);
		#endif // _AA_ON
	#else
		float3 df = dist / (thickness*2.0  + 1e-6);
		df += _WParam;
		wire = df.x*df.y*df.z - 0.5;

		#ifdef _AA_ON
			wire /= _AASmooth * fwidth(wire) + 1e-6;
			wire = smoothstep(0.0, 1.0, wire + 0.5);
		#else
			wire = step(0.0, wire);
		#endif // _AA_ON
	#endif

	//Mask
	#ifdef WFS_PROJECTOR
		mask = 1.0;
	#else
		mask = tex2D(_WMask, uv).g;
	#endif
	wire = lerp(1.0, wire, mask);

	// Invert
	wire = lerp(wire, 0.5, _WInvert);
}

#if !defined(WFS_PASS_SHADOWCASTER) 
#include "UnityStandardInput.cginc"
// Get surface properties
inline void WFSSurface(float4 uv, out half4 col, out half3 emission, out half metallic, out half smoothness) {
	#if defined(WFS_PROJECTOR) || defined(WFS_UNLIT)
		col = half4(0.0, 0.0, 0.0, 0.0);
		#if defined(WFS_PROJECTOR) &&  defined(_GLOW_ON) && defined(_ALPHABLEND_ON)
			col.rgb = _GColor.rgb;
		#endif
		emission = half3(0.0, 0.0, 0.0);
		metallic = 0.0;
		smoothness = 0.0;
	#else
		col = half4(Albedo(uv), Alpha(uv.xy));
		emission = Emission(uv.xy);
		half2 metallicGloss = MetallicGloss(uv.xy);
		metallic = metallicGloss.x;
		smoothness = metallicGloss.y;
	#endif
}
#endif

#ifdef _FADE_ON
// Fade out the wire depending on the camera distance
inline void WFSFade(float camDist, inout float wire, out float fade) {
	fade = saturate(exp2(-pow(_FDist*max(0, camDist), _FPow)));
	wire = lerp(_FMode, wire, fade);
}
#endif

#ifdef _GLOW_ON
// Calculate the glow coming from the wire edges
inline void WFSGlow(float3 dist, float3 thickness, half mask, float fade, inout half4 col, inout half3 emission) {
	
	float3 df; // glow distance field
	#ifdef _WSTYLE_DEFAULT
		df = max(0, dist - thickness*0.95);
	#else
		df = dist / (thickness*2.0 + 1e-6) + _WParam;
	#endif
	df /= _GDist + 1e-6;
	df = smoothstep(0.0, 1.0, sqrt(df));

	// color
	half4 glowCol = _GColor*mask;
	half blend = glowCol.a*_GPower*(1.0-_WInvert*0.5);
	#ifdef _FADE_ON
		blend *= fade;
	#endif 
	//BLEND
	glowCol.rgb = lerp(col.rgb, glowCol.rgb, blend);
	glowCol.a = lerp(col.a, 1.0, blend);
	// LERP
	col = LERP3(glowCol, col, df);
	
	// emission
	#ifndef _WLIGHT_UNLIT
		half3 glowEmi = _GColor.rgb*_GEmission;
		#ifdef WFS_PASS_META
			// The Meta pass seems to only look at fragments at vertex points
			emission += glowEmi;
		#elif defined(_EMISSION)
			emission = lerp(emission, LERP3(glowEmi, emission, df), blend);
		#endif
	#endif
}
#endif

// Get the wireframe albedo and alpha
inline half4 WFSWireColor(float3 dist, float3 thickness, float2 uv) {
	#ifdef _WUV_BARYCENTRIC
		#ifdef _WSTYLE_DEFAULT
			float3 df = dist / thickness;
			float u = min(df.x, min(df.y, df.z));
		#else
			float3 df = dist / (_WThickness*1.5) + _WParam;
			float u = df.x*df.y*df.z;
		#endif
		// Set _WTex to clamp mode for best results
		half4 texcol = tex2D(_WTex, float2(saturate(u), 0.5)); 
		half4 wireCol = _WColor*texcol;
	#else // UV0
		half4 wireCol = _WColor*tex2D(_WTex, uv);
	#endif
	return wireCol;
}

#ifndef WFS_PROJECTOR
// Blend the wireframe properties with the surface properties, then lerp according to the wire threshold
inline void WFSBlendLerp(half4 surfCol, half3 surfEmi, half4 wireCol, float wire, inout half4 col, 
	inout half3 emission, inout half metallic, inout half smoothness) {
	half blend = wireCol.a;

	// BLEND
	wireCol.rgb = lerp(surfCol.rgb, wireCol.rgb, blend); // alpha blend wire to surface
	half transparency = lerp(surfCol.a*_WTransparency, _WTransparency, blend);

	// WIRELERP
	col.rgb = lerp(wireCol.rgb, col.rgb, wire);
	col.a = lerp(transparency, col.a, wire);

	#ifndef _WLIGHT_UNLIT
		// BLEND
		half wireMetal = lerp(metallic, _WMetal, blend);
		half wireSmooth = lerp(smoothness, _WGloss, blend);

		// WIRELERP
		metallic = lerp(wireMetal, metallic, wire);
		#if defined(_AA_ON) && !defined(_WLIGHT_UNLIT)
			// Just lerping the smoothness looks strange with anti-aliasing
			// this code tones down the AA when wire smoothness > surface smoothness
			half smoothnesslerp = lerp(0.1, 0.9, wireSmooth > smoothness); 
			smoothnesslerp = step(smoothnesslerp, wire);
		#else
			half smoothnesslerp = wire;
		#endif
		smoothness = lerp(wireSmooth, smoothness, smoothnesslerp);

		#ifdef WFS_PASS_META
			// The Meta pass seems to only look at fragments at vertex points
			emission += _WEmission * wireCol.rgb * transparency * blend;
		#elif defined(_EMISSION)
			// BLEND
			half3 wireEmi = lerp(surfEmi, _WEmission*wireCol.rgb, blend);
			// WIRELERP
			emission = lerp(emission, lerp(wireEmi, emission, wire), col.a);
		#endif
	#endif
}
#endif

#if !defined(WFS_PASS_SHADOWCASTER) && !defined(WFS_UNLIT) && !defined(WFS_DIFFUSE) && !defined(WFS_VCOLOR)
// The root fragment function
inline void WFSFragBase(float3 dist, float4 uv,
	#ifdef _FADE_ON
		float camDist,
	#endif
	out half3 albedo, out half alpha, out half3 emission, out half metallic, out half smoothness, out float wire) {
	
	half4 col;
	WFSSurface(uv, col, emission, metallic, smoothness);
	half3 surfEmi = emission;
	half4 surfCol = col;

	half mask;
	float fade;
	float3 thickness;
	WFSWire(dist, uv.xy, mask, thickness, wire);

	#ifdef _FADE_ON
		WFSFade(camDist, wire, fade);
	#endif

	#ifdef _GLOW_ON
		WFSGlow(dist, thickness, mask, fade, col, emission);
	#endif

	half4 wireCol = WFSWireColor(dist, thickness, uv.xy);

	#ifdef WFS_PROJECTOR
		// albedo = lerp(wireCol.rgb, col.rgb, wire);
		albedo = lerp(wireCol.rgb, col.rgb, wire);
		#ifdef _ALPHABLEND_ON
			alpha = lerp(_WTransparency, col.a, wire);
		#else
			alpha = _WTransparency;
			albedo *= alpha;
		#endif
	#else
		WFSBlendLerp(surfCol, surfEmi, wireCol, wire, col, emission, metallic, smoothness);
		albedo = col.rgb;
		alpha = col.a;
	#endif
}

#ifndef WFS_PROJECTOR
inline FragmentCommonData WFSFrag (float4 i_tex, half3 i_eyeVec, float3 i_dist, half4 tangentToWorld[3], float3 i_posWorld){
	half alpha, metallic, smoothness;
	float wire;
	half3 emission, albedo;
	WFSFragBase(i_dist, i_tex,
		#ifdef _FADE_ON
		length(i_posWorld.xyz - _WorldSpaceCameraPos),
		#endif
		albedo, alpha, emission, metallic, smoothness, wire);

	#if defined(_ALPHATEST_ON)
		clip (alpha - _Cutoff);
	#endif

	FragmentCommonData o = (FragmentCommonData)0;
	o.diffColor = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ o.specColor, /*out*/ o.oneMinusReflectivity);
	o.smoothness = smoothness;

	o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
	o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
	o.posWorld = i_posWorld;
	o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);

	#ifdef _WLIGHT_UNLIT
		o.wire = wire;
		#ifndef WFS_PASS_FORWARDADD
			#if defined(WFS_PASS_DEFERRED) || defined(WFS_PASS_META)
				o.specColor *= wire;
				o.smoothness *= wire;
				o.diffColor *= wire;
			#endif
			albedo = PreMultiplyAlpha(albedo, alpha, o.oneMinusReflectivity, alpha);
			o.albedo = albedo;
		#endif
	#elif defined(_WLIGHT_OVERLAY) && defined(_NORMALMAP)
		// Don't bump an overlay wireframe
		o.normalWorld = lerp(normalize(tangentToWorld[2].xyz), o.normalWorld, wire);
		o.wire = wire;
	#endif

	#ifdef _EMISSION
		o.emission = emission;
	#endif
	
	return o;
}
#endif // WFS_PROJECTOR

#endif // !defined(WFS_PASS_SHADOWCASTER) && !defined(WFS_UNLIT)

#ifdef WFS_PASS_SHADOWCASTER
// Fragment code for the shadow pass, which only cares about alpha
inline half WFSAlpha(float3 dist, float2 uv, float3 posWorld){
	float camDist = length(posWorld - _WorldSpaceCameraPos);
	half mask; 
	float wire;
	float3 thickness;
	WFSWire(dist, uv, mask, thickness, wire);
	#ifdef _FADE_ON
		float fade;
		WFSFade(camDist, wire, fade);
	#endif // _FADE_ON
	half surfAlpha = tex2D(_MainTex, uv).a * _Color.a;
	half4 wireAlpha = WFSWireColor(dist, thickness, uv.xy).a;

	half transparency = lerp(surfAlpha*_WTransparency, _WTransparency, wireAlpha*mask);
	return lerp(transparency, surfAlpha, wire);
}

#define WFS_VERTSHADOW(o, vertex) UNITY_INITIALIZE_OUTPUT(WFS_VERTOUT, o); \
	TRANSFER_SHADOW_CASTER_NOPOS(o,o._pos); \
	o.posWorld = mul(unity_ObjectToWorld, vertex).xyz; 
#endif


// -------------------------------------------------------------------
//  Geometry functions (shader model 4.0)

// Calculate barycentric distances
#ifdef _WSTYLE_SMOOTH
	#define WFSMAXDIST 1.0
#else
	#define WFSMAXDIST 10000.0
#endif
inline void WFSgeom(float3 p0, float3 p1, float3 p2, out float3 d0, out float3 d1, out float3 d2) {
	float4 d;

	#if defined(_MODE_DEFAULT) || defined(_MODE_WORLD) || defined(_QUAD_ON)
		float l0 = length(p0-p1);
		float l1 = length(p0-p2);
		float l2 = length(p2-p1);
	#endif

	#if defined(_MODE_DEFAULT) || defined(_MODE_WORLD)
		d = float4(0.0,
			DISTTOLINE_LEN(p2, p0, p1, l0),
			DISTTOLINE_LEN(p0, p1, p2, l2),
			DISTTOLINE_LEN(p1, p0, p2, l1));
		#ifdef _MODE_DEFAULT
			d /= min(d.y, min(d.z, d.w));
		#endif
	#else
		d = float4(0.0, 1.0, 1.0, 1.0); //barycentric or screen space
	#endif

	d0 = d.xzx;
	d1 = d.xxw;
	d2 = d.yxx;

	#ifdef _QUAD_ON
		d0.x = WFSMAXDIST * ((l0 > l1) && (l0 > l2));
		d0.z = WFSMAXDIST * ((l1 >= l0) && (l1 > l2));
		d1.y = WFSMAXDIST * ((l2 >= l0) && (l2 >= l1));
	#endif
}

#endif // WFS_CG_INCLUDED