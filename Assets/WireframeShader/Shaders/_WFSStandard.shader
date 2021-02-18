Shader "WireframeShader/Standard"{
    Properties{
        // Wire
        _WTex("Wire Texture", 2D) = "white" {}
        [KeywordEnum(UV0, Barycentric)] _WUV("Wire UV Set", Float) = 0.0
        _WColor("Wire Color", Color) = (0,0,1,1)
        _WTransparency("Wire Transparency", Range(0.0,1.0)) = 1.0
        _WOpacity("Wire Opacity", Range(0.0,1.0)) = 1.0
        _WEmission("Wire Emission", Float) = 0
        _WThickness("Wire Thickness", Float) = 0.05
        _WGloss("Wire Smoothness", Range(0.0,1.0)) = 0
        [Gamma] _WMetal("Wire Metallic", Range(0.0,1.0)) = 0.0
        _WParam("Wire Parameter", Float) = 0
        _WMode("Wire Mode", Float) = 0.0
        [KeywordEnum(Default, Smooth)] _WStyle("Wire Style", Float) = 0.0 
        _WMask("Wire Mask", 2D) = "white" {}
        [KeywordEnum(Surface, Overlay, Unlit)] _WLight("Wire Lighting Mode", Float) = 0.0
        _AASmooth("AA Smoothing", Float) = 1.5
        _WInvert("Wire Invert", Float) = 0.0
        [Toggle] _Quad("Quad Mesh", Float) = 0.0

        // Glow
        [Toggle] _Glow("Glow Enable", Float) = 0.0
        _GColor("Glow Color", Color) = (0,0,0,1)
        _GEmission("Glow Emission", Float) = 0
        _GDist("Glow Distance", Float) = 0.35
        _GPower("Glow Power", Float) = 0.5

        // Fade
        [Toggle] _Fade("Glow Enable", Float) = 0.0
        _FMode("Fade Mode", Float) = 1
        _FDist("Fade Distance", Float) = 0.1
        _FPow("Fade Power", Float) = 5

        // Settings
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _Fold("__fld", Float) = 1.0
        [HideInInspector] _Limits("__lmt", Float) = 1.0
        [HideInInspector] _TwoSided ("__two", Float) = 1.0

        // Surface
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        _DetailMask("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}
        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
        #define _EMISSION
        #define _AA_ON
    ENDCG

    SubShader {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300
        Cull [_Cull] //WFS


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 4.0

            #pragma shader_feature _GLOW_ON
            #pragma shader_feature _FADE_ON 
            #pragma shader_feature _QUAD_ON 
            // #pragma shader_feature _AA_ON
            #pragma shader_feature _WLIGHT_OVERLAY _WLIGHT_SURFACE _WLIGHT_UNLIT
            #pragma shader_feature _WUV_UV0 _WUV_BARYCENTRIC
            #pragma shader_feature _ _MODE_SCREEN _MODE_WORLD _MODE_DEFAULT
            #pragma shader_feature _WSTYLE_DEFAULT _WSTYLE_SMOOTH

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON WFS_TWOSIDED
            // #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            // #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            // #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex vertBase
            #pragma geometry geom
            #pragma fragment fragBase

            #define WFS_PASS_FORWARDBASE
            // #include "UnityStandardCoreForward.cginc"
            #include "WFSSetup.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass{
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 4.0

            #pragma shader_feature _GLOW_ON
            #pragma shader_feature _FADE_ON 
            #pragma shader_feature _QUAD_ON 
            // #pragma shader_feature _AA_ON
            #pragma shader_feature _WLIGHT_OVERLAY _WLIGHT_SURFACE _WLIGHT_UNLIT
            #pragma shader_feature _WUV_UV0 _WUV_BARYCENTRIC
            #pragma shader_feature _ _MODE_SCREEN _MODE_WORLD _MODE_DEFAULT
            #pragma shader_feature _WSTYLE_DEFAULT _WSTYLE_SMOOTH

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON WFS_TWOSIDED
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature ___ _DETAIL_MULX2
            // #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma geometry geom
            #pragma fragment fragAdd

            #define WFS_PASS_FORWARDADD
            // #include "UnityStandardCoreForward.cginc"
            #include "WFSSetup.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 4.0

            #pragma shader_feature _FADE_ON 
            #pragma shader_feature _QUAD_ON 
            #pragma shader_feature _WUV_UV0 _WUV_BARYCENTRIC
            #pragma shader_feature _ _MODE_SCREEN _MODE_WORLD _MODE_DEFAULT
            #pragma shader_feature _WSTYLE_DEFAULT _WSTYLE_SMOOTH

            // -------------------------------------


            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON WFS_TWOSIDED
            #pragma shader_feature _METALLICGLOSSMAP
            // #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma geometry geom
            #pragma fragment fragShadowCaster

            #define WFS_PASS_SHADOWCASTER
            // #include "UnityStandardShadow.cginc"
            #include "WFSSetup.cginc"

            ENDCG
        }

        Pass{
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            #pragma target 4.0
            #pragma exclude_renderers nomrt

            #pragma shader_feature _GLOW_ON
            #pragma shader_feature _FADE_ON 
            #pragma shader_feature _QUAD_ON 
            // #pragma shader_feature _AA_ON
            #pragma shader_feature _WLIGHT_OVERLAY _WLIGHT_SURFACE _WLIGHT_UNLIT
            #pragma shader_feature _WUV_UV0 _WUV_BARYCENTRIC
            #pragma shader_feature _ _MODE_SCREEN _MODE_WORLD _MODE_DEFAULT
            #pragma shader_feature _WSTYLE_DEFAULT _WSTYLE_SMOOTH


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON WFS_TWOSIDED
            // #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature ___ _DETAIL_MULX2
            // #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_prepassfinal

            #pragma vertex vertDeferred
            #pragma geometry geom
            #pragma fragment fragDeferred

            #define WFS_PASS_DEFERRED
            // #include "UnityStandardCore.cginc"
            #include "WFSSetup.cginc"

            ENDCG
        }

        Pass {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma target 4.0

            #pragma vertex vert_meta
            #pragma geometry geom
            #pragma fragment frag_meta

            #pragma shader_feature _GLOW_ON
            #pragma shader_feature _QUAD_ON 
            #pragma shader_feature WFS_TWOSIDED
            #pragma shader_feature _WLIGHT_OVERLAY _WLIGHT_SURFACE _WLIGHT_UNLIT
            #pragma shader_feature _WUV_UV0 _WUV_BARYCENTRIC
            #pragma shader_feature _ _MODE_SCREEN _MODE_WORLD _MODE_DEFAULT
            #pragma shader_feature _WSTYLE_DEFAULT _WSTYLE_SMOOTH

            // #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature ___ _DETAIL_MULX2

            #define WFS_PASS_META
            // #include "UnityStandardMeta.cginc"
            #include "WFSSetup.cginc"
            ENDCG
        }
    }

    FallBack "VertexLit"
    CustomEditor "WFShader.WFShaderGUI"
}
