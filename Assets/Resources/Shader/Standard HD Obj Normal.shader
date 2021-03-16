Shader "Standard HD Obj Normal" {
    Properties{

        _BaseColor("BaseColor", Color) = (1,1,1,1)
        _BaseColorMap("BaseColorMap", 2D) = "white" {}
        _MaskMap("MaskMap", 2D) = "white" {}
        _NormalMapOS("Object Space Normal (RGB)", 2D) = "bump" {}
        _NormalScale("_NormalScale", Range(0.0, 2.0)) = 1
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }
        SubShader{
        Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
        //Tags{"RenderType" = "Opaque"}
        LOD 400

        CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
    #pragma target 3.0
    #pragma surface surf ObjNormal addshadow  

            float _NormalScale;
        sampler2D _BaseColorMap;
        sampler2D _MaskMap;
        sampler2D _NormalMapOS;


        struct SurfaceOutputObjNormal {
            fixed3 Albedo;
            fixed3 Emission;
            fixed3 Normal;
            fixed Alpha;
            fixed Metallic;
            fixed Smoothness;
            fixed Occlusion;
            fixed3 ObjNormal;
        };


        struct Input {
            float2 uv_BaseColorMap;
        };

        fixed4 _BaseColor;
        float _Cutoff;

        void surf(Input IN, inout SurfaceOutputObjNormal o) {

            fixed4 c = tex2D(_BaseColorMap, IN.uv_BaseColorMap) * _BaseColor;
            fixed4 mask = tex2D(_MaskMap,IN.uv_BaseColorMap);

            o.Albedo = c.rgb;
            o.Metallic = mask.r;
            o.Smoothness = mask.a;
            o.Occlusion = mask.g;
            o.ObjNormal = mul((float3x3)unity_ObjectToWorld, tex2D(_NormalMapOS, IN.uv_BaseColorMap).rgb);
            o.Alpha = c.a;
        }

        inline fixed4 LightingObjNormal(SurfaceOutputObjNormal s, fixed3 lightDir, fixed3 viewDir, fixed atten)
        {
            clip(s.Alpha - _Cutoff);

            viewDir = normalize(viewDir);
            lightDir = normalize(lightDir);
            s.ObjNormal = normalize(s.ObjNormal);

            float NdotL = saturate(dot(s.ObjNormal, lightDir));

            fixed4 c;
            c.rgb = s.Albedo * NdotL * _LightColor0.rgb * atten * 2;
            c.a = 1.0;
            return c;
        }
        ENDCG



        }

            FallBack Off
}