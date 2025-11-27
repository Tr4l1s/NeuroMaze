// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Real Ivy/Flat leaves simple"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_AlbedoTexture("Albedo Texture", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
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
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _WindPattern;
		uniform float _Frequency;
		uniform float _Amplitude;
		uniform float _Center;
		uniform float _Radius;
		uniform float4 _Color;
		uniform sampler2D _AlbedoTexture;
		uniform float4 _AlbedoTexture_ST;
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
			float2 uv_AlbedoTexture = i.uv_texcoord * _AlbedoTexture_ST.xy + _AlbedoTexture_ST.zw;
			float4 tex2DNode76 = tex2D( _AlbedoTexture, uv_AlbedoTexture );
			o.Albedo = ( _Color * tex2DNode76 ).rgb;
			o.Alpha = 1;
			clip( tex2DNode76.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18707
51;315;1706;986;1151.664;1426.309;1;True;True
Node;AmplifyShaderEditor.SimpleTimeNode;65;-3970.744,-250.4898;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-3969.887,-176.9988;Float;False;Property;_Frequency;Frequency;3;0;Create;True;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-3717.07,-225.1368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-3725.737,-76.67069;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;100;-3508.737,-124.6707;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;88;-3662.847,-418.0225;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;94;-3342.102,-224.5218;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;87;-3344.573,-394.1266;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-2558.526,131.9046;Float;False;Property;_Center;Center;5;0;Create;True;0;0;False;0;False;0;-0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;59;-2559.701,48.15817;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;-2938.709,-271.5038;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-2312.713,-121.8755;Float;False;Property;_Amplitude;Amplitude;4;0;Create;True;0;0;False;0;False;1;0.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;85;-2479.124,-346.8083;Inherit;True;Property;_WindPattern;Wind Pattern;7;0;Create;True;0;0;False;0;False;-1;2d15e4a275691a645ab383c7c5724d67;2d15e4a275691a645ab383c7c5724d67;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;62;-2560.129,239.3232;Float;False;Constant;_Float5;Float 5;1;0;Create;True;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-2346.226,85.32156;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;60;-2189.929,171.6869;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-2080.544,-204.4523;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1902.379,-40.07847;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;21;-1612.395,119.3297;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-1627.301,229.0952;Float;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;False;0;False;-1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TangentVertexDataNode;25;-1782.183,596.5594;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;26;-1780.596,416.7985;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;33;-1241.802,685.8445;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;13;-1066.285,-323.6609;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;15;-969.5804,-99.77255;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-1392.97,126.7804;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CosOpNode;52;-1070.412,-421.5802;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-1359.162,-62.95768;Float;False;Property;_Radius;Radius;6;0;Create;True;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CrossProductOpNode;24;-1333.071,494.9755;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-941.5338,327.8264;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-613.9263,-352.6164;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;76;-1720.486,-1208.143;Inherit;True;Property;_AlbedoTexture;Albedo Texture;1;0;Create;True;0;0;False;0;False;-1;a13d0741ba1b57640b1b4b035043021f;78e19074a8af01548b8086c7aa2050b4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;102;-854.4099,-1342.493;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-382.6417,-249.7737;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-515.8284,-1191.559;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0.905966,-641.6393;Float;False;True;-1;2;;0;0;StandardSpecular;Real Ivy/Flat leaves simple;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;1;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;85;1;90;0
WireConnection;73;0;59;0
WireConnection;73;1;72;0
WireConnection;60;0;73;0
WireConnection;60;1;62;0
WireConnection;69;0;85;0
WireConnection;69;1;68;0
WireConnection;63;0;69;0
WireConnection;63;1;60;0
WireConnection;21;0;63;0
WireConnection;58;0;21;0
WireConnection;58;1;57;0
WireConnection;52;0;63;0
WireConnection;24;0;26;0
WireConnection;24;1;25;0
WireConnection;31;0;58;0
WireConnection;31;1;24;0
WireConnection;31;2;74;0
WireConnection;31;3;33;4
WireConnection;10;0;52;0
WireConnection;10;1;13;0
WireConnection;10;2;74;0
WireConnection;10;3;15;4
WireConnection;37;0;10;0
WireConnection;37;1;31;0
WireConnection;103;0;102;0
WireConnection;103;1;76;0
WireConnection;0;0;103;0
WireConnection;0;10;76;4
WireConnection;0;11;37;0
ASEEND*/
//CHKSM=99160414E96157AEB1C18966886632A7FCA9860E