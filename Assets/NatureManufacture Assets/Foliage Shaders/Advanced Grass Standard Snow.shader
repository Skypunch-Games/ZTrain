Shader "NatureManufacture Shaders/Grass/Advanced Grass Standard Snow"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_HealthyColor("Healthy Color", Color) = (1,1,1,1)
		_DryColor("Dry Color", Color) = (0.875,0.8280551,0.7270221,1)
		_ColorNoiseSpread("Color Noise Spread", Float) = 15
		_MainTex("MainTex", 2D) = "white" {}
		[NoScaleOffset]_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Range( 0 , 2)) = 1
		[NoScaleOffset]_AmbientOcclusionG("AO (G) Sm (A)", 2D) = "white" {}
		_MetallicPower("Metallic Power", Range( 0 , 1)) = 0
		_AmbientOcclusionPower("Ambient Occlusion Power", Range( 0 , 1)) = 0
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
		_SnowMaxCover("Snow Max Cover", Range( 0 , 2)) = 0.35
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
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _TOUCHREACTACTIVE_ON
		#include "NM_indirect.cginc"
		#include "NMWind.cginc"
		#pragma instancing_options procedural:setup
		#pragma multi_compile GPU_FRUSTUM_ON __
		#pragma vertex vert
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
			half2 uv_texcoord;
			half ASEVFace : VFACE;
			float3 worldPos;
			float4 vertexColor : COLOR;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform half _BackFaceMirrorNormal;
		uniform half _BumpScale;
		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform half4 _HealthyColor;
		uniform half4 _DryColor;
		uniform half _ColorNoiseSpread;
		uniform half3 _WindColorInfluence;
		uniform half _WindColorThreshold;
		uniform sampler2D _SnowCover;
		uniform float4 _SnowCover_ST;
		uniform half _SnowColorBrightness;
		uniform half _SnowMaxCover;
		uniform half _Snow_Amount;
		uniform half _Snow_Min_Height;
		uniform half _Snow_Min_Height_Blending;
		uniform half _MetallicPower;
		uniform sampler2D _AmbientOcclusionG;
		uniform half _SmoothnessPower;
		uniform half _AmbientOcclusionPower;
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
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half3 tex2DNode4 = UnpackScaleNormal( tex2D( _BumpMap, uv0_MainTex ), _BumpScale );
			float3 switchResult439 = (((i.ASEVFace>0)?(tex2DNode4):(( tex2DNode4 * float3( -1,-1,-1 ) ))));
			o.Normal = lerp(tex2DNode4,switchResult439,_BackFaceMirrorNormal);
			float3 ase_worldPos = i.worldPos;
			float2 appendResult427 = (half2(ase_worldPos.x , ase_worldPos.z));
			float simplePerlin2D432 = snoise( ( appendResult427 / _ColorNoiseSpread ) );
			float4 lerpResult433 = lerp( _HealthyColor , _DryColor , simplePerlin2D432);
			half4 tex2DNode3 = tex2D( _MainTex, uv0_MainTex );
			float clampResult437 = clamp( i.vertexColor.r , 0.0 , 1.0 );
			float3 lerpResult465 = lerp( ( float3( 1,1,1 ) - _WindColorInfluence ) , ( float3( 1,1,1 ) + _WindColorInfluence ) , pow( clampResult437 , _WindColorThreshold ));
			float2 uv0_SnowCover = i.uv_texcoord * _SnowCover_ST.xy + _SnowCover_ST.zw;
			float3 newWorldNormal475 = (WorldNormalVector( i , tex2DNode4 ));
			float temp_output_495_0 = ( ( 1.0 - _Snow_Min_Height ) + ase_worldPos.y );
			float clampResult502 = clamp( ( temp_output_495_0 + 1.0 ) , 0.0 , 1.0 );
			float clampResult503 = clamp( ( ( 1.0 - ( ( temp_output_495_0 + _Snow_Min_Height_Blending ) / temp_output_495_0 ) ) + -0.5 ) , 0.0 , 1.0 );
			float clampResult505 = clamp( ( clampResult502 + clampResult503 ) , 0.0 , 1.0 );
			float4 lerpResult483 = lerp( ( ( lerpResult433 * tex2DNode3 ) * half4( lerpResult465 , 0.0 ) ) , ( tex2D( _SnowCover, uv0_SnowCover ) * _SnowColorBrightness ) , saturate( abs( ( ( newWorldNormal475.x + ( newWorldNormal475.y * _SnowMaxCover ) + newWorldNormal475.z ) * ( _Snow_Amount * clampResult505 ) ) ) ));
			o.Albedo = lerpResult483.rgb;
			o.Metallic = _MetallicPower;
			half4 tex2DNode98 = tex2D( _AmbientOcclusionG, uv0_MainTex );
			o.Smoothness = ( tex2DNode98.a * _SmoothnessPower );
			float clampResult150 = clamp( tex2DNode98.g , ( 1.0 - _AmbientOcclusionPower ) , 1.0 );
			o.Occlusion = clampResult150;
			o.Alpha = 1;
			clip( ( ( 1.0 - saturate( ( ( distance( ase_worldPos , _WorldSpaceCameraPos ) - _CullFarStart ) / _CullFarDistance ) ) ) * tex2DNode3.a ) - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				AdditionalWind(v);
				Input customInputData;
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
				o.color = v.color;
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
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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