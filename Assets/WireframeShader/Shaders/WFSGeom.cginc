#ifndef WFS_GEOM_INCLUDED
#define WFS_GEOM_INCLUDED


// Geometry Shader
#ifdef WFS_TWOSIDED
	[maxvertexcount(6)]
#else
	[maxvertexcount(3)]
#endif
void geom(triangle WFS_VERTOUT i[3], inout TriangleStream<WFS_VERTOUT> stream) {
	WFS_VERTOUT i0, i1, i2; i0 = i[0]; i1 = i[1]; i2 = i[2];
	
	WFSgeom(i0.posWorld, i1.posWorld, i2.posWorld, 
		/*out*/i0.dist, /*out*/i1.dist, /*out*/i2.dist);
	
	stream.Append(i0); stream.Append(i1); stream.Append(i2);

	// Two-sided rendering
	#ifdef WFS_TWOSIDED
		#if !defined(WFS_PASS_SHADOWCASTER) && !defined(WFS_PASS_META) && !defined(WFS_UNLIT)
			// Invert normals
			#if defined(WFS_VCOLOR) || (defined(WFS_DIFFUSE) && !defined(_NORMALMAP))
				i0.worldNormal.xyz *= -1; i1.worldNormal.xyz *= -1; i2.worldNormal.xyz *= -1;
			#else
				i0.TNGNTTOWRLD[2].xyz *= -1; i1.TNGNTTOWRLD[2].xyz *= -1; i2.TNGNTTOWRLD[2].xyz *= -1;
			#endif
		#endif

		// Emit triangle with inverted winding order
		stream.Append(i2); stream.Append(i0); stream.Append(i1);
	#endif
}

#endif // WFS_GEOM_INCLUDED