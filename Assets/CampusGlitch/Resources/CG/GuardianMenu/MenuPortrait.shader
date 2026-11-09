Shader "Hidden/CampusGlitch/MenuPortrait" {
 Properties {_MainTex ("Atlas",2D)="white"{} _Cutoff ("Corte de transparencia",Range(0,1))=.56}
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float _Cutoff;
   fixed4 frag(v2f_img input):SV_Target{
    fixed4 color=tex2D(_MainTex,input.uv);
    return color.a>=_Cutoff?fixed4(color.rgb,1):fixed4(0,0,0,0);
   }
   ENDCG
  }
 }
}
