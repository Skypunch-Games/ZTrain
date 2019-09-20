Shader "Enviro/Standard/RaymarchClouds"
{
	Properties
	{ 
		    
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Tags{ "RenderType" = "Opaque" }

		Pass
	{
		CGPROGRAM
	#pragma vertex vert    
	#pragma fragment frag
	#pragma target 3.0
	#pragma exclude_renderers gles 
	#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
	#include "UnityCG.cginc"
	#include "../../../Core/Resources/Shaders/Core/EnviroFogCore.cginc"
	#include "Core/EnviroVolumeCloudsCore.cginc"
		                 
		struct appdata
	{ 
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 sky : TEXCOORD1;
		float4 screenPos : TEXCOORD2;
	};


	v2f vert(appdata_img v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.position = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		o.sky.x = saturate(_SunDir.y + 0.25);
		o.sky.y = saturate(clamp(1.0 - _SunDir.y, 0.0, 0.5));
		o.screenPos = ComputeScreenPos(o.position);
		return o;
	}

	float4 frag(v2f i) : SV_Target
	{

		float4 cameraRay = float4(i.uv * 2.0 - 1.0, 1.0, 1.0);
		//World Space
		float3 EyePosition = _WorldSpaceCameraPos;
		//Workaround for large scale games where player position will be resetted.
		//float3 EyePosition = float3(0.0,_WorldSpaceCameraPos.y, 0.0);

		float3 ray = 0;

#if UNITY_SINGLE_PASS_STEREO
		if (unity_StereoEyeIndex == 0)
		{
			cameraRay = mul(_InverseProjection, cameraRay);
			cameraRay = cameraRay / cameraRay.w;
			ray = normalize(mul((float3x3)_InverseRotation, cameraRay.xyz));
		}
		else
		{
			cameraRay = mul(_InverseProjection_SP, cameraRay);
			cameraRay = cameraRay / cameraRay.w;
			ray = normalize(mul((float3x3)_InverseRotation_SP, cameraRay.xyz));
		}
#else    
		cameraRay = mul(_InverseProjection, cameraRay);
		cameraRay = cameraRay / cameraRay.w;
		ray = normalize(mul((float3x3)_InverseRotation, cameraRay.xyz));
#endif  

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv));
		float dpth = Linear01Depth(rawDepth);

		float3 wsDir = dpth * ray;
		float3 viewDir = normalize(wsDir);
		float3 wsPos = wsDir - EyePosition;

		//float4 sky = ComputeScattering(viewDir, i.sky.xy);
		float4 sky = ComputeScatteringClouds(viewDir, i.sky.xy, _gameTime);
		float4 color = 0;

		float3 LightDirection = _LightDir;
		float3 LightColor = _LightColor.rgb;

		//Switch to Moon Light Color  
		if (_gameTime > 0.55)
			LightColor = _MoonLightColor.rgb;

		float pRad = _CloudsParameter.w;
		float3 pCent = float3(EyePosition.x, -pRad, EyePosition.z);

		float2 hitDistBottom = ComputeBothSphereIntersections(EyePosition, ray, pCent, _CloudsParameter.w + _CloudsParameter.x);
		float2 hitDistTop = ComputeBothSphereIntersections(EyePosition, ray, pCent, _CloudsParameter.w + _CloudsParameter.y);

		float2 hitDistance;

		float ch = length(EyePosition - pCent) - _CloudsParameter.w;

		if (ch < _CloudsParameter.x)
		{
			hitDistance = float2(hitDistBottom.y, hitDistTop.y);

			if (ray.y < 0.0)
				return float4(0, 0, 0, 0);
		}
		else if (ch > _CloudsParameter.y)
		{
			if (hitDistBottom.x > -env_inf)
				hitDistance = float2(hitDistTop.x, hitDistBottom.x);
			else
				hitDistance = float2(0.0, -1.0);

		}
		else
		{
			if (hitDistBottom.x < 0.0)
				hitDistance = float2(0.0, hitDistTop.y);
			else
				hitDistance = float2(0.0, hitDistBottom.x);
		}

		hitDistance.x = max(0.0, hitDistance.x);

		//clip(hitDistance.y - hitDistance.x);

		int steps = (int)lerp(_Steps.x*1.2, _Steps.x, ray.y);

		if ((EyePosition.y < _CloudsParameter.y - (_CloudsParameter.y * 0.1)) && (EyePosition.y > _CloudsParameter.x - (_CloudsParameter.x * 0.25)))
		{
			float reducedDistance = 400 * (1.0 + hitDistance.x) / (1 * lerp(1.0, 0.025, smoothstep(-0.2, -0.6, 1)));
			hitDistance.y = min(hitDistance.y, hitDistance.x + reducedDistance);
		}

		/////       
		float inScatteringAngle = dot(normalize(LightDirection), normalize(ray));
		float rayStepLength = 1 * (hitDistance.y - hitDistance.x) / steps;
		float3 rayStep = ray * rayStepLength;
		float offset = getRandomRayOffset((i.uv + _Randomness.xy) * _ScreenParams.xy * _BlueNoise_TexelSize.xy);
		float3 pos = EyePosition + (hitDistance.x + offset * rayStepLength) * ray;
		////    
		float extinct = 1.0;
		float opticalDepth = 0.0;
		float cloud_test = 0.0;
		int zero_density_sample_count = 0;
		float sampled_density_previous = -1.0;

		// Reduce steps when rendering behind objects. 
		if (dpth < 1)
			steps *= _stepsInDepth;

		//Raymarching     
		[loop]
		for (int i = 0; i < steps; i++)
		{          
			pos += rayStep;
			//Calculate projection height
			float height = GetSamplingHeight(pos, pCent);
			 
			//Get out of expensive raymarching
			if (extinct < 0.1 || height > 1.0 || height < 0.0 || _CloudsCoverageSettings.x <= -0.9)
				break;

			// Get Weather Data           
			float3 weather = GetWeather(pos);

			if (cloud_test > 0.0)
			{
				float sampled_density = saturate(CalculateCloudDensity(pos, pCent, weather, 0, ray.y, true) * _CloudDensityScale.x);

				if (sampled_density == 0.0 && sampled_density_previous == 0.0)
				{
					zero_density_sample_count++;
				}

				if (zero_density_sample_count < 11 && sampled_density != 0.0)
				{
					///Density 
					float2 dl = GetDensityAlongRay(pos, pCent, LightDirection, weather, ray.y, height) * _CloudDensityScale.y;
					float currentOpticalDepth = sampled_density  * rayStepLength;
					opticalDepth += currentOpticalDepth;
					extinct = Beer(opticalDepth);

					//// LIGHTING
					float hg = max(PhaseHenyeyGreenStein(inScatteringAngle, _CloudsLighting.y), (_CloudsLighting.z) * PhaseHenyeyGreenStein(inScatteringAngle, 0.99 - _CloudsLighting.w));
					float energy = GetLightEnergy(pos, height, dl, sampled_density * 1.5, hg, inScatteringAngle, rayStepLength, _CloudsLighting.x * 10, ray.y);
					float3 sunLight = pow(LightColor,2) * _LightIntensity;
					float lightIntensity = energy * extinct;
					sunLight.rgb = sunLight.rgb * lightIntensity;
					////        
					color.rgb += sunLight.rgb;

				}
				// if not, then set cloud_test to zero so that we go back to the cheap sample case
				else
				{
					cloud_test = 0.0;
					zero_density_sample_count = 0;
				}

				sampled_density_previous = sampled_density;
			}
			else
			{
				// sample density the cheap way, only using the low frequency noise
				cloud_test = CalculateCloudDensity(pos, pCent, weather, 0, ray.y, false) * _CloudDensityScale.x;

				if (cloud_test == 0.0)
				{
					pos += rayStep * 2;
				}
				else  //take a step back and capture area we skipped.
				{
					pos -= rayStep;
				}
			}
		}

		color.a = 1 - GetAlpha(opticalDepth);

		// Ambient Lighting:
		color = color + float4(sky.rgb * _AmbientSkyColorIntensity * _CloudsLightingExtended.y, 0) * saturate(1 - color);


		//Tonemapping
		if (_CloudsLightingExtended.z == 0)
		{
			color.rgb = tonemapACES(color.rgb, _CloudsLightingExtended.w);
		}

#if defined(UNITY_COLORSPACE_GAMMA)
		color.rgb = LinearToGammaSpace(color.rgb);
#endif

		return color;
	}
		ENDCG
	}
	}
}
