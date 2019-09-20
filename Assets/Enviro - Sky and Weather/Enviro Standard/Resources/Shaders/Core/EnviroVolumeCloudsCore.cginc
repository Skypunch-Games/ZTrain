
uniform float4x4 _InverseProjection;
uniform float4x4 _InverseRotation;
uniform float4x4 _InverseProjection_SP;
uniform float4x4 _InverseRotation_SP;

uniform sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;
uniform sampler3D _Noise;
uniform sampler3D _NoiseLow;
uniform sampler3D _DetailNoise;
uniform sampler2D _WeatherMap;
uniform sampler2D _CurlNoise;
uniform sampler2D _BlueNoise;
uniform float4 _BlueNoise_TexelSize;

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

uniform float4 _CloudsParameter;
uniform float4 _Steps;
uniform float4 _CloudsLighting; //x = ExtinctionCoef, y = HgPhaseFactor, z = Silver_intensity, w = Silver_spread
uniform float4 _CloudsLightingExtended; // x = EdgeDarkness, y = AmbientSkyColorIntensity, z = _Tonemapping, w = _CloudsExposure
uniform float4 _CloudsErosionIntensity; //x = Base, y = Detail
uniform float _BaseNoiseUV;
uniform float _DetailNoiseUV;
uniform float4 _CloudDensityScale;
uniform float _LightIntensity;
uniform float _AmbientSkyColorIntensity;

uniform float4 _CloudsCoverageSettings; //x = _GlobalCoverage, y = Bottom Coverage Mod, z = Top coverage mod, w = Clouds Up Morph Intensity
uniform float _GlobalCoverage;

uniform float4 _LightColor;
uniform float4 _MoonLightColor;
uniform float4 _CloudsAnimation;
uniform float3 _LightDir;
uniform float _stepsInDepth;
uniform float _LODDistance;
uniform float _gameTime;
////
uniform float4 _Randomness;
////

const float env_inf = 1e10;
const float3 RandomUnitSphere[6] = { { 0.452, -0.679, 0.156 },{ 0.2122, -0.363, -0.2324 },{ -0.934, 0.374, -0.1356 },{ -0.556, 0.553, 0.498 },{ -1.0, 0.345, 0.212 },{ 0.343, -0.998, 0.441 } };

half3 tonemapACES(half3 color, float Exposure)
{
	color *= Exposure;

	// See https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
	const half a = 2.51;
	const half b = 0.03;
	const half c = 2.43;
	const half d = 0.59;
	const half e = 0.14;
	return saturate((color * (a * color + b)) / (color * (c * color + d) + e));
}

float2 raySphereIntersect(float3 ro, float3 rd, float3 so, float radius)
{
	float3 D = ro - so;
	float b = dot(rd, D);
	float c = dot(D, D) - radius * radius;
	float Delta = b * b - c;
	if (Delta < 0.0) {
		return float2(-env_inf, env_inf);
	}
	Delta = sqrt(Delta);
	return float2(-b - Delta, -b + Delta);
}

float2 ComputeBothSphereIntersections(float3 _Pos, float3 _Direction, float3 _SphereCenter, float _SphereRadius)
{
	float3 D = _Pos - _SphereCenter;
	float b = dot(_Direction, D);
	float c = dot(D, D) - _SphereRadius*_SphereRadius;
	float Delta = b*b - c;
	float SqrtDelta = sqrt(Delta);
	return lerp(float2(-b - SqrtDelta, -b + SqrtDelta), float2(+env_inf, -env_inf), saturate(-10000.0 * Delta));
}

/*
// Realtime Volumetric Rendering Course Notes by Patapom (page 15)
float exponential_integral(float z) {
return 0.5772156649015328606065 + log(1e-4 + abs(z)) + z * (1.0 + z * (0.25 + z * ((1.0 / 18.0) + z * ((1.0 / 96.0) + z * (1.0 / 600.0))))); // For x!=0
}


// Realtime Volumetric Rendering Course Notes by Patapom (page 15)
float3 CalculateAmbientLighting(float altitude, float extinction_coeff, float4 skyColor)
{
float ambient_term = 0.6 * saturate(1.0 - altitude);
float3 isotropic_scattering_top = (skyColor.rgb * _CloudTopColor * _LightColor) * max(0.0, exp(ambient_term) - ambient_term * exponential_integral(ambient_term));

ambient_term = -extinction_coeff * altitude;
float3 isotropic_scattering_bottom = skyColor.rgb * _CloudBaseColor * max(0.0, exp(ambient_term) - ambient_term * exponential_integral(ambient_term)) * 1.5;

isotropic_scattering_top *= saturate(altitude);

return (isotropic_scattering_top)+(isotropic_scattering_bottom);
}

float Beer(float opticalDepth)
{
return max(exp(-opticalDepth), (exp(-opticalDepth * 0.005f) * 0.7));
}
*/
float PhaseHenyeyGreenStein(float inScatteringAngle, float g)
{
	return ((1.0 - g * g) / pow((1.0 + g * g - 2.0 * g * inScatteringAngle), 3.0 / 2.0)) / (4.0 * 3.14159);
}

float Beer(float opticalDepth)
{
	return exp(-opticalDepth * 0.0025f) * 0.7;
}

float getRandomRayOffset(float2 uv) // uses blue noise texture to get random ray offset
{
	float noise = tex2D(_BlueNoise, uv).x;
	noise = mad(noise, 2.0, -1.0);
	return noise;
}


float GetAlpha(float opticalDepth)
{
	return exp(-2 * 0.005f * opticalDepth);
}

float Remap(float org_val, float org_min, float org_max, float new_min, float new_max)
{
	return new_min + saturate(((org_val - org_min) / (org_max - org_min))*(new_max - new_min));
}

float3 decode_curl(float3 c) {
	return (c - 0.5) * 2.0;
}

float3 get_curl_offset(float3 pos, float curl_amplitude, float curl_frequency, float altitude) {
	float4 curl_data = tex2Dlod(_CurlNoise, float4(pos.xy * curl_frequency, 0, 0));
	return decode_curl(curl_data.rgb) * curl_amplitude * (1.0 - altitude * 0.5);
}

float4 GetHeightGradient(float cloudType)
{
	const float4 CloudGradient1 = float4(0.0, 0.05, 0.1, 0.25);
	const float4 CloudGradient2 = float4(0.0, 0.05, 0.4, 0.8);
	const float4 CloudGradient3 = float4(0.0, 0.05, 0.6, 1.0);

	float a = 1.0 - saturate(cloudType * 2.0);
	float b = 1.0 - abs(cloudType - 0.5) * 2.0;
	float c = saturate(cloudType - 0.5) * 2.0;

	return CloudGradient1 * a + CloudGradient2 * b + CloudGradient3 * c;
}

float GradientStep(float a, float4 gradient)
{
	return smoothstep(gradient.x, gradient.y, a) - smoothstep(gradient.z, gradient.w, a);
}

float3 GetWeather(float3 pos)
{
	float2 uv = pos.xz * 0.00001 + 0.5;
	return tex2Dlod(_WeatherMap, float4(uv, 0.0, 0.0));
}

float GetSamplingHeight(float3 pos, float3 center)
{
	return (length(pos - center) - (_CloudsParameter.w + _CloudsParameter.x)) / _CloudsParameter.z;
}


float set_range_clamped(float value, float low, float high) {
	float ranged_value = clamp(value, low, high);
	ranged_value = (ranged_value - low) / (high - low);
	return saturate(ranged_value);
}

float get_fade_term(float3 sample_pos) {
	float distance = length(sample_pos.xy);
	return saturate((distance - 5000) / 20000.0);
}

// Sample Cloud Density
float CalculateCloudDensity(float3 pos, float3 PlanetCenter, float3 weather, float mip, float dist, bool details)
{
	const float baseFreq = 1e-5;

	// Get Height fraction
	float height = GetSamplingHeight(pos, PlanetCenter);

	// wind settings
	float cloud_top_offset = 2000.0;
	float3 wind_direction = float3(_CloudsAnimation.z, 0.0, _CloudsAnimation.w);

	// skew in wind direction
	pos += height * wind_direction * cloud_top_offset;

	float4 coord = float4(pos * baseFreq * _BaseNoiseUV, mip);
	// Animate Wind
	coord.xyz += float3(_CloudsAnimation.x, _CloudsErosionIntensity.w, _CloudsAnimation.y);

	float4 baseNoise = 0;

	if (dist > _LODDistance)
		baseNoise = tex3Dlod(_Noise, coord);
	else
		baseNoise = tex3Dlod(_NoiseLow, coord);

	float low_freq_fBm = (baseNoise.g * 0.625) + (baseNoise.b * 0.25) + (baseNoise.a * 0.125);
	float base_cloud = Remap(baseNoise.r, -(1.0 - low_freq_fBm) * _CloudsErosionIntensity.x, 1.0, 0.0, 1.0);

	float heightGradient = GradientStep(height, GetHeightGradient(weather.b));
	base_cloud *= heightGradient;

	float cloud_coverage = saturate(1 - weather.r);
	   
	//cloud_coverage = pow(cloud_coverage, Remap((1 - height), 0.7, 0.8, 1.0, lerp(1.0, 0.5, _CloudsCoverageSettings.z)));
	cloud_coverage = pow(cloud_coverage, Remap((height), 0.7, 0.8, 1.0, lerp(1.0, 0.5, _CloudsCoverageSettings.y)));

	float cloudDensity = Remap(base_cloud, cloud_coverage, 1.0, 0.0, 1.0);

	cloudDensity = Remap(cloudDensity, saturate(height * 0.75 / _CloudsCoverageSettings.z), 1.0, 0.0, 1.0);
	  
	//cloudDensity *= cloudDensity;
	cloudDensity *= weather.r;
	 
	//DETAIL
	[branch]
	if (details)
	{ 		
		coord = float4(pos * baseFreq * _DetailNoiseUV, mip);		
		coord.xyz += float3(_CloudsAnimation.x, _CloudsErosionIntensity.w, _CloudsAnimation.y);
		//coord.xyz += get_curl_offset(coord, 0.05, 10, height);
		float3 detailNoise = tex3Dlod(_DetailNoise, coord).rgb;
		float high_freq_fBm = (detailNoise.r * 0.625) + (detailNoise.g * 0.25) + (detailNoise.b * 0.125);
		float high_freq_noise_modifier = lerp(high_freq_fBm, 1.0f - high_freq_fBm, saturate(height * 20));
		cloudDensity = Remap(cloudDensity, high_freq_noise_modifier * _CloudsErosionIntensity.y, 1.0, 0.0, 1.0);
	}
	return saturate(cloudDensity * lerp(1, 0.75, dist));
}


// Lighting Energy Function
float GetLightEnergy(float3 p, float height_fraction, float dl, float ds_loded, float phase_probability, float cos_angle, float step_size, float brightness, float view)
{
	float prim_att = exp(-dl);
	float sec_att = max(exp(-dl), exp(-dl * 0.25) * 0.7);
	float attenuation_probability = lerp(prim_att, sec_att * 0.5, 1 - cos_angle);
	float vertical_probability = pow(Remap(height_fraction, 0.07, 0.14, 0.3, 1.0), 0.8);
	float depth_probability = lerp(0.05 + pow(ds_loded, Remap(height_fraction, 0.3, 0.85, _CloudsLightingExtended.x * 0.25, _CloudsLightingExtended.x * 2)), 1.0, saturate(dl / step_size));
	float in_scatter_probability = depth_probability * vertical_probability;
	float light_energy = attenuation_probability * in_scatter_probability * phase_probability * brightness;
	return light_energy;
}

// Lighting Sample Function
float GetDensityAlongRay(float3 pos, float3 PlanetCenter, float3 LightDirection, float3 weather, float dist, float h)
{
	float sunRayStepLength = _CloudsParameter.z / 6;
	float3 sunRayStep = (LightDirection * sunRayStepLength) * 0.1;

	float opticalDepth = 0.0;
	//pos += sunRayStep;

	//float densMult = 1;
	[loop]
	for (int i = 0; i < 6; i++)
	{
		float cone_spread_multplier = length(sunRayStep);

		pos += sunRayStep + (cone_spread_multplier * RandomUnitSphere[i] * float(i + 1));

		int mip_offset = int(i*0.5);

		if (opticalDepth < 0.3)
			opticalDepth += CalculateCloudDensity(pos, PlanetCenter, weather, mip_offset, dist, true);
		else
			opticalDepth += CalculateCloudDensity(pos, PlanetCenter, weather, mip_offset, dist, false);

		if (i == 4)
		{
			sunRayStep *= 4;
		//	densMult = 4;
		}

		pos += sunRayStep;
	}
	return saturate(opticalDepth);
}