// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Real Ivy/Flat leaves"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_AlbedoTexture("AlbedoTexture", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Specular("Specular", Range( 0 , 1)) = 0
		_SpecularTexture("Specular Texture", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_NormalTexture("NormalTexture", 2D) = "bump" {}
		_HeightMap("HeightMap", 2D) = "white" {}
		_ParallaxIntensity("Parallax Intensity", Range( 0 , 0.1)) = 0.01
		_Frequency("Frequency", Float) = 1
		_Amplitude("Amplitude", Float) = 1
		_Center("Center", Float) = 0
		_Radius("Radius", Float) = 0.2
		_WindPattern("Wind Pattern", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 viewDir;
			INTERNAL_DATA
			half ASEVFace : VFACE;
		};

		uniform sampler2D _WindPattern;
		uniform float _Frequency;
		uniform float _Amplitude;
		uniform float _Center;
		uniform float _Radius;
		uniform sampler2D _NormalTexture;
		uniform sampler2D _HeightMap;
		uniform float4 _HeightMap_ST;
		uniform float _ParallaxIntensity;
		uniform float4 _Color;
		uniform sampler2D _AlbedoTexture;
		uniform sampler2D _SpecularTexture;
		uniform float _Specular;
		uniform float _Smoothness;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float4 appendResult87 = (float4(ase_vertex3Pos.x , ase_vertex3Pos.y , 0.0 , 0.0));
			float temp_output_66_0 = ( _Time.y * _Frequency );
			float4 appendResult94 = (float4(temp_output_66_0 , ( temp_output_66_0 / 2.0 ) , 0.0 , 0.0));
			float4 temp_output_63_0 = ( ( tex2Dlod( _WindPattern, float4( ( appendResult87 + appendResult94 ).xy, 0, 0.0) ) * _Amplitude ) + ( ( UNITY_PI + _Center ) / 2.0 ) );
			float3 ase_vertexNormal = v.normal.xyz;
			float4 ase_vertexTangent = v.tangent;
			v.vertex.xyz += ( ( cos( temp_output_63_0 ) * float4( ase_vertexNormal , 0.0 ) * _Radius * v.color.a ) + ( ( sin( temp_output_63_0 ) + -1.0 ) * float4( cross( ase_vertexNormal , ase_vertexTangent.xyz ) , 0.0 ) * _Radius * v.color.a ) ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_HeightMap = i.uv_texcoord * _HeightMap_ST.xy + _HeightMap_ST.zw;
			float2 Offset109 = ( ( tex2D( _HeightMap, uv_HeightMap ).r - 1 ) * i.viewDir.xy * _ParallaxIntensity ) + i.uv_texcoord;
			float2 Offset117 = Offset109;
			float3 tex2DNode103 = UnpackNormal( tex2D( _NormalTexture, Offset117 ) );
			float4 appendResult132 = (float4(tex2DNode103.r , tex2DNode103.g , ( tex2DNode103.b * i.ASEVFace ) , 0.0));
			o.Normal = appendResult132.xyz;
			float4 tex2DNode76 = tex2D( _AlbedoTexture, Offset117 );
			o.Albedo = ( _Color * tex2DNode76 ).rgb;
			o.Specular = ( tex2D( _SpecularTexture, Offset117 ) * _Specular ).rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
			clip( tex2DNode76.a - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18707
24;349;1706;986;1808.881;1220.292;1.3;True;True
Node;AmplifyShaderEditor.SimpleTimeNode;65;-3970.744,475.5388;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-3969.887,549.0299;Float;False;Property;_Frequency;Frequency;9;0;Create;True;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-3717.07,500.8918;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-3725.737,649.358;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;100;-3508.737,601.358;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;88;-3662.847,308.0062;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;94;-3342.102,501.5068;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-3344.573,331.9021;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;-2938.709,454.5248;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-2558.526,857.9333;Float;False;Property;_Center;Center;11;0;Create;True;0;0;False;0;False;0;-0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;59;-2559.701,774.1868;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-2346.226,811.3502;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;85;-2479.124,379.2203;Inherit;True;Property;_WindPattern;Wind Pattern;13;0;Create;True;0;0;False;0;False;-1;2d15e4a275691a645ab383c7c5724d67;2d15e4a275691a645ab383c7c5724d67;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;62;-2560.129,965.3518;Float;False;Constant;_Float5;Float 5;1;0;Create;True;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-2312.713,604.1532;Float;False;Property;_Amplitude;Amplitude;10;0;Create;True;0;0;False;0;False;1;0.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-2080.544,521.5764;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;60;-2189.929,897.7156;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-2911.027,-745.8546;Float;False;Property;_ParallaxIntensity;Parallax Intensity;8;0;Create;True;0;0;False;0;False;0.01;0.0217;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;110;-2923.407,-956.1487;Inherit;True;Property;_HeightMap;HeightMap;7;0;Create;True;0;0;False;0;False;-1;7162178e217d90342ba7a90c3498130e;2a256c3a8dd4fb448b777e90692a6e47;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;118;-2870.982,-1089.352;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;114;-2871.027,-653.8546;Float;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1902.379,685.9502;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ParallaxMappingNode;109;-2441.422,-941.0354;Inherit;False;Normal;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-1627.301,955.1239;Float;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;False;0;False;-1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;26;-1780.596,1142.827;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;-2147.426,-940.2611;Float;False;Offset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TangentVertexDataNode;25;-1782.183,1322.588;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;21;-1612.395,845.3584;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;15;-969.5806,626.2561;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosOpNode;52;-1070.412,304.4485;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;33;-1241.802,1411.873;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CrossProductOpNode;24;-1333.071,1221.004;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;13;-1066.285,402.3677;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;74;-1359.162,663.071;Float;False;Property;_Radius;Radius;12;0;Create;True;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;131;-1550.654,-753.5874;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-1392.97,852.8091;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;103;-1725.713,-963.9395;Inherit;True;Property;_NormalTexture;NormalTexture;6;0;Create;True;0;0;False;0;False;-1;67631cd853304ee45bfa2775f9c9fc9c;e929b2198f5572746876750909242f93;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;133;-1343.884,-774.2144;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-613.9263,373.4123;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;106;-1709.884,-655.9253;Inherit;True;Property;_SpecularTexture;Specular Texture;4;0;Create;True;0;0;False;0;False;-1;2a0ee0866fd464e4b85928a591e4d625;0310cf39af27532468d205f8a6c9ebcf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;134;-930.4792,-1213.019;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-941.5339,1053.855;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;76;-1720.486,-1208.143;Inherit;True;Property;_AlbedoTexture;AlbedoTexture;1;0;Create;True;0;0;False;0;False;-1;a13d0741ba1b57640b1b4b035043021f;78e19074a8af01548b8086c7aa2050b4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;104;-1704.707,-454.1123;Float;False;Property;_Specular;Specular;3;0;Create;True;0;0;False;0;False;0;0.137;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;132;-1112.708,-834.9487;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-617.6169,-960.1431;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-1707.861,-367.3922;Float;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;False;1;0.388;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-382.6417,476.255;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-1333.66,-532.3452;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;25.12077,-423.7062;Float;False;True;-1;2;;0;0;StandardSpecular;Real Ivy/Flat leaves;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;1;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;66;0;65;0
WireConnection;66;1;56;0
WireConnection;100;0;66;0
WireConnection;100;1;101;0
WireConnection;94;0;66;0
WireConnection;94;1;100;0
WireConnection;87;0;88;1
WireConnection;87;1;88;2
WireConnection;90;0;87;0
WireConnection;90;1;94;0
WireConnection;73;0;59;0
WireConnection;73;1;72;0
WireConnection;85;1;90;0
WireConnection;69;0;85;0
WireConnection;69;1;68;0
WireConnection;60;0;73;0
WireConnection;60;1;62;0
WireConnection;63;0;69;0
WireConnection;63;1;60;0
WireConnection;109;0;118;0
WireConnection;109;1;110;1
WireConnection;109;2;111;0
WireConnection;109;3;114;0
WireConnection;117;0;109;0
WireConnection;21;0;63;0
WireConnection;52;0;63;0
WireConnection;24;0;26;0
WireConnection;24;1;25;0
WireConnection;58;0;21;0
WireConnection;58;1;57;0
WireConnection;103;1;117;0
WireConnection;133;0;103;3
WireConnection;133;1;131;0
WireConnection;10;0;52;0
WireConnection;10;1;13;0
WireConnection;10;2;74;0
WireConnection;10;3;15;4
WireConnection;106;1;117;0
WireConnection;31;0;58;0
WireConnection;31;1;24;0
WireConnection;31;2;74;0
WireConnection;31;3;33;4
WireConnection;76;1;117;0
WireConnection;132;0;103;1
WireConnection;132;1;103;2
WireConnection;132;2;133;0
WireConnection;135;0;134;0
WireConnection;135;1;76;0
WireConnection;37;0;10;0
WireConnection;37;1;31;0
WireConnection;108;0;106;0
WireConnection;108;1;104;0
WireConnection;0;0;135;0
WireConnection;0;1;132;0
WireConnection;0;3;108;0
WireConnection;0;4;107;0
WireConnection;0;10;76;4
WireConnection;0;11;37;0
ASEEND*/
//CHKSM=E2B80C4BDEECB8050BF240B63FC2390138DA0912