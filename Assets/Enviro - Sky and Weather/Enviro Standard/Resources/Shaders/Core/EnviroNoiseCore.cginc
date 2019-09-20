
			float3 interpolation_c2( float3 x ) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }

			float3 mod(float3 x, float3 y)
			{
				return x - y * floor(x / y);
			}

			float3 mod289(float3 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float4 mod289(float4 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float4 permute(float4 x)
			{
				return mod289(((x*34.0) + 1.0)*x);
			}

			float4 taylorInvSqrt(float4 r)
			{
				return (float4)1.79284291400159 - r * 0.85373472095314;
			}

			float3 fade(float3 t) {
				return t*t*t*(t*(t*6.0 - 15.0) + 10.0);
			}

			// Classic Perlin noise
			float cnoise(float3 P)
			{
				float3 Pi0 = floor(P); // Integer part for indexing
				float3 Pi1 = Pi0 + (float3)1.0; // Integer part + 1
				Pi0 = mod289(Pi0);
				Pi1 = mod289(Pi1);
				float3 Pf0 = frac(P); // Fractional part for interpolation
				float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
				float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
				float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
				float4 iz0 = (float4)Pi0.z;
				float4 iz1 = (float4)Pi1.z;

				float4 ixy = permute(permute(ix) + iy);
				float4 ixy0 = permute(ixy + iz0);
				float4 ixy1 = permute(ixy + iz1);

				float4 gx0 = ixy0 / 7.0;
				float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
				gx0 = frac(gx0);
				float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
				float4 sz0 = step(gz0, (float4)0.0);
				gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
				gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

				float4 gx1 = ixy1 / 7.0;
				float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
				gx1 = frac(gx1);
				float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
				float4 sz1 = step(gz1, (float4)0.0);
				gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
				gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

				float3 g000 = float3(gx0.x, gy0.x, gz0.x);
				float3 g100 = float3(gx0.y, gy0.y, gz0.y);
				float3 g010 = float3(gx0.z, gy0.z, gz0.z);
				float3 g110 = float3(gx0.w, gy0.w, gz0.w);
				float3 g001 = float3(gx1.x, gy1.x, gz1.x);
				float3 g101 = float3(gx1.y, gy1.y, gz1.y);
				float3 g011 = float3(gx1.z, gy1.z, gz1.z);
				float3 g111 = float3(gx1.w, gy1.w, gz1.w);

				float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
				g000 *= norm0.x;
				g010 *= norm0.y;
				g100 *= norm0.z;
				g110 *= norm0.w;

				float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
				g001 *= norm1.x;
				g011 *= norm1.y;
				g101 *= norm1.z;
				g111 *= norm1.w;

				float n000 = dot(g000, Pf0);
				float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
				float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
				float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
				float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
				float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
				float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
				float n111 = dot(g111, Pf1);

				float3 fade_xyz = fade(Pf0);
				float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
				float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
				float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
				return 2.2 * n_xyz;
			}

			// Classic Perlin noise, periodic variant
			float pnoise(float3 P, float3 rep)
			{
				float3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
				float3 Pi1 = mod(Pi0 + (float3)1.0, rep); // Integer part + 1, mod period
				Pi0 = mod289(Pi0);
				Pi1 = mod289(Pi1);
				float3 Pf0 = frac(P); // Fractional part for interpolation
				float3 Pf1 = Pf0 - (float3)1.0; // Fractional part - 1.0
				float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
				float4 iy = float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
				float4 iz0 = (float4)Pi0.z;
				float4 iz1 = (float4)Pi1.z;

				float4 ixy = permute(permute(ix) + iy);
				float4 ixy0 = permute(ixy + iz0);
				float4 ixy1 = permute(ixy + iz1);

				float4 gx0 = ixy0 / 7.0;
				float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
				gx0 = frac(gx0);
				float4 gz0 = (float4)0.5 - abs(gx0) - abs(gy0);
				float4 sz0 = step(gz0, (float4)0.0);
				gx0 -= sz0 * (step((float4)0.0, gx0) - 0.5);
				gy0 -= sz0 * (step((float4)0.0, gy0) - 0.5);

				float4 gx1 = ixy1 / 7.0;
				float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
				gx1 = frac(gx1);
				float4 gz1 = (float4)0.5 - abs(gx1) - abs(gy1);
				float4 sz1 = step(gz1, (float4)0.0);
				gx1 -= sz1 * (step((float4)0.0, gx1) - 0.5);
				gy1 -= sz1 * (step((float4)0.0, gy1) - 0.5);

				float3 g000 = float3(gx0.x, gy0.x, gz0.x);
				float3 g100 = float3(gx0.y, gy0.y, gz0.y);
				float3 g010 = float3(gx0.z, gy0.z, gz0.z);
				float3 g110 = float3(gx0.w, gy0.w, gz0.w);
				float3 g001 = float3(gx1.x, gy1.x, gz1.x);
				float3 g101 = float3(gx1.y, gy1.y, gz1.y);
				float3 g011 = float3(gx1.z, gy1.z, gz1.z);
				float3 g111 = float3(gx1.w, gy1.w, gz1.w);

				float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
				g000 *= norm0.x;
				g010 *= norm0.y;
				g100 *= norm0.z;
				g110 *= norm0.w;
				float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
				g001 *= norm1.x;
				g011 *= norm1.y;
				g101 *= norm1.z;
				g111 *= norm1.w;

				float n000 = dot(g000, Pf0);
				float n100 = dot(g100, float3(Pf1.x, Pf0.y, Pf0.z));
				float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
				float n110 = dot(g110, float3(Pf1.x, Pf1.y, Pf0.z));
				float n001 = dot(g001, float3(Pf0.x, Pf0.y, Pf1.z));
				float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
				float n011 = dot(g011, float3(Pf0.x, Pf1.y, Pf1.z));
				float n111 = dot(g111, Pf1);

				float3 fade_xyz = fade(Pf0);
				float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
				float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
				float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
				return 2.2 * n_xyz;
			}

			float perlin5oct(float3 p) {

				float3 xyz = p;
				float amplitude_factor = 0.5;
				float frequency_factor = 2.0;

				float a = 1.0;
				float perlin_value = 0.0;
				perlin_value += a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.12);
				perlin_value -= a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.03);
				perlin_value -= a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.01);
				perlin_value -= a * cnoise(xyz).r; a *= amplitude_factor; xyz *= (frequency_factor + 0.01);
				perlin_value += a * cnoise(xyz).r;

				return perlin_value;
			}

			float3 voronoi_hash(float3 x, float s) {
				x = x % s;
				x = float3(dot(x, float3(127.1, 311.7, 74.7)),
					dot(x, float3(269.5, 183.3, 246.1)),
					dot(x, float3(113.5, 271.9, 124.6)));

				return frac(sin(x) * 43758.5453123);
			}

			float3 voronoi(in float3 x, float s, bool inverted) {
				x *= s;
				x += 0.5;
				float3 p = floor(x);
				float3 f = frac(x);
				 
				float id = 0.0;
				float2 res = float2(1.0, 1.0);

				for (int k = -1; k <= 1; k++) {
					for (int j = -1; j <= 1; j++) {
						for (int i = -1; i <= 1; i++) {
							float3 b = float3(i, j, k);
							float3 r = float3(b)-f + voronoi_hash(p + b, s);
							float d = dot(r, r);

							if (d < res.x) {
								id = dot(p + b, float3(1.0, 57.0, 113.0));
								res = float2(d, res.x);
							}
							else if (d < res.y) {
								res.y = d;
							}
						}
					}
				}

				float2 result = sqrt(res);
				id = abs(id);

				if (inverted)
					return float3(1.0 - result, id);
				else
					return float3(result, id);
			}


			float get_worley_3_octaves(float3 p, float s) {
				float3 xyz = p;
				float3 xyz2 = p + float3(100,20,500);
				float worley_value1 = voronoi(xyz, 2.0 * s, true).r;
				float worley_value2 = voronoi(xyz, 1.0 * s, true).r;
				float worley_value3 = voronoi(xyz2, 2.0 * s, true).r;

				worley_value1 = saturate(worley_value1);
				worley_value2 = saturate(worley_value2);
				worley_value3 = saturate(worley_value3);

				float worley_value = worley_value1;
				worley_value = worley_value - worley_value2 * 0.3;
				//worley_value = worley_value - worley_value3 * 0.3;

				return saturate(worley_value);
			}






			////



			float Falloff_Xsq_C2(float xsq) { xsq = 1.0 - xsq; return xsq*xsq*xsq; }	// ( 1.0 - x*x )^3.   NOTE: 2nd derivative is 0.0 at x=1.0, but non-zero at x=0.0
			float4 Falloff_Xsq_C2(float4 xsq) { xsq = 1.0 - xsq; return xsq*xsq*xsq; }
			float2 Interpolation_C2(float2 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }


			void FAST32_hash_2D(float2 gridcell, out float4 hash_0, out float4 hash_1)	//	generates 2 random numbers for each of the 4 cell corners
			{
				//    gridcell is assumed to be an integer coordinate
				const float2 OFFSET = float2(26.0, 161.0);
				const float DOMAIN = 71.0;
				const float2 SOMELARGEFLOATS = float2(951.135664, 642.949883);
				float4 P = float4(gridcell.xy, gridcell.xy + 1.0);
				P = P - floor(P * (1.0 / DOMAIN)) * DOMAIN;
				P += OFFSET.xyxy;
				P *= P;
				P = P.xzxz * P.yyww;
				hash_0 = frac(P * (1.0 / SOMELARGEFLOATS.x));
				hash_1 = frac(P * (1.0 / SOMELARGEFLOATS.y));
			}

			float4 FAST32_hash_2D(float2 gridcell)	//	generates a random number for each of the 4 cell corners
			{
				//	gridcell is assumed to be an integer coordinate
				const float2 OFFSET = float2(26.0, 161.0);
				const float DOMAIN = 71.0;
				const float SOMELARGEFLOAT = 951.135664;
				float4 P = float4(gridcell.xy, gridcell.xy + 1.0);
				P = P - floor(P * (1.0 / DOMAIN)) * DOMAIN;	//	truncate the domain
				P += OFFSET.xyxy;								//	offset to interesting part of the noise
				P *= P;											//	calculate and return the hash
				return frac(P.xzxz * P.yyww * (1.0 / SOMELARGEFLOAT));
			}


			//
			//	Perlin Noise 2D  ( gradient noise )
			//	Return value range of -1.0->1.0
			//	http://briansharpe.files.wordpress.com/2011/11/perlinsample.jpg
			//
			float Perlin2D(float2 P)
			{
				//	establish our grid cell and unit position
				float2 Pi = floor(P);
				float4 Pf_Pfmin1 = P.xyxy - float4(Pi, Pi + 1.0);

#if CLASSICPERLIN
				//
				//	classic noise looks much better than improved noise in 2D, and with an efficent hash function runs at about the same speed.
				//	requires 2 random numbers per point.
				//

				//	calculate the hash.
				//	( various hashing methods listed in order of speed )
				float4 hash_x, hash_y;
				FAST32_hash_2D(Pi, hash_x, hash_y);
				//SGPP_hash_2D( Pi, hash_x, hash_y );

				//	calculate the gradient results
				float4 grad_x = hash_x - 0.49999;
				float4 grad_y = hash_y - 0.49999;
				float4 grad_results = rsqrt(grad_x * grad_x + grad_y * grad_y) * (grad_x * Pf_Pfmin1.xzxz + grad_y * Pf_Pfmin1.yyww);

#if CLASSICPERLIN
				//	Classic Perlin Interpolation
				grad_results *= 1.4142135623730950488016887242097;		//	(optionally) scale things to a strict -1.0->1.0 range    *= 1.0/sqrt(0.5)
				float2 blend = Interpolation_C2(Pf_Pfmin1.xy);
				float4 blend2 = float4(blend, float2(1.0 - blend));
				return dot(grad_results, blend2.zxzx * blend2.wwyy);
#else
				//	Classic Perlin Surflet
				//	http://briansharpe.wordpress.com/2012/03/09/modifications-to-classic-perlin-noise/
				grad_results *= 2.3703703703703703703703703703704;		//	(optionally) scale things to a strict -1.0->1.0 range    *= 1.0/cube(0.75)
				float4 vecs_len_sq = Pf_Pfmin1 * Pf_Pfmin1;
				vecs_len_sq = vecs_len_sq.xzxz + vecs_len_sq.yyww;
				return dot(Falloff_Xsq_C2(min(float4(1.0), vecs_len_sq)), grad_results);
#endif

#else
				//
				//	2D improved perlin noise.
				//	requires 1 random value per point.
				//	does not look as good as classic in 2D due to only a small number of possible cell types.  But can run a lot faster than classic perlin noise if the hash function is slow
				//

				//	calculate the hash.
				//	( various hashing methods listed in order of speed )
				float4 hash = FAST32_hash_2D(Pi);
				//vec4 hash = BBS_hash_2D( Pi );
				//vec4 hash = SGPP_hash_2D( Pi );
				//vec4 hash = BBS_hash_hq_2D( Pi );

				//
				//	evaulate the gradients
				//	choose between the 4 diagonal gradients.  ( slightly slower than choosing the axis gradients, but shows less grid artifacts )
				//	NOTE:  diagonals give us a nice strict -1.0->1.0 range without additional scaling
				//	[1.0,1.0] [-1.0,1.0] [1.0,-1.0] [-1.0,-1.0]
				//
				hash -= 0.5;
				float4 grad_results = Pf_Pfmin1.xzxz * sign(hash) + Pf_Pfmin1.yyww * sign(abs(hash) - 0.25);

				//	blend the results and return
				float2 blend = Interpolation_C2(Pf_Pfmin1.xy);
				float4 blend2 = float4(blend, float2(1.0 - blend));
				return dot(grad_results, blend2.zxzx * blend2.wwyy);

#endif
			}

			//	convert a 0.0->1.0 sample to a -1.0->1.0 sample weighted towards the extremes
			float4 Cellular_weight_samples(float4 samples)
			{
				samples = samples * 2.0 - 1.0;
				//return (1.0 - samples * samples) * sign(samples);	// square
				return (samples * samples * samples) - sign(samples);	// cubic (even more variance)
			}

			float Cellular2D(float2 P)
			{
				//	establish our grid cell and unit position
				float2 Pi = floor(P);
				float2 Pf = P - Pi;

				//	calculate the hash.
				//	( various hashing methods listed in order of speed )
				float4 hash_x, hash_y;
				FAST32_hash_2D(Pi, hash_x, hash_y);
				//SGPP_hash_2D( Pi, hash_x, hash_y );

				//	generate the 4 random points
#if WORLEY_1
				//	restrict the random point offset to eliminate artifacts
				//	we'll improve the variance of the noise by pushing the points to the extremes of the jitter window
				const float JITTER_WINDOW = 0.25;	// 0.25 will guarentee no artifacts.  0.25 is the intersection on x of graphs f(x)=( (0.5+(0.5-x))^2 + (0.5-x)^2 ) and f(x)=( (0.5+x)^2 + x^2 )
				hash_x = Cellular_weight_samples(hash_x) * JITTER_WINDOW + float4(0.0, 1.0, 0.0, 1.0);
				hash_y = Cellular_weight_samples(hash_y) * JITTER_WINDOW + float4(0.0, 0.0, 1.0, 1.0);
#else
				//	non-weighted jitter window.  jitter window of 0.4 will give results similar to Stefans original implementation
				//	nicer looking, faster, but has minor artifacts.  ( discontinuities in signal )
				const float JITTER_WINDOW = 0.4;
				hash_x = hash_x * JITTER_WINDOW * 2.0 + float4(-JITTER_WINDOW, 1.0 - JITTER_WINDOW, -JITTER_WINDOW, 1.0 - JITTER_WINDOW);
				hash_y = hash_y * JITTER_WINDOW * 2.0 + float4(-JITTER_WINDOW, -JITTER_WINDOW, 1.0 - JITTER_WINDOW, 1.0 - JITTER_WINDOW);
#endif

				//	return the closest squared distance
				float4 dx = Pf.xxxx - hash_x;
				float4 dy = Pf.yyyy - hash_y;
				float4 d = dx * dx + dy * dy;
				d.xy = min(d.xy, d.zw);
				return min(d.x, d.y) * (1.0 / 1.125);	//	scale return value from 0.0->1.125 to 0.0->1.0  ( 0.75^2 * 2.0  == 1.125 )
			} 

			 

			float CalculateWorley3oct(float2 p, float p1, float p2, float p3) {
				float2 xy = p * p1;
				float2 xy2 = p * p2;
				float2 xy3 = p * p3;
				   
				float worley_value1 = Cellular2D(xy).r;
				float worley_value2 = Cellular2D(xy2).r;
				float worley_value3 = Cellular2D(xy3).r;

				worley_value1 = worley_value1;
				worley_value2 = worley_value2;
				worley_value3 = worley_value3; 
				 
				float worley_value = worley_value1 * 3;  
				worley_value = worley_value + worley_value2 * 1.5;
				worley_value = worley_value + worley_value3 * 1.5;
				    
				return saturate(1-worley_value);  
			}



			float CalculatePerlin5(float2 p) 
			{

				float2 xy = p;
				float amplitude_factor = 0.5;
				float frequency_factor = 2.0;

				float a = 1.0;
				float perlin_value = 0.0;
				perlin_value += a * Perlin2D(xy).r; a *= amplitude_factor; xy *= (frequency_factor + 0.12);
				perlin_value -= a * Perlin2D(xy).r; a *= amplitude_factor; xy *= (frequency_factor + 0.03);
				perlin_value -= a * Perlin2D(xy).r; a *= amplitude_factor; xy *= (frequency_factor + 0.01);
				perlin_value -= a * Perlin2D(xy).r; a *= amplitude_factor; xy *= (frequency_factor + 0.01);
				perlin_value += a * Perlin2D(xy).r;

				return perlin_value;
			}
