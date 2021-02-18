namespace WFShader {

    public enum WFSBlendMode {
        Opaque,
        Cutout,
        Fade,       // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Transparent, // Physically plausible transparency mode, implemented as alpha pre-multiply
        Additive, // Blend One One
        SoftAdditive, //Blend OneMinusDstColor One
        Multiplicative, //Blend DstColor OneMinusSrcAlpha
        MultiplicativeDouble, //Blend DstColor SrcColor
    }

    // Projectors rendering modes
    public enum WFSBlendModeProj {
        Fade=2,       
        Additive=4, // Blend One One
        SoftAdditive=5, //Blend OneMinusDstColor One
        //Multiplicative=6, //Blend DstColor OneMinusSrcAlpha
    }

    public enum WireMode {
        Default,
        ScreenSpace,
        WorldSpace,
        Barycentric,
    }

    // The smooth wire style doesn't support screen space
    public enum WireModeSmooth {
        Default=0,
        WorldSpace=2,
        Barycentric=3,
    }

    public enum WireStyle {
        Default,
        Smooth
    }

    public enum WireTextureUV {
        UV0,
        Barycentric
    }

    public enum FadeMode {
        Surface = 1,
        Wire = 0
    }

    public enum LightMode {
        SurfaceLit,
        Overlay,
        Unlit
    }
    
}