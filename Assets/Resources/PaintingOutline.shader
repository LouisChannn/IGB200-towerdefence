Shader "HueHarmony/PaintingOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Color;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 colour:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 colour:COLOR; };
            Output vert(Input v) { Output o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.colour=v.colour*_Color; return o; }
            fixed4 frag(Output i):SV_Target
            {
                fixed4 source=tex2D(_MainTex,i.uv);
                float ink=1-smoothstep(.20,.45,max(source.r,max(source.g,source.b)));
                return fixed4(.055,.045,.055,source.a*ink*i.colour.a);
            }
            ENDHLSL
        }
    }
}
