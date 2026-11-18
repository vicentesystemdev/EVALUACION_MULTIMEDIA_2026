Shader "Hidden/CampusGlitch/MenuGlitch" {
 Properties {_MainTex("Retrato",2D)="white"{} _Strength("Intensidad",Range(0,1))=0 _Phase("Fase",Float)=0}
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_TexelSize;float _Strength,_Phase;
   float noise(float row,float tick){return frac(sin(row*13.13+tick*17.17)*947.0);}
   float2 bounded(float2 uv,float2 cell){
    // A 4 x 2 atlas: an offset never samples the neighboring pose or variant.
    float2 edge=_MainTex_TexelSize.xy*.5;
    return clamp(uv,cell*float2(.25,.5)+edge,(cell+1)*float2(.25,.5)-edge);
   }
   fixed4 frag(v2f_img input):SV_Target{
    float2 cell=min(floor(input.uv*float2(4,2)),float2(3,1));
    float row=floor(frac(input.uv.y*2)*48),tick=floor(_Phase*24);
    float slice=step(.72,noise(row,tick));
    float jitter=(noise(row+19,tick)*2-1)*9*_Strength*slice*_MainTex_TexelSize.x;
    float drift=(1.5+3*noise(row+7,tick))*_Strength*_MainTex_TexelSize.x;
    float2 uv=input.uv+float2(jitter,0);
    fixed4 center=tex2D(_MainTex,bounded(uv,cell));
    fixed4 red=tex2D(_MainTex,bounded(uv+float2(drift,0),cell));
    fixed4 blue=tex2D(_MainTex,bounded(uv-float2(drift,0),cell));
    float scan=1-.12*_Strength*step(.82,frac(input.uv.y*_MainTex_TexelSize.w*.25));
    return fixed4(fixed3(red.r,center.g,blue.b)*scan,max(center.a,max(red.a,blue.a)));
   }
   ENDCG
  }
 }
}
