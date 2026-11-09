Shader "CampusGlitch/AtlasAlpha" {
 Properties {
  [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
  _Color ("Tint", Color) = (1,1,1,1)
  _Cutoff ("Minimum authored alpha", Range(0,1)) = 0.56
 }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="True" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   half4 _Color; half _Cutoff;
   CBUFFER_END
   struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
   Varyings Vert(Attributes input) {
    Varyings output; output.positionCS=TransformObjectToHClip(input.positionOS);
    output.uv=input.uv; output.color=input.color*_Color; return output;
   }
   half4 Frag(Varyings input):SV_Target {
    half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*input.color;
    clip(color.a-_Cutoff); return color;
   }
   ENDHLSL
  }
 }
}
