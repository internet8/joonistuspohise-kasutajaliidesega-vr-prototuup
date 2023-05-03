// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

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

Shader "FunktronicLabs/VolumetricMobile"
{
	Properties
	{
		[Header(Layer 1)]
		_Layer0Tex ("Layer 1 Texture", 2D) = "white" {}
		_Layer0Tint("Layer 1 Tint", COLOR) = (1,1,1,1)
		_Layer0SpeedX("Layer 1 Scroll Speed X", Range(-1.0, 1.0)) = 0
		_Layer0SpeedY("Layer 1 Scroll Speed Y", Range(-1.0, 1.0)) = 0

		[Header(Layer 2)]
		[Toggle(EnableLayer1)] _EnableLayer1("Enable", Float) = 1
		_Layer1Tex ("Layer 2 Texture", 2D) = "white" {}
		_Layer1Tint("Layer 2 Tint", COLOR) = (1,1,1,1)
		_Layer1SpeedX("Layer 2 Scroll Speed X", Range(-1.0, 1.0)) = 0
		_Layer1SpeedY("Layer 2 Scroll Speed Y", Range(-1.0, 1.0)) = 0

		[Header(Layer 3)]
		[Toggle(EnableLayer2)] _EnableLayer2("Enable", Float) = 1
		_Layer2Tex ("Layer 3 Texture", 2D) = "white" {}
		_Layer2Tint("Layer 3 Tint", COLOR) = (1,1,1,1)
		_Layer2SpeedX("Layer 3 Scroll Speed X", Range(-1.0, 1.0)) = 0
		_Layer2SpeedY("Layer 3 Scroll Speed Y", Range(-1.0, 1.0)) = 0

		[Header(Layers Global Properties)]
		_LayerHeightBias("Layer Height Start Bias", Range(0.0, 0.2)) = 0.1
		_LayerHeightBiasStep("Layer Height Step", Range(0.0, 0.3)) = 0.1
		_LayerDepthFalloff("Layer Depth Fallofff", Range(0.0, 1.0)) = 0.9

		[Header(Volumetric Marble)]
		_MarbleTex ("Marble Heightmap Texture", 2D) = "black" {}
		_MarbleHeightScale("Marble Height Scale", Range(0.0, 0.5)) = 0.1
		_MarbleHeightCausticOffset("Marble Caustic Offset", Range(-5.0, 5.0)) = 0.1

		[Header(Caustic)]
		[Toggle(EnableCaustic)] _EnableCaustic("Enable", Float) = 0
		_CausticMap("Caustic Map", 2D) = "black" {}
		_CausticTint("Caustic Tint", COLOR) = (1,1,1,1)
		_CausticScrollSpeed("Caustic Scroll Speed X", Range(-5.0, 5.0)) = 1.0

		[Header(Fresnel)]
		[Toggle(EnableFresnel)] _EnableFresnel("Enable", Float) = 1
		[Toggle(EnableFresnelUseSkybox)] _EnableFresnelUseSkybox("Use Skybox Reflection", Float) = 0
		_FresnelTightness("Fresnel Tightness", Range(0.0, 10.0)) = 4.0
		[HDR] _FresnelColorInside("Fresnel Color Inside", COLOR) = (1,1,0.5,1)
		[HDR] _FresnelColorOutside("Fresnel Color Outside", COLOR) = (1,1,1,1)

		[Header(Surface Mask)]
		[Toggle(EnableSurfaceMask)] _EnableSurfaceMask("Enable", Float) = 0
		_SurfaceAlphaMaskTex("Surface Alpha Mask Texture", 2D) = "white" {}
		[HDR] _SurfaceAlphaColor("Surface Mask Color", COLOR) = (1,1,1,1)

		[Header(Inner Light)]
		[Toggle(EnableInnerLight)] _EnableInnerLight("Enable", Float) = 0
		_InnerLightTightness("Inner Light Tightness", Range(0.0, 40.0)) = 20.0
		[HDR] _InnerLightColorInside("Inner Light Color Inside", COLOR) = (1,1,1,1)
		[HDR] _InnerLightColorOutside("Inner Light Color Outside", COLOR) = (1,1,0,1)

		[Header(Specular)]
		[Toggle(EnableSpecular)] _EnableSpecular("Enable Specular", Float) = 0
		_SpecularTightness("Specular Tightness", Range(0.0, 40.0)) = 2.0
		_SpecularBrightness("Specular Brightness", Range(0.0, 5.0)) = 1.0

		[Header(Fog)]
		[Toggle(EnableFog)] _EnableFog("Enable Fog", Float) = 0

		/*[Header(Refraction)]
		[Toggle(EnableRefraction)] _EnableRefraction("Enable Refraction", Float) = 0
		_RefractionStrength("Refraction Strength", Range(0.0, 1.0)) = 0.2*/
		
		[Header(Unity Lighting)]
		[Toggle(DisableUnityLighting)] _DisableUnityLighting("Disable Unity Lighting", Float) = 0

		// HDRP-only properties 
		[HideInInspector] [ToggleUI]  _AlphaCutoffEnable("Alpha Cutoff Enable", Float) = 0.0
		[HideInInspector] _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[HideInInspector] _TransparentSortPriority("_TransparentSortPriority", Float) = 0
        [HideInInspector] _SurfaceType("__surfacetype", Float) = 0.0
        [HideInInspector] _BlendMode("__blendmode", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _AlphaSrcBlend("__alphaSrc", Float) = 1.0
        [HideInInspector] _AlphaDstBlend("__alphaDst", Float) = 0.0
        [HideInInspector] [ToggleUI]_AlphaToMaskInspectorValue("_AlphaToMaskInspectorValue", Float) = 0 // Property used to save the alpha to mask state in the inspector
        [HideInInspector] [ToggleUI]_AlphaToMask("__alphaToMask", Float) = 0
        [HideInInspector] [ToggleUI] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] [ToggleUI] _TransparentZWrite("_TransparentZWrite", Float) = 0.0
        [HideInInspector] _CullMode("__cullmode", Float) = 2.0
		[HideInInspector] [Enum(UnityEditor.Rendering.HighDefinition.TransparentCullMode)] _TransparentCullMode("_TransparentCullMode", Int) = 2 // Back culling by default
		[HideInInspector] [Enum(UnityEditor.Rendering.HighDefinition.OpaqueCullMode)] _OpaqueCullMode("_OpaqueCullMode", Int) = 2 // Back culling by default
        [HideInInspector] _ZTestModeDistortion("_ZTestModeDistortion", Int) = 8
		[HideInInspector] [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestTransparent("Transparent ZTest", Int) = 4 // Less equal
        [HideInInspector] _ZTestDepthEqualForOpaque("_ZTestDepthEqualForOpaque", Int) = 4 // Less equal
		[HideInInspector] [ToggleUI] _EnableFogOnTransparent("Enable Fog", Float) = 0.0
		[HideInInspector] [ToggleUI] _DoubleSidedEnable("Double sided enable", Float) = 0.0
        [HideInInspector] _StencilRef("_StencilRef", Int) = 0 // StencilUsage.Clear
        [HideInInspector] _StencilWriteMask("_StencilWriteMask", Int) = 3 // StencilUsage.RequiresDeferredLighting | StencilUsage.SubsurfaceScattering
        [HideInInspector] _StencilRefGBuffer("_StencilRefGBuffer", Int) = 2 // StencilUsage.RequiresDeferredLighting
        [HideInInspector] _StencilWriteMaskGBuffer("_StencilWriteMaskGBuffer", Int) = 3 // StencilUsage.RequiresDeferredLighting | StencilUsage.SubsurfaceScattering
        [HideInInspector] _StencilRefDepth("_StencilRefDepth", Int) = 0 // Nothing
        [HideInInspector] _StencilWriteMaskDepth("_StencilWriteMaskDepth", Int) = 8 // StencilUsage.TraceReflectionRay
        [HideInInspector] _StencilRefMV("_StencilRefMV", Int) = 32 // StencilUsage.ObjectMotionVector
        [HideInInspector] _StencilWriteMaskMV("_StencilWriteMaskMV", Int) = 32 // StencilUsage.ObjectMotionVector
	}

	SubShader
	{
		PackageRequirements {"com.unity.render-pipelines.universal"}

		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
		LOD 100

		Pass
		{
			// Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
			// no LightMode tag are also rendered by Universal Render Pipeline
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}

			HLSLPROGRAM

			#pragma shader_feature __ EnableLayer0
			#pragma shader_feature __ EnableLayer1
			#pragma shader_feature __ EnableLayer2
			#pragma shader_feature __ EnableCaustic			
			#pragma shader_feature __ EnableFresnel
			#pragma shader_feature __ EnableFresnelUseSkybox
			#pragma shader_feature __ EnableSurfaceMask
			#pragma shader_feature __ EnableInnerLight
			#pragma shader_feature __ EnableSpecular
			#pragma shader_feature __ EnableFog
			#pragma shader_feature __ EnableRefraction
			#pragma shader_feature __ DisableUnityLighting

			#pragma vertex vertVolumetric
			#pragma fragment fragVolumetric
			#pragma multi_compile_instancing
			#pragma multi_compile_fog

			#ifndef EnableFresnelUseSkybox
				#define _ENVIRONMENTREFLECTIONS_OFF 1
			#endif

			// -------------------------------------
			// Universal Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma vertex vertVolumetric
			#pragma fragment fragVolumetric
			#pragma multi_compile_instancing
			#pragma multi_compile_fog

			#define VOLUMETRIC_MOBILE_VER // NO grabpass for mobile.. too heavy!
			#define IS_URP
			#include "Volumetric.cginc"

			ENDHLSL
		}

		Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

			#pragma shader_feature EnableTopCover
			#pragma shader_feature EnableTopCoverVertexColorBlue

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
	}

	SubShader
	{
		PackageRequirements {"com.unity.render-pipelines.high-definition"}

		// This tags allow to use the shader replacement features
		Tags{ "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDLitShader" }

		Pass
		{
			Name "GBuffer"
			Tags { "LightMode" = "GBuffer" } // This will be only for opaque object based on the RenderQueue index

			//  Cull[_CullMode]
			//  ZTest[_ZTestGBuffer]

			Stencil
			{
				WriteMask[_StencilWriteMaskGBuffer]
				Ref[_StencilRefGBuffer]
				Comp Always
				Pass Replace
			}

			HLSLPROGRAM
		
			// HDRP 
			#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ DOTS_INSTANCING_ON
			#pragma multi_compile _ LOD_FADE_CROSSFADE
		
			// funktronic
			#pragma shader_feature __ EnableLayer0
			#pragma shader_feature __ EnableLayer1
			#pragma shader_feature __ EnableLayer2
			#pragma shader_feature __ EnableCaustic			
			#pragma shader_feature __ EnableFresnel
			#pragma shader_feature __ EnableFresnelUseSkybox
			#pragma shader_feature __ EnableSurfaceMask
			#pragma shader_feature __ EnableInnerLight
			#pragma shader_feature __ EnableSpecular
			#pragma shader_feature __ EnableFog
			#pragma shader_feature __ EnableRefraction
			#pragma shader_feature __ DisableUnityLighting
		
			#pragma vertex vertVolumetric
			#pragma fragment fragVolumetric
			#pragma multi_compile_instancing
			#pragma multi_compile_fog

			#define VOLUMETRIC_MOBILE_VER 
			#define IS_HDRP
			#define SHADERPASS GBUFFER

			#include "Volumetric.cginc"
				
			ENDHLSL
		}

		Pass
		{
			Name "Forward"
			Tags { "LightMode" = "Forward" } // This will be only for transparent object based on the RenderQueue index

			Stencil
			{
				WriteMask[_StencilWriteMask]
				Ref[_StencilRef]
				Comp Always
				Pass Replace
			}

			HLSLPROGRAM

			// HDRP 
			#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ DOTS_INSTANCING_ON
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile _ DEBUG_DISPLAY
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON
			#pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
			#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH
			#pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST
			
			#define SHADERPASS SHADERPASS_FORWARD
			#define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST

			#define HAS_LIGHTLOOP

			// funktronic
			#pragma shader_feature __ EnableLayer0
			#pragma shader_feature __ EnableLayer1
			#pragma shader_feature __ EnableLayer2
			#pragma shader_feature __ EnableCaustic			
			#pragma shader_feature __ EnableFresnel
			#pragma shader_feature __ EnableFresnelUseSkybox
			#pragma shader_feature __ EnableSurfaceMask
			#pragma shader_feature __ EnableInnerLight
			#pragma shader_feature __ EnableSpecular
			#pragma shader_feature __ EnableFog
			#pragma shader_feature __ EnableRefraction
			#pragma shader_feature __ DisableUnityLighting
	
			#pragma vertex vertVolumetric
			#pragma fragment fragVolumetric
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			
			#define VOLUMETRIC_MOBILE_VER 
			#define IS_HDRP

			#include "Volumetric.cginc"
				
			ENDHLSL
		}
	}

	SubShader
	{
		Tags{ "LightMode" = "ForwardBase" } // the specular "_WorldSpaceLightPos0" query only works in forward rendering
		LOD 200

		Pass
		{
			CGPROGRAM

			#pragma shader_feature __ EnableLayer0
			#pragma shader_feature __ EnableLayer1
			#pragma shader_feature __ EnableLayer2
			#pragma shader_feature __ EnableCaustic			
			#pragma shader_feature __ EnableFresnel
			#pragma shader_feature __ EnableFresnelUseSkybox
			#pragma shader_feature __ EnableSurfaceMask
			#pragma shader_feature __ EnableInnerLight
			#pragma shader_feature __ EnableSpecular
			#pragma shader_feature __ EnableFog
			//#pragma multi_compile __ EnableRefraction
			#pragma shader_feature __ DisableUnityLighting

			#pragma vertex vertVolumetric
			#pragma fragment fragVolumetric
			#pragma multi_compile_fog

			#define VOLUMETRIC_MOBILE_VER // NO grabpass for mobile.. too heavy!
			#include "Volumetric.cginc"

			ENDCG
		}
	}
	
	CustomEditor "FunktronicLabs.VolumetricCrystalMaterialEditor_HDRP"
	Fallback "VertexLit"
}
