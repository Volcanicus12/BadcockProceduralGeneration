Shader "Custom/Terrain"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxColorCount = 8;//8 is max number of colors we can have

        int baseColorCount;
        float3 baseColors[maxColorCount];//array like C# but [] is after var name
        float baseStartHeights[maxColorCount];

        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;//helps with finding world height
        };


        //a = min, b = max, value = cur val
        float inverseLerp(float a, float b, float value) {//up here bc func has to be declared before calling it
            return saturate((value - a) / (b - a));//saturate clamps to 0,1
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)//called for every function that the pixel is visible
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            for (int i = 0; i < baseColorCount; i++) {
                //set cur color if above corresponding base start height
                float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));
                o.Albedo = o.Albedo *(1 - drawStrength) + baseColors[i] * drawStrength;//first half is to ensure that we don't get a black color bc negatives
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
