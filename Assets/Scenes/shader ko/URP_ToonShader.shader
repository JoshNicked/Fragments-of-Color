Shader "Custom/URP_Toon_Fixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        _Steps ("Steps", Range(1,10)) = 3
        _Smooth ("Smoothness", Range(0.001,0.5)) = 0.05

        _Ambient ("Ambient", Range(0,1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float _Steps;
            float _Smooth;
            float _Ambient;

            Varyings vert (Attributes v)
            {
                Varyings o;

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 normal = normalize(i.normalWS);

                // Main light (URP)
                Light light = GetMainLight();

                float3 lightDir = normalize(light.direction);
                float NdotL = dot(normal, lightDir);

                // Toon step lighting
                float toon = smoothstep(0, _Smooth, NdotL);
                toon = floor(toon * _Steps) / _Steps;

                // Texture + color
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 col = tex.rgb * _Color.rgb;

                // Apply lighting
                float3 final = col * (toon + _Ambient) * light.color;

                return float4(final, 1);
            }

            ENDHLSL
        }

        // Shadow caster (important so no errors)
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}