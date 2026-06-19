Shader "Custom/FOV_Stencil_Writer"
{
    SubShader
    {
        // Se renderiza antes que los objetos transparentes y no proyecta sombras
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
        
        Pass
        {
            ZWrite Off
            ColorMask 0 // Esto es lo que lo hace invisible visualmente

            // Escribe un 1 en el Stencil Buffer donde exista el mesh
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            half4 frag(v2f i) : SV_Target { return half4(0,0,0,0); }
            ENDCG
        }
    }
}