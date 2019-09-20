Shader "Enviro/Standard/WeatherMap" {
	Properties {
		_Coverage ("Coverage", Range(0,1)) = 0.5
		_Tiling ("Tiling", Range(1,100)) = 10
	}
	SubShader { 
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass { 
		CGPROGRAM
	    #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        #include "/Core/EnviroNoiseCore.cginc"
		#pragma target 3.0
		#pragma exclude_renderers gles 


	#define CLASSICPERLIN

		sampler2D _MainTex;

		   struct VertexInput {
  				half4 vertex : POSITION;
 				float2 uv : TEXCOORD0;	
            };

            struct VertexOutput {
           		float4 position : SV_POSITION;
 				float2 uv : TEXCOORD0;
            }; 
			          
            VertexOutput vert (VertexInput v) {
 			 	VertexOutput o;
 				o.position = UnityObjectToClipPos(v.vertex);				
 				o.uv = v.uv;
 				return o; 
            }       
 		     
 			float4x4 world_view_proj;

 			float _Coverage;  
			float _CloudsType;
			float _CoverageType;
 			int _Tiling;
 			float2 _WindDir;
			float2 _Location; 
 			float _AnimSpeedScale;

			float set_range(float value, float low, float high) {
							return saturate((value - low)/(high - low));
			}    

			float remap(float value, float original_min, float original_max, float new_min, float new_max)
			{
  			  return new_min + saturate(((value - original_min) / (original_max - original_min)) * (new_max - new_min));
			}

			float dilate_perlin_worley(float p, float w, float x) {
				float curve = 0.75;
				if (x < 0.5) {
					x = x / 0.5;
					float n = p + w * x;
					return n * lerp(1, 0.5, pow(x, curve));
				}
				else {  
					x = (x - 0.5) / 0.5;
					float n = w + p * (1.0 - x);
					return n * lerp(0.5, 1.0, pow(x, 1.0 / curve));
				}  
			}  
		            

 			float4 frag(VertexInput input) : SV_Target 
 			{
				float2 xy_offset = _WindDir * 10 * _AnimSpeedScale;
 				float2 xy_offset1 = xy_offset;
				float2 xy_offset2 = xy_offset + float2(50,100);
				float2 xy_offset3 = xy_offset + float2(100, 50);

 				float2 sampling_pos1 = float2(input.uv + xy_offset1 + _Location) * 2.0 * _Tiling;
				float2 sampling_pos2 = float2(input.uv + xy_offset1 + _Location) * 3.0 *_Tiling;
				float2 sampling_pos3 = float2(input.uv + xy_offset2 + _Location) * 4.0 * _Tiling;
				float2 sampling_pos4 = float2(input.uv + xy_offset2 + _Location) * 6.0 * _Tiling;
				float2 sampling_pos5 = float2(input.uv + xy_offset3 + _Location) * 4.0 * _Tiling;

				// Calculate our perlin noise for red channel	
				float perlin = CalculatePerlin5(sampling_pos1.xy);
				 
				perlin = perlin + _Coverage;
				perlin = pow(perlin * 1.5, 1);
				perlin = clamp(perlin, 0, 1);

				float coverage1 = perlin;
				
				// Calculate our worley noise
				float worley = CalculateWorley3oct(sampling_pos2.xy,0.9,0.3,0.8);				
				worley = worley - (worley * (1 - _Coverage));
				worley = saturate(worley);

				float perlin_worley_coverage = dilate_perlin_worley(perlin, worley, _CoverageType);

				float perlin2 = CalculatePerlin5(sampling_pos3);
				perlin2 = perlin2;
				perlin2 = pow(perlin2 * 2, 1);
				perlin2 = saturate(perlin2);

				//Get perlin worley noise for clouds types
				float worley2 = CalculateWorley3oct(sampling_pos4.xy,0.3, 0.8, 1.3);
				worley2 = saturate(worley2); 

				float worley3 = CalculateWorley3oct(sampling_pos5.xy, 0.3, 0.4, 0.9);
				worley3 = saturate(worley3 * 2);

				worley2 -= worley3 * perlin_worley_coverage;
				
				float perlin_worley_type = saturate(_CloudsType - 0.5) + dilate_perlin_worley(perlin2 * 0.75, (0.5 - perlin2) + (worley2 - 0.2), 0.35) * _CloudsType;
		
				return float4(perlin_worley_coverage, 0, perlin_worley_type, 0);
			}

	ENDCG
	}
	}
	FallBack "Diffuse"
}
