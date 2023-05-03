/*
Copyright (c) 2017 - Funktronic Labs, Inc. All Rights Reserved

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.
*/

#ifndef __FUNKTRONIC_LABS_VOLUMETRIC_CGINC__
#define __FUNKTRONIC_LABS_VOLUMETRIC_CGINC__

// uncomment for vertex lighting 
// #define VERTEXLIGHT_ON 1

#if defined(IS_URP)
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#elif defined(IS_HDRP)
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

	#if SHADERPASS == GBUFFER
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialGBufferMacros.hlsl"
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
	#endif

#else
	#include "UnityCG.cginc"
	#include "Lighting.cginc" // for _LightColor0
	#include "AutoLight.cginc" // for UnityShadowLibrary.cginc
#endif

#ifdef EnableNoiseDistortion
	float4 _ShadowStepSettings;
	sampler2D _ShadowOffsetMap;
	float4 _ShadowStepDistortionSettings;
#endif

//////////////////////////////////////////////////
// GLOBAL FUNCTIONS
//////////////////////////////////////////////////
// renamed to avoid light brigade name collision 
float luminVolumetric(float3 rgb)
{
	return dot(rgb, float3(0.299, 0.587, 0.114));
}

//////////////////////////////////////////////////
// SHADER UNIFORMS
//////////////////////////////////////////////////
// layer 0
#ifdef EnableLayer0
sampler2D _Layer0Tex;
half4 _Layer0Tint;
float4 _Layer0Tex_ST;
float _Layer0SpeedX;
float _Layer0SpeedY;
#endif

// layer 1
#ifdef EnableLayer1
sampler2D _Layer1Tex;
half4 _Layer1Tint;
float4 _Layer1Tex_ST;
float _Layer1SpeedX;
float _Layer1SpeedY;
#endif

// layer 2
#ifdef EnableLayer2
sampler2D _Layer2Tex;
half4 _Layer2Tint;
float4 _Layer2Tex_ST;
float _Layer2SpeedX;
float _Layer2SpeedY;
#endif

// global layer params
float _LayerDepthFalloff;
float _LayerHeightBias;
float _LayerHeightBiasStep;

// marble tex
sampler2D _MarbleTex;
float4 _MarbleTex_ST;
float _MarbleHeightScale;
float _MarbleHeightCausticOffset;

// caustic
sampler2D _CausticMap;
float4 _CausticMap_ST;
half4 _CausticTint;
float _CausticScrollSpeed;

// surface alpha masking
sampler2D _SurfaceAlphaMaskTex;
float4 _SurfaceAlphaMaskTex_ST;
float4 _SurfaceAlphaColor;

// fresnel
#ifdef EnableFresnel
float _FresnelTightness;
float4 _FresnelColorInside;
float4 _FresnelColorOutside;
#endif

// inner light
#ifdef EnableInnerLight
float _InnerLightTightness;
float4 _InnerLightColorInside;
float4 _InnerLightColorOutside;
#endif

// specular
#ifdef EnableSpecular
float _SpecularTightness;
float _SpecularBrightness;
#endif

// refraction
#if defined(EnableRefraction) && !defined(VOLUMETRIC_MOBILE_VER)
float _RefractionStrength;

#if defined(IS_URP)
	sampler2D _CameraOpaqueTexture;
#elif defined(IS_HDRP)
	// sampler2D _ColorPyramidTexture;
#else
	sampler2D _BackgroundTexture;
#endif
#endif

//////////////////////////////////////////////////
// SHADER DATA
//////////////////////////////////////////////////
struct appdata
{
	float4 pos : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float4 uv2 : TEXCOORD1;

#ifdef EnableTopCover
	float4 color : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float2 uv : TEXCOORD0;
	float4 pos : SV_POSITION;
	float4 lightData : TEXCOORD1;
	float3 worldPos: TEXCOORD2;
	float3 worldNormal: TEXCOORD3;
	float3 worldRefl: TEXCOORD4;
	float3 worldViewDir: TEXCOORD5;
	float3 camPosTexcoord : TEXCOORD6;

#if defined(EnableRefraction)
	float4 screenPos:TEXCOORD7;
#endif

	float3 viewNormal : TEXCOORD8;

	#if defined(IS_URP)
		float fogFactor : TEXCOORD9;
	#elif defined(IS_HDRP)
		// todo 
	#else
		#if defined(EnableFog)
			UNITY_FOG_COORDS(9)
		#endif
    #endif

#if defined(IS_URP)
	DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 10);
	#ifdef DYNAMICLIGHTMAP_ON
		float2  dynamicLightmapUV : TEXCOORD11; // Dynamic lightmap UVs
	#endif

	float4 uv2 : TEXCOORD12;
#elif defined(IS_HDRP)
	// todo
	float4 uv2 : TEXCOORD12;
#else
	UNITY_LIGHTING_COORDS(10, 11)
	float4 uv2 : TEXCOORD12;

	// these three vectors will hold a 3x3 rotation matrix
	// that transforms from tangent to world space
	half3 tspace0 : TEXCOORD13; // tangent.x, bitangent.x, normal.x
	half3 tspace1 : TEXCOORD14; // tangent.y, bitangent.y, normal.y
	half3 tspace2 : TEXCOORD15; // tangent.z, bitangent.z, normal.z
#endif

#if defined(VERTEXLIGHT_ON)
	float3 vertexLightColor : TEXCOORD17;
#endif

#if defined(EnableTopCover)
	float3 topCover : TEXCOORD18;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#ifdef UNITY_5
	#define unity_ObjectToWorld _Object2World // UNITY_SHADER_NO_UPGRADE
	#define unity_WorldToObject _World2Object // UNITY_SHADER_NO_UPGRADE
	#define UnityObjectToClipPos(_X) mul(UNITY_MATRIX_MVP, float4(_X.xyz, 1.0)) // UNITY_SHADER_NO_UPGRADE
#endif

#if defined(IS_URP)
half3 ComputeLightingURP(float3 worldPos, float3 worldNormal, float3 viewDirection)
{
	float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
	Light mainLight = GetMainLight(shadowCoord);
	half mainLightShadow = mainLight.shadowAttenuation;
	
	half roughness = 1.0;
	half perceptualRoughness = roughness * roughness;
	
	// for non intensity 1 shadows..
	half3 reflectVector = reflect(-viewDirection, worldNormal);
	half3 ambientLighting = GlossyEnvironmentReflection(reflectVector, perceptualRoughness, 1.0 - mainLightShadow); 
	
	half3 lighting = _MainLightColor * mainLightShadow + ambientLighting;

	// uncomment for additive lights 
	//  const uint lightsCount = GetAdditionalLightsCount();
	//  for (uint lightIndex = 0u; lightIndex < lightsCount; ++lightIndex)
	//  {
	//  	Light light = GetAdditionalLight(lightIndex, worldPos);
	//  	half3 lightColor = light.color * light.distanceAttenuation;
	//  
	//  	lighting += lightColor;
	//  }

	return lighting;
}
#endif

#if defined(IS_HDRP)
half3 ComputeLightingHDRP(float3 worldPos, float3 worldNormal, float3 viewDirection)
{
	// todo: hdrp lightin? 
	#if defined(HAS_LIGHTLOOP)
		if (_DirectionalShadowIndex >= 0)
		{
			DirectionalLightData light = _DirectionalLightDatas[_DirectionalShadowIndex];
			return light.color; 
		}
	#endif

	return 1;
}
#endif

void ComputeVertexLightColor(inout v2f i)
{
#ifdef VERTEXLIGHT_ON
	#if defined(IS_URP)
		i.vertexLightColor = ComputeLightingURP(i.worldPos, i.worldNormal, i.viewNormal);
	#elif defined(IS_HDRP)
		i.vertexLightColor = ComputeLightingHDRP(i.worldPos, i.worldNormal, i.viewNormal);
	#else
		i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.worldNormal
		);
	#endif
#endif
}

// TODO: A similar function should be already available in SRP lib on master. Use that instead
float4 ComputeScreenPosHDRP(float4 positionCS)
{
	float4 o = positionCS * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = positionCS.zw;
	return o;
}

//////////////////////////////////////////////////
// VERTEX SHADER
//////////////////////////////////////////////////
v2f vertVolumetric(appdata v)
{
	UNITY_SETUP_INSTANCE_ID(v);

	float3 localPos = v.pos;

#if defined(IS_URP) || defined(IS_HDRP)
	float3 worldPos = TransformObjectToWorld(localPos);
	float3 worldNormal = normalize(TransformObjectToWorldNormal(v.normal));
	float3 worldViewDir = GetWorldSpaceNormalizeViewDir(worldPos);
#else
	float3 worldPos = mul(unity_ObjectToWorld, v.pos).xyz;
	float3 worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
	float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
#endif

	// texture space (TBN) basis-vector
	float3 binormal = cross(v.tangent.xyz, v.normal);
	float3x3 tbn = float3x3(v.tangent.xyz, binormal, v.normal);

	// get cam pos in texture (TBN) space
#if defined(IS_URP) || defined(IS_HDRP)
	float3 camPosLocal = TransformWorldToObject(float4(_WorldSpaceCameraPos, 1.0)).xyz;
#else
	float3 camPosLocal = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;
#endif

	float3 dirToCamLocal = camPosLocal - localPos;
	float3 camPosTexcoord = mul(tbn, dirToCamLocal);

	v2f o = (v2f) 0;

    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

#if defined(IS_URP) || defined(IS_HDRP)
	o.pos = TransformObjectToHClip(localPos);
#else
    o.pos = UnityObjectToClipPos(localPos);
#endif

#ifdef EnableTopCover
	o.topCover = v.color;
#endif

	o.uv = v.uv;
	o.uv2 = v.uv2;
	o.worldNormal = worldNormal;
	o.worldRefl = reflect(-worldViewDir, worldNormal);
	o.worldPos = worldPos;
	o.worldViewDir = worldViewDir;
	o.camPosTexcoord = camPosTexcoord;

#if defined(EnableRefraction)
	#if defined(IS_HDRP)
		o.screenPos = ComputeScreenPosHDRP(o.pos);
	#else
		o.screenPos = ComputeScreenPos(o.pos);
	#endif
#endif

#if defined(IS_URP)
	o.viewNormal = normalize(mul(GetWorldToViewMatrix(), float4(worldNormal, 0.0)).xyz);
#elif defined(IS_HDRP)
	o.viewNormal = normalize(mul(GetWorldToViewMatrix(), float4(worldNormal, 0.0)).xyz);
#else
	o.viewNormal = normalize(mul(UNITY_MATRIX_MV, float4(v.normal, 0.0)).xyz);
#endif

	#if defined(IS_URP)
		o.fogFactor = ComputeFogFactor(o.pos.z);
	#elif defined(IS_HDRP)
		// o.fogFactor = ComputeFogFactor(o.pos.z); // todo 
	#else
		#if defined(EnableFog)
			UNITY_TRANSFER_FOG(o, o.pos);
		#endif
    #endif

#if defined(IS_URP)
#elif defined(IS_HDRP)
#else
	UNITY_TRANSFER_LIGHTING(o, v.uv2);

	half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);

	// compute bitangent from cross product of normal and tangent
	half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	half3 wBitangent = cross(worldNormal, wTangent) * tangentSign;

	// output the tangent space matrix
	o.tspace0 = half3(wTangent.x, wBitangent.x, worldNormal.x);
	o.tspace1 = half3(wTangent.y, wBitangent.y, worldNormal.y);
	o.tspace2 = half3(wTangent.z, wBitangent.z, worldNormal.z);
#endif

	ComputeVertexLightColor(o);

    return o;
}

#if defined(IS_URP)
#elif defined(IS_HDRP)
#else

// modified from UnityGI_Base
inline float4 Funk_UnityGI_Base(float2 lightmapUV, float3 worldPos, half ambient, half atten, half occlusion, half3 normalWorld)
{
	// Base pass with Lightmap support is responsible for handling ShadowMask / blending here for performance reason
	#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
		half bakedAtten = UnitySampleBakedOcclusion(lightmapUV.xy, worldPos);
		float zDist = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
		float fadeDist = UnityComputeShadowFadeDistance(worldPos, zDist);
		atten = UnityMixRealtimeAndBakedShadows(atten, bakedAtten, UnityComputeShadowFade(fadeDist));
	#endif

	float3 diffuse = max(0, ShadeSHPerPixel(normalWorld, ambient, worldPos));

	#if defined(LIGHTMAP_ON)
		diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV));

		#if defined(DIRLIGHTMAP_COMBINED)
			float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, lightmapUV);
			diffuse = DecodeDirectionalLightmap(diffuse, lightmapDirection, normalWorld);
		#endif
	#endif

	#ifdef DYNAMICLIGHTMAP_ON
		// Dynamic lightmaps
		half4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, lightmapUV.xy);
		half3 realtimeColor = DecodeRealtimeLightmap(realtimeColorTex);

		#ifdef DIRLIGHTMAP_COMBINED
			half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, lightmapUV.zw);
			diffuse += DecodeDirectionalLightmap(realtimeColor, realtimeDirTex, normalWorld);
		#else
			diffuse += realtimeColor;
		#endif
	#endif

	diffuse *= occlusion;

	return float4(diffuse, atten);
}
#endif

#if defined(IS_HDRP) && !defined(HAS_LIGHTLOOP)
	#undef EnableSpecular
#endif

//////////////////////////////////////////////////
// FRAGMENT SHADER
//////////////////////////////////////////////////

#if defined(IS_HDRP) && SHADERPASS == GBUFFER 
void fragVolumetric(v2f i, OUTPUT_GBUFFER(outGBuffer))
#else
half4 fragVolumetric(v2f i) : SV_Target
#endif
{
    UNITY_SETUP_INSTANCE_ID(i);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    float phong = saturate(dot(i.worldNormal, normalize(_WorldSpaceCameraPos - i.worldPos)));
	
	// height map UV (marble texture)
	float2 uvMarble = i.uv;

	// caustic sampling
# ifdef EnableCaustic
	float caustic = tex2D(_CausticMap, TRANSFORM_TEX(i.uv, _CausticMap) + float2(0.0, _Time.x*_CausticScrollSpeed)).r;
	uvMarble += float2(caustic, _Time.x) * _MarbleHeightCausticOffset;
#endif

    // height-field offset
    float3 eyeVec = normalize(i.camPosTexcoord);
	float height = tex2D(_MarbleTex, TRANSFORM_TEX(uvMarble, _MarbleTex)).r;
	float v = height * _MarbleHeightScale - (_MarbleHeightScale*0.5);
	float2 newCoords = i.uv + eyeVec.xy * v;

	// accumulate layers
	float3 colorLayersAccum = float3(0.0, 0.0, 0.0);
	float layerDepthFalloffAccum = 1.0;
	float layerHeightBiasAccum = _LayerHeightBias;

	// layer 0
	#ifdef EnableLayer0
	{
		float2 layerBaseUV = TRANSFORM_TEX(i.uv, _Layer0Tex) + _Time.x * float2(_Layer0SpeedX,_Layer0SpeedY);
		float2 layerParallaxUV = layerBaseUV + eyeVec.xy * v + eyeVec.xy * -layerHeightBiasAccum;

		colorLayersAccum += tex2D(_Layer0Tex, layerParallaxUV).xyz * layerDepthFalloffAccum * _Layer0Tint.xyz;
		layerDepthFalloffAccum *= _LayerDepthFalloff;
		layerHeightBiasAccum += _LayerHeightBiasStep;
	}
	#endif

	// layer 1
	#ifdef EnableLayer1
	{
		float2 layerBaseUV = TRANSFORM_TEX(i.uv, _Layer1Tex) + _Time.x * float2(_Layer1SpeedX, _Layer1SpeedY);
		float2 layerParallaxUV = layerBaseUV + eyeVec.xy * v + eyeVec.xy * -layerHeightBiasAccum;

		colorLayersAccum += tex2D(_Layer1Tex, layerParallaxUV).xyz * layerDepthFalloffAccum * _Layer1Tint.xyz;
		layerDepthFalloffAccum *= _LayerDepthFalloff;
		layerHeightBiasAccum += _LayerHeightBiasStep;
	}
	#endif

	// layer 2
	#ifdef EnableLayer2
	{
		float2 layerBaseUV = TRANSFORM_TEX(i.uv, _Layer2Tex) + _Time.x * float2(_Layer2SpeedX, _Layer2SpeedY);
		float2 layerParallaxUV = layerBaseUV + eyeVec.xy * v + eyeVec.xy * -layerHeightBiasAccum;

		colorLayersAccum += tex2D(_Layer2Tex, layerParallaxUV).xyz * layerDepthFalloffAccum * _Layer2Tint.xyz;
		layerDepthFalloffAccum *= _LayerDepthFalloff;
		layerHeightBiasAccum += _LayerHeightBiasStep;
	}
	#endif

	float3 color = colorLayersAccum;
	float alpha = 0.0;

	// marble
	half4 texMarble = tex2D(_MarbleTex, TRANSFORM_TEX(newCoords, _MarbleTex));
	color += texMarble.xyz;
	//half4 texMarble2 = tex2D(_MarbleTex, newCoords + i.uv);
	//color += texMarble2.xyz;

	// alpha everything so far
	alpha += saturate(luminVolumetric(color));


	// fresnel
	#ifdef EnableFresnel
	float fresnel = pow(1.0 - phong, _FresnelTightness);
														
	// fresnel - reflections use skybox?
	#ifdef EnableFresnelUseSkybox

		#if defined(IS_URP)
			half4 skyData = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, i.worldRefl);
			half3 skyColor = DecodeHDREnvironment(skyData, unity_SpecCube0_HDR);
		#elif defined(IS_HDRP)
			// todo 
	half4 skyData = 0; // SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, i.worldRefl);
	half3 skyColor = 0; //  DecodeHDREnvironment(skyData, unity_SpecCube0_HDR);
		#else
			half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
			half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
		#endif

	color += lerp(_FresnelColorInside, _FresnelColorOutside, fresnel) * skyColor.xyz * fresnel;
	#else
	color += lerp(_FresnelColorInside, _FresnelColorOutside, fresnel) * fresnel;
	#endif

	alpha += fresnel;
	#endif

	// inner light
	#ifdef EnableInnerLight
	float innerLight = pow(phong, _InnerLightTightness);
	color += lerp(_InnerLightColorOutside, _InnerLightColorInside, innerLight) * innerLight;
	alpha += innerLight;
	#endif

	// caustic
	#ifdef EnableCaustic
	color += _CausticTint.xyz * caustic;
	alpha += caustic*_CausticTint.w;
	#endif

	// overall alpha mask
	#ifdef EnableSurfaceMask
	float alphaMask = tex2D(_SurfaceAlphaMaskTex, TRANSFORM_TEX(i.uv, _SurfaceAlphaMaskTex)).r;
	color = color + _SurfaceAlphaColor.xyz * alphaMask;
	alpha += alphaMask;
	#endif


	// specular
	#ifdef EnableSpecular
		float3 worldNormalNormalized = normalize(i.worldNormal);

	#if defined(IS_URP)
		float3 R = reflect(-_MainLightPosition.xyz, worldNormalNormalized);
	#elif defined(IS_HDRP)

		float3 mainLightDirectionHdrp = float3(0, 1, 0);
		if (_DirectionalShadowIndex >= 0)
		{
			DirectionalLightData light = _DirectionalLightDatas[_DirectionalShadowIndex];
			mainLightDirectionHdrp = light.forward;
		}

		float3 R = reflect(-mainLightDirectionHdrp, worldNormalNormalized);
	#else
		float3 R = reflect(-_WorldSpaceLightPos0.xyz, worldNormalNormalized);
	#endif

		float specular = pow(saturate(dot(R, normalize(i.worldViewDir))), _SpecularTightness);

	#if defined(IS_URP)
		color += _MainLightColor.xyz * specular * _SpecularBrightness;
	#elif defined(IS_HDRP)
		if (_DirectionalShadowIndex >= 0)
		{
			DirectionalLightData light = _DirectionalLightDatas[_DirectionalShadowIndex];
			color += light.color * specular * _SpecularBrightness;
		}
	#else
		color += _LightColor0.xyz * specular * _SpecularBrightness;
	#endif
		alpha += specular * _SpecularBrightness;
	#endif

	color = saturate(color);
	alpha = saturate(alpha);

	// refraction/distortion
	#if defined(EnableRefraction) && !defined(VOLUMETRIC_MOBILE_VER)
	float2 screenUV = i.screenPos.xy / i.screenPos.w;
	float2 refractionUV = screenUV + (-i.viewNormal.xy * 0.5 + float2(height, 0.0)) * _RefractionStrength;

#if defined(IS_URP)
	half4 bgcolor = tex2D(_CameraOpaqueTexture, refractionUV);
#elif defined(IS_HDRP)
	half4 bgcolor = half4(SampleCameraColor(refractionUV), 1);
#else
	half4 bgcolor = tex2D(_BackgroundTexture, refractionUV);
#endif

	color = lerp(bgcolor.xyz, color, alpha);
	alpha = 1.0;
	#endif

	// snow - TOP Cover
#ifdef EnableNoiseDistortion
	const half noiseFreq = _ShadowStepDistortionSettings.x;
	const half noiseStrength = _ShadowStepDistortionSettings.y;
	const half noiseBiplanarK = _ShadowStepDistortionSettings.z;
	half4 noiseTex = biplanar(_ShadowOffsetMap, i.worldPos * noiseFreq, i.worldNormal, noiseBiplanarK);
		   noiseTex = noiseTex * 2.0 - 1.0;

	const half darknessOffsetNoise = noiseTex.r * noiseStrength;
#else
	const half darknessOffsetNoise = 1;
	half4 noiseTex = 1;
#endif

	// lighting/attenuation 
	#ifndef DisableUnityLighting
		#if defined(IS_URP)
			#ifdef VERTEXLIGHT_ON
				color.rgb *= i.vertexLightColor;
			#else
				color.rgb *= ComputeLightingURP(i.worldPos, i.worldNormal, -i.viewNormal);
			#endif
		#elif defined(IS_HDRP)
				#ifdef VERTEXLIGHT_ON
					color.rgb *= i.vertexLightColor;
				#else
					color.rgb *= ComputeLightingHDRP(i.worldPos, i.worldNormal, -i.viewNormal);
				#endif
		#else
			UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);

			float lightingAmbient = 0.0;
			float lightingOcclusion = 1.0;
			float4 gi = Funk_UnityGI_Base(i.uv2, i.worldPos, lightingAmbient, attenuation, lightingOcclusion, i.worldNormal);
			float3 ambient = gi.rgb;

			half3 lighting = attenuation * _LightColor0 + ambient;

			color *= lighting;

		#endif
	#endif

	#if defined(IS_URP)
		color.rgb = MixFog(color.rgb, i.fogFactor);
	#elif defined(IS_HDRP)
		// todo: fog 
	#else
		#if defined(EnableFog)
			UNITY_APPLY_FOG(i.fogCoord, color);
		#endif
    #endif

	#if defined(IS_HDRP) && SHADERPASS == GBUFFER  
		outGBuffer0 = float4(color, 1.0);
		
		NormalData gbufferNormalData = (NormalData) 0;
		gbufferNormalData.normalWS = i.worldNormal;
		gbufferNormalData.perceptualRoughness = 0.02;

		EncodeIntoNormalBuffer(gbufferNormalData, outGBuffer1);

		outGBuffer2 = float4(color, 1.0);
		outGBuffer3 = float4(color, 1.0);

		// requires built-in hdrp data
		//  #ifdef SHADOWS_SHADOWMASK
		//  		OUT_GBUFFER_SHADOWMASK = BUILTIN_DATA_SHADOW_MASK;
		//  #endif
		//  
		//  #ifdef UNITY_VIRTUAL_TEXTURING
		//  		OUT_GBUFFER_VTFEEDBACK = builtinData.vtPackedFeedback;
		//  #endif
	#else
		return float4(color, alpha);
	#endif
}

#endif