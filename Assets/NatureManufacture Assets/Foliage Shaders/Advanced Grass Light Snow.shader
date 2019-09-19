Shader "NatureManufacture Shaders/Grass/Advanced Grass Light Snow"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_HealthyColor("Healthy Color", Color) = (1,1,1,1)
		_DryColor("Dry Color", Color) = (0.875,0.8280551,0.7270221,1)
		_ColorNoiseSpread("Color Noise Spread", Float) = 15
		_MainTex("MainTex", 2D) = "white" {}
		_MetallicPower("Metallic Power", Range( 0 , 1)) = 0
		_SmoothnessPower("Smoothness Power", Range( 0 , 2)) = 0
		_NewNormal("Vertex Normal Multiply", Vector) = (0,0,0,0)
		_InitialBend("Wind Initial Bend", Float) = 1
		_Stiffness("Wind Stiffness", Float) = 1
		_Drag("Wind Drag", Float) = 1
		_ShiverDrag("Wind Shiver Drag", Float) = 0.05
		_ShiverDirectionality("Wind Shiver Directionality", Range( 0 , 1)) = 0.5
		_WindColorInfluence("Wind Color Influence", Vector) = (0,0,0,0)
		_WindColorThreshold("Wind Color Threshold", Float) = 1
		_WindNormalInfluence("Wind Normal Influence", Float) = 0
		_CullFarDistance("CullFarDistance", Range( 0 , 10000)) = 5
		_CullFarStart("CullFarStart", Range( 0 , 10000)) = 40
		[Toggle]_BackFaceMirrorNormal("BackFace Mirror Normal", Float) = 0
		_Snow_Amount("Snow_Amount", Range( 0 , 2)) = 0.13
		_SnowCover("Snow Cover", 2D) = "white" {}
		_SnowColorBrightness("Snow Color Brightness", Range( 0 , 2)) = 1
		_Snow_Min_Height("Snow_Min_Height", Range( -1000 , 10000)) = -1000
		_Snow_Min_Height_Blending("Snow_Min_Height_Blending", Range( 0 , 500)) = 1
		[Toggle(_TOUCHREACTACTIVE_ON)] _TouchReactActive("TouchReactActive", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _TOUCHREACTACTIVE_ON
		#include "NM_indirect.cginc"
		#include "NMWind.cginc"
		#pragma instancing_options procedural:setup
		#pragma multi_compile GPU_FRUSTUM_ON __
		#pragma vertex vert
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half ASEVFace : VFACE;
			float3 worldPos;
			half2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform half _BackFaceMirrorNormal;
		uniform half4 _HealthyColor;
		uniform half4 _DryColor;
		uniform half _ColorNoiseSpread;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform half3 _WindColorInfluence;
		uniform half _WindColorThreshold;
		uniform sampler2D _SnowCover;
		uniform float4 _SnowCover_ST;
		uniform half _SnowColorBrightness;
		uniform half _Snow_Amount;
		uniform half _Snow_Min_Height;
		uniform half _Snow_Min_Height_Blending;
		uniform half _MetallicPower;
		uniform half _SmoothnessPower;
		uniform half _CullFarStart;
		uniform half _CullFarDistance;
		uniform float _Cutoff = 0.5;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 switchResult439 = (((i.ASEVFace>0)?(half3(0,0,1)):(half3(0,0,-1))));
			o.Normal = lerp(float3( 0,0,1 ),switchResult439,_BackFaceMirrorNormal);
			float3 ase_worldPos = i.worldPos;
			float2 appendResult427 = (half2(ase_worldPos.x , ase_worldPos.z));
			float simplePerlin2D432 = snoise( ( appendResult427 / _ColorNoiseSpread ) );
			float4 lerpResult433 = lerp( _HealthyColor , _DryColor , simplePerlin2D432);
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half4 tex2DNode3 = tex2D( _MainTex, uv0_MainTex );
			float clampResult437 = clamp( i.vertexColor.r , 0.0 , 1.0 );
			float3 lerpResult465 = lerp( ( float3( 1,1,1 ) - _WindColorInfluence ) , ( float3( 1,1,1 ) + _WindColorInfluence ) , pow( clampResult437 , _WindColorThreshold ));
			float2 uv0_SnowCover = i.uv_texcoord * _SnowCover_ST.xy + _SnowCover_ST.zw;
			float temp_output_495_0 = ( ( 1.0 - _Snow_Min_Height ) + ase_worldPos.y );
			float clampResult502 = clamp( ( temp_output_495_0 + 1.0 ) , 0.0 , 1.0 );
			float clampResult503 = clamp( ( ( 1.0 - ( ( temp_output_495_0 + _Snow_Min_Height_Blending ) / temp_output_495_0 ) ) + -0.5 ) , 0.0 , 1.0 );
			float clampResult505 = clamp( ( clampResult502 + clampResult503 ) , 0.0 , 1.0 );
			float4 lerpResult483 = lerp( ( ( lerpResult433 * tex2DNode3 ) * half4( lerpResult465 , 0.0 ) ) , ( tex2D( _SnowCover, uv0_SnowCover ) * _SnowColorBrightness ) , saturate( abs( ( _Snow_Amount * clampResult505 ) ) ));
			o.Albedo = lerpResult483.rgb;
			o.Metallic = _MetallicPower;
			o.Smoothness = _SmoothnessPower;
			o.Alpha = 1;
			clip( ( ( 1.0 - saturate( ( ( distance( ase_worldPos , _WorldSpaceCameraPos ) - _CullFarStart ) / _CullFarDistance ) ) ) * tex2DNode3.a ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}