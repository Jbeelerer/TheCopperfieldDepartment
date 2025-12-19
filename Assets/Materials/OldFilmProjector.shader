Shader "UI/OldFilmProjector"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _TintColor ("Tint Color", Color) = (1,1,1,1)

        _ScratchIntensity ("Scratch Intensity", Range(0,1)) = 0.4
        _ScratchDensity ("Scratch Density", float) = 20

        _FlickerAmount ("Flicker Amount", Range(0,1)) = 0.2
        _Speed ("Animation Speed", float) = 1

        _JitterAmount ("Frame Jitter (Horizontal)", Range(0,0.01)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _TintColor;

            float _ScratchIntensity;
            float _ScratchDensity;

            float _FlickerAmount;
            float _Speed;
            float _JitterAmount;

            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 78.233))) * 43758.5453);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y * _Speed;

                // ---------------------------
                // FRAME JITTER
                // ---------------------------
                float jitter = (rand(float2(t, t * 1.37)) - 0.5) * _JitterAmount;

                fixed4 color = tex2D(_MainTex, i.uv + float2(jitter, 0));

                // ---------------------------
                // VERTICAL SCRATCHES
                // ---------------------------
                float scratchX = floor(i.uv.x * _ScratchDensity);
                float scratchRand = rand(float2(scratchX, t * 0.15));
                float scratch = step(0.985, scratchRand);

                color.rgb += scratch * _ScratchIntensity;

               // ---- CHOPPY OLD-PROJECTOR FLICKER (slower) ----

    // slower time base
    float ft = t * 0.8;

    // random choppy flicker
    float f1 = rand(float2(floor(ft * 6.0), 12.7));
    float f2 = rand(float2(floor(ft * 4.0), 3.1));

    // add slight smooth fade between steps
    float blend = frac(ft * 2.0);
    float flicker = lerp(f1, f2, blend);

    // apply flicker
    color.rgb *= 1.0 + (flicker - 0.5) * _FlickerAmount;


                // ---------------------------
                // TINT
                // ---------------------------
                color.rgb *= _TintColor.rgb;

                return color;
            }
            ENDCG
        }
    }
}
