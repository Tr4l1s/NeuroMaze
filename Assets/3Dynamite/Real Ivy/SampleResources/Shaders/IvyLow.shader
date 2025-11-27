// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Real Ivy/Ivy Low"
{
	Properties
	{
		_AlbedoTexture("Albedo Texture", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_NormalTexture("Normal Texture", 2D) = "bump" {}
		_Specular("Specular", Range( 0 , 1)) = 0
		_SpecularTexture("Specular Texture", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			half ASEVFace : VFACE;
		};

		uniform sampler2D _NormalTexture;
		uniform float4 _NormalTexture_ST;
		uniform sampler2D _AlbedoTexture;
		uniform float4 _AlbedoTexture_ST;
		uniform sampler2D _SpecularTexture;
		uniform float4 _SpecularTexture_ST;
		uniform float _Specular;
		uniform float _Smoothness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_NormalTexture = i.uv_texcoord * _NormalTexture_ST.xy + _NormalTexture_ST.zw;
			float3 tex2DNode103 = UnpackNormal( tex2D( _NormalTexture, uv_NormalTexture ) );
			float4 appendResult132 = (float4(tex2DNode103.r , tex2DNode103.g , ( tex2DNode103.b * i.ASEVFace ) , 0.0));
			o.Normal = appendResult132.xyz;
			float2 uv_AlbedoTexture = i.uv_texcoord * _AlbedoTexture_ST.xy + _AlbedoTexture_ST.zw;
			float4 tex2DNode76 = tex2D( _AlbedoTexture, uv_AlbedoTexture );
			o.Albedo = tex2DNode76.rgb;
			float2 uv_SpecularTexture = i.uv_texcoord * _SpecularTexture_ST.xy + _SpecularTexture_ST.zw;
			o.Specular = ( tex2D( _SpecularTexture, uv_SpecularTexture ) * _Specular ).rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
			clip( tex2DNode76.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18707
68;315;1706;986;3107.025;1746.961;2.342349;True;True
Node;AmplifyShaderEditor.FaceVariableNode;131;-1550.654,-753.5874;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;103;-1725.713,-963.9395;Inherit;True;Property;_NormalTexture;Normal Texture;2;0;Create;True;0;0;False;0;False;-1;67631cd853304ee45bfa2775f9c9fc9c;e929b2198f5572746876750909242f93;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;133;-1343.884,-774.2144;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;106;-1709.884,-655.9253;Inherit;True;Property;_SpecularTexture;Specular Texture;4;0;Create;True;0;0;False;0;False;-1;2a0ee0866fd464e4b85928a591e4d625;0310cf39af27532468d205f8a6c9ebcf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;104;-1704.707,-454.1123;Float;False;Property;_Specular;Specular;3;0;Create;True;0;0;False;0;False;0;0.137;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-1707.861,-367.3922;Float;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;False;1;0.388;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-1333.66,-532.3452;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;132;-1112.708,-834.9487;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;76;-1720.486,-1208.143;Inherit;True;Property;_AlbedoTexture;Albedo Texture;0;0;Create;True;0;0;False;0;False;-1;a13d0741ba1b57640b1b4b035043021f;78e19074a8af01548b8086c7aa2050b4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0.905966,-641.6393;Float;False;True;-1;2;;0;0;StandardSpecular;Real Ivy/Ivy Low;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;1;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;133;0;103;3
WireConnection;133;1;131;0
WireConnection;108;0;106;0
WireConnection;108;1;104;0
WireConnection;132;0;103;1
WireConnection;132;1;103;2
WireConnection;132;2;133;0
WireConnection;0;0;76;0
WireConnection;0;1;132;0
WireConnection;0;3;108;0
WireConnection;0;4;107;0
WireConnection;0;10;76;4
ASEEND*/
//CHKSM=BC232406CCE2A82E1EFE067DCDDA30C1D53A2550