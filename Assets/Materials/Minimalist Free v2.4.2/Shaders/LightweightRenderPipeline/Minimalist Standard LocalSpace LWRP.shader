shader "Minimalist Free/LWRP/Standard LocalSpace" {
	Properties{
		//Texture Module
		[HideInInspector][MaterialToggle] _ShowTexture ("Show Texture settngs", Float ) = 0
		_MainTexture ("Main Texture", 2D) = "white" {}
		_MainTexturePower ("Main Texture Power", Range(-1, 1)) = 0
		//Custom Shading
		[HideInInspector][MaterialToggle] _ShowCustomShading ("Show Custom Shading settngs", Float ) = 0

		[HideInInspector][MaterialToggle] _ShowFront ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(VertexColor, SolidColor, Gradient)] _Shading_F("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_F("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_F("front gizmo", Vector) = (0, 0, 10, 10)

		_Color1_F("Forward Color 1", Color) = (1, 1, 1, 1)
		_Color2_F("Forward Color 2", Color) = (1, 1, 1, 1) 
		_GradientYStartPos_F ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_F("Gradient Height", Float) = 1
		_Rotation_F("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowBack ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_B("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_B("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_B("back gizmo", Vector) = (0, 0, 10, 10)

		_Color1_B("Backward Color 1", Color) = (1, 1, 1, 1)
		_Color2_B("Backward Color 2", Color) = (1, 1, 1, 1)
		_GradientYStartPos_B ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_B("Gradient Height", Float) = 1
		_Rotation_B("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowLeft ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_L("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_L("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_L("Left gizmo", Vector) = (0, 0, 10, 10)

		_Color1_L("Left Color 1", Color) = (1, 1, 1, 1)
		_Color2_L("Left Color 2", Color) = (1, 1, 1, 1)
		_GradientYStartPos_L ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_L("Gradient Height", Float) = 1
		_Rotation_L("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowRight ("Front", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_R("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_R("Shading mode", Float) = 0
		[HideInInspector]_GizmoPosition_R("Right gizmo", Vector) = (0, 0, 10, 10)

		_Color1_R("Right Color 1", Color) = (1, 1, 1, 1)
		_Color2_R("Right Color 2", Color) = (1, 1, 1, 1)
		_GradientYStartPos_R ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_R("Gradient Height", Float) = 1
		_Rotation_R("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowTop ("Top", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_T("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_T("Gradient mode", Float) = 0
		[HideInInspector]_GizmoPosition_T("Top gizmo", Vector) = (0, 0, 10, 10)

		_Color1_T ("Top Color 1", Color) = (1, 1, 1, 1)
		_Color2_T ("Top Color 2", Color) = (1, 1, 1, 1)
		_GradientXStartPos_T ("Gradient start X", Vector) = (0, 0, 0, 0)
		_GradientHeight_T("Gradient Height", Float) = 1
		_Rotation_T("Rotation", Range(0, 360)) = 0

		[HideInInspector][MaterialToggle] _ShowBottom ("Botttom", Float ) = 0
		[HideInInspector][KeywordEnum(Vertex Color, Solid Color, Gradient)] _Shading_D("Shading mode", Float) = 0
		[HideInInspector][KeywordEnum(Use Global, Custom)] _GradSettings_D("Gradient mode", Float) = 0
		[HideInInspector]_GizmoPosition_D("Down gizmo", Vector) = (0, 0, 10, 10)

		_Color1_D ("Bottom Color 1", Color) = (1, 1, 1, 1)
		_Color2_D ("Bottom Color 2", Color) = (1, 1, 1, 1)
		_GradientXStartPos_D ("Gradient start X", Vector) = (0, 0, 0, 0)
		_GradientHeight_D("Gradient Height", Float) = 1
		_Rotation_D("Rotation", Range(0, 360)) = 0
		//Ambient Occlution
		[HideInInspector][MaterialToggle] _ShowAO ("AO", Float ) = 0
		[HideInInspector][MaterialToggle] _AOEnable ("Enable", Float ) = 0
		_AOTexture ("AO Texture", 2D) = "white" {}
		_AOColor ("AO Color", Color) = (0, 0, 0, 1)
		_AOPower ("AO Texture Power", Range(0, 3)) = 1
		[HideInInspector][KeywordEnum(uv0, uv1)] _AOuv("UV", Float) = 0
		//Lightmap
		[HideInInspector][MaterialToggle] _ShowLMap ("Lightmap", Float ) = 0
		[HideInInspector][MaterialToggle] _LmapEnable ("Enable", Float ) = 0
		[HideInInspector][KeywordEnum(Add, Multiply, AO)] _LmapBlendingMode("Blend Mode", Float) = 0
		_LMColor ("LightMap Color", Color) = (0, 0, 0, 1)
		_LMPower ("LightMap Power", Range(0, 5.0)) = 1
		//Fog
		[HideInInspector][MaterialToggle] _ShowFog ("ShowFog", Float ) = 0
		[MaterialToggle] _UnityFogEnable ("Fog", Float ) = 0
		[MaterialToggle] _HFogEnable ("Fog", Float ) = 0
		_Color_Fog ("Fog Color",     Color) = (0.5, 0.5, 0.5, 1)
		_FogYStartPos ("Gradient start Y", Float) = 0
		_FogHeight("Gradient Height", Float) = 10
		//Color Correction
		[HideInInspector][MaterialToggle] _ShowColorCorrection ("Color Correction", Float ) = 0
		[HideInInspector][MaterialToggle] _ColorCorrectionEnable ("Enable", Float ) = 0
		_TintColor ("Tint Color", Color) = (0, 0, 0, 1)
		_Saturation ("Saturation", Range(0, 1)) = 0
		_Brightness ("Brightness", Range(-1, 1)) = 0
		//OtherSettings
		[MaterialToggle] _OtherSettings ("OtherSettings", Float ) = 0

		[HideInInspector][MaterialToggle] _ShowGlobalGradientSettings ("Show Global Gradient Settings", Float ) = 0
		_GradientYStartPos_G ("Gradient start Y", Vector) = (0, 0, 0, 0)
		_GradientHeight_G("Gradient Height", Float) = 1
		_Rotation_G("Rotation", Float) = 0

		[HideInInspector][MaterialToggle] _ShowAmbientSettings ("Show Ambient Settings", Float ) = 0
		_AmbientColor("Ambient Color",Color) = (0, 0, 0, 0)
		_AmbientPower("Ambient Power", Range(0, 2.0)) = 0

		[HideInInspector][MaterialToggle] _RimEnable ("Use Rim", Float ) = 0
		_RimColor ("Rim Color", Color) = (0, 0, 0, 1)
		_RimPower ("Power", Range(0, 4)) = 1
		
		[MaterialToggle] _RealtimeShadow ("RealTime Shadow", Float ) = 0
		_ShadowColor("ShadowColor",    Color) = (0, 0, 0, 1)
		_ShadowInfluence ("Influence", Range(0.001, 2)) = 0
		_ShadowBlend ("Strength", Range(0, 1)) = 0

		[KeywordEnum(World Space, Local Space)] _GradientSpace("Gradient Space", Float) = 0
		[MaterialToggle] _DontMix ("Don't Mix Color", Float ) = 0
		_Fade ("Fade", Range(0, 1)) = 1
		
		[KeywordEnum(Opaque, Transparent)] _Mode("Mode", Float) = 0
		[HideInInspector] _SrcBlend ("_src", Float) = 1.0
		[HideInInspector] _DstBlend ("_dst", Float) = 0.0
		[HideInInspector] _ZWrite ("_zWrite", Float) = 0.0
		[HideInInspector] _ZWriteOverride ("_zWrite", Float) = 0.0
		_Cull("Cull", Float) = 2
	}

	SubShader{	
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True" "DisableBatching" = "True"}
		
		Pass{
			Name "StandardPass"
			Tags {"RenderType"="Opaque" "LightMode"="LightweightForward"}
			Blend [_SrcBlend] [_DstBlend]
			cull back
			ZWrite [_ZWrite]
			ZTest LEqual
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase_fullshadows
			#pragma multi_compile_fog
			#pragma fragmentoption ARB_precision_hint_fastest
			//shader_features
			#pragma shader_feature TEXTUREMODULE_ON
			
			#pragma shader_feature FRONTGRADIENT
			#pragma shader_feature BACKGRADIENT
			#pragma shader_feature LEFTGRADIENT
			#pragma shader_feature RIGHTGRADIENT
			#pragma shader_feature TOPGRADIENT
			#pragma shader_feature BOTTOMGRADIENT
			
			#pragma shader_feature FRONTSOLID
			#pragma shader_feature BACKSOLID
			#pragma shader_feature LEFTSOLID
			#pragma shader_feature RIGHTSOLID
			#pragma shader_feature TOPSOLID
			#pragma shader_feature BOTTOMSOLID
			
			#pragma shader_feature AO_ON_UV0
			#pragma shader_feature AO_ON_UV1
			
			#pragma shader_feature LIGHTMAP_ADD
			#pragma shader_feature LIGHTMAP_MULTIPLY
			#pragma shader_feature LIGHTMAP_AO
			
			#pragma shader_feature HEIGHT_FOG
			#pragma shader_feature UNITY_FOG
			#pragma shader_feature SHADOW_ON
			
			#pragma shader_feature COLORCORRECTION_ON
			#pragma shader_feature USERIM
			#pragma shader_feature DONTMIX
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			
			//Uniforms
			#if TEXTUREMODULE_ON
				uniform sampler2D _MainTexture; uniform half4 _MainTexture_ST;
				uniform half _MainTexturePower;
			#endif
			
			#if FRONTSOLID || FRONTGRADIENT
				uniform half3 _Color1_F;
				#if FRONTGRADIENT
					uniform half3 _Color2_F;
					uniform half2 _GradientYStartPos_F;
					uniform half _GradientHeight_F;
					uniform half _Rotation_F;
				#endif
			#endif
			
			#if BACKSOLID || BACKGRADIENT
				uniform half3 _Color1_B;
				#if BACKGRADIENT
					uniform half3 _Color2_B;
					uniform half2 _GradientYStartPos_B;
					uniform half _GradientHeight_B;
					uniform half _Rotation_B;
				#endif
			#endif
			
			#if LEFTSOLID || LEFTGRADIENT
				uniform half3 _Color1_L;
				#if LEFTGRADIENT
					uniform half3 _Color2_L;
					uniform half2 _GradientYStartPos_L;
					uniform half _GradientHeight_L;
					uniform half _Rotation_L;
				#endif
			#endif
			
			#if RIGHTSOLID || RIGHTGRADIENT
				uniform half3 _Color1_R;
				#if RIGHTGRADIENT
					uniform half3 _Color2_R;
					uniform half2 _GradientYStartPos_R;
					uniform half _GradientHeight_R;
					uniform half _Rotation_R;
				#endif
			#endif
			
			#if TOPSOLID || TOPGRADIENT
				uniform half3 _Color1_T;
				#if TOPGRADIENT
					uniform half3 _Color2_T;
					uniform half2 _GradientXStartPos_T;
					uniform half _GradientHeight_T;
					uniform half _Rotation_T;
				#endif
			#endif
			
			#if BOTTOMSOLID || BOTTOMGRADIENT
				uniform half3 _Color1_D;
				#if BOTTOMGRADIENT
					uniform half3 _Color2_D;
					uniform half2 _GradientXStartPos_D;
					uniform half _GradientHeight_D;
					uniform half _Rotation_D;
				#endif
			#endif
			
			#if AO_ON_UV0 || AO_ON_UV1
				uniform sampler2D _AOTexture; uniform half4 _AOTexture_ST;
				uniform half3 _AOColor;
				uniform half _AOPower;
			#endif
			
			#if LIGHTMAP_AO
				uniform half3 _LMColor;
			#endif
			
			uniform half _LMPower;
			
			#if HEIGHT_FOG
				uniform half3 _Color_Fog;
				uniform half _FogYStartPos;
				uniform half _FogHeight;
			#endif
			
			#if USERIM
				uniform half3 _RimColor;
				uniform half _RimPower;
			#endif
			
			#if COLORCORRECTION_ON
				uniform half3 _TintColor;
				uniform half _Saturation;
				uniform half _Brightness;
			#endif
			
			uniform half3 _AmbientColor;
			uniform half _AmbientPower;
			#if SHADOW_ON
				uniform half3 _ShadowColor;
				uniform half _ShadowInfluence;
				uniform half _ShadowBlend;
			#endif
			uniform half _GradientSpace;
			uniform half _Fade;
			
			//Direction vector constants
			static const half3 FrontDir = half3(0, 0, 1);
			static const half3 BackDir = half3(0, 0, -1);
			static const half3 LeftDir = half3(1, 0, 0);
			static const half3 RightDir = half3(-1, 0, 0);
			static const half3 TopDir = half3(0, 1, 0);
			static const half3 BottomDir = half3(0, -1, 0);
			static const half3 whiteColor = half3(1, 1, 1);
			
			struct vertexInput{
				float4 vertex : POSITION;
				half3 normal : NORMAL;
				half3 vColor : COLOR;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
			};
			
			struct vertexOutput{
				float4 pos : POSITION;
				float4 worldPos : TEXCOORD0;
				#if TEXTUREMODULE_ON
					float2 uv : TEXCOORD1;
				#endif
				
				#if LIGHTMAP_ADD || LIGHTMAP_MULTIPLY || LIGHTMAP_AO
					float2 lightmapUV : TEXCOORD2;
				#endif
				float3 customLighting : COLOR0;
				
				#if AO_ON_UV0 || AO_ON_UV1  
					float2 aouv : TEXCOORD4;
				#endif
				#if UNITY_FOG
					UNITY_FOG_COORDS(5)
				#endif
				#if SHADOW_ON
					LIGHTING_COORDS(6, 7)
					float3 normalDir : TEXCOORD8;
				#endif
				#if USERIM
					float rimCol : COLOR1;
				#endif
			};
			
			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				half3 normal = normalize(mul(unity_ObjectToWorld, half4(v.normal, 0))).xyz;
				
				//Maping Texture
				#if TEXTUREMODULE_ON
					o.uv = TRANSFORM_TEX(v.uv0, _MainTexture);
				#endif
				
				//Maping AO maps
				#if AO_ON_UV0
					o.aouv = TRANSFORM_TEX(v.uv0, _AOTexture);
				#endif
				#if AO_ON_UV1
					o.aouv = TRANSFORM_TEX(v.uv1, _AOTexture);
				#endif
				
				
				//Calculating custom shadings
				half3 colorFront, colorBack, colorLeft, colorRight, colorTop, colorDown;
				half dirFront  = max(dot(normal, FrontDir),  0.0);
				half dirBack   = max(dot(normal, BackDir),   0.0);
				half dirLeft   = max(dot(normal, LeftDir),   0.0);
				half dirRight  = max(dot(normal, RightDir),  0.0);
				half dirTop    = max(dot(normal, TopDir),    0.0);
				half dirBottom = max(dot(normal, BottomDir), 0.0);
				
				colorFront = colorBack = colorLeft = colorRight = colorTop = colorDown = v.vColor;
				
				#if FRONTSOLID 
					colorFront = _Color1_F; 
				#endif
				#if BACKSOLID
					colorBack = _Color1_B;
				#endif
				#if LEFTSOLID
					colorLeft = _Color1_L;
				#endif
				#if RIGHTSOLID
					colorRight = _Color1_R;
				#endif
				#if TOPSOLID
					colorTop = _Color1_T;
				#endif
				#if BOTTOMSOLID
					colorDown = _Color1_D;
					
				#endif
				
				float4 GradPos = v.vertex;
				
				half3 Maincolor;
				#if DONTMIX
					Maincolor = colorFront * dirFront + colorBack * dirBack + colorLeft * dirLeft + colorRight * dirRight + colorTop * dirTop + colorDown * dirBottom;
				#else
					Maincolor = lerp(colorFront, whiteColor, 1-dirFront) * lerp(colorBack, whiteColor, 1-dirBack) * lerp(colorLeft, whiteColor, 1-dirLeft) * lerp(colorRight, whiteColor, 1-dirRight) * lerp(colorTop, whiteColor, 1-dirTop) * lerp(colorDown, whiteColor, 1-dirBottom);
				#endif
				o.customLighting = Maincolor + (_AmbientColor * _AmbientPower);
				
				//Lightmap
				#if	LIGHTMAP_ADD || LIGHTMAP_MULTIPLY || LIGHTMAP_AO
					o.lightmapUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				
				//Apply Unity fog
				#if UNITY_FOG
					UNITY_TRANSFER_FOG(o, o.pos);
				#endif
				
				return o;
			}
			
			half4 frag(vertexOutput i) : COLOR
			{
				half4 Result = half4(whiteColor,1);
				//applying main texture
				#if TEXTUREMODULE_ON
					half4 _MainTexture_var = tex2D(_MainTexture, i.uv) + half4(_MainTexturePower, _MainTexturePower, _MainTexturePower, 0);
					_MainTexture_var = clamp(_MainTexture_var, 0.0, 1.0);
					Result *= _MainTexture_var;
				#endif
				//Applying AO
				#if AO_ON_UV0
					half4 AOTexVar = lerp(half4(0, 0, 0, 1), half4(whiteColor, 1), lerp(half4(1,1,1,1), tex2D(_AOTexture, i.aouv), 1));
					Result *= AOTexVar;
				#endif
				#if AO_ON_UV1
					half4 AOTexVar = lerp(half4(0, 0, 0, 1), half4(whiteColor, 1), lerp(half4(1,1,1,1), tex2D(_AOTexture, i.aouv), 1));
					Result *= AOTexVar;
				#endif
				//applying custom Lighting Data
				Result *= half4(i.customLighting, 1);
				
				//Calculating Lightmap
				#if LIGHTMAP_ADD
					Result += (half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)), 0));
					Result = clamp(Result, 0.0, 1.0);
				#endif
				#if LIGHTMAP_MULTIPLY
					Result *= half4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)), 0);
					Result = clamp(Result, 0.0, 1.0);
				#endif

				//Unity Fog
				#if UNITY_FOG
					UNITY_APPLY_FOG(i.fogCoord, Result);
				#endif
				Result *= half4(1, 1, 1, _Fade);
				return  Result;
			}
			ENDCG
		}
	}
	FallBack "Standard"
	CustomEditor "Minimalist.MinimalistStandardEditor"
}
