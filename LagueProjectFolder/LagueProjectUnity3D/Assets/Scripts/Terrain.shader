Shader "Custom/Terrain"
{
    Properties
    {
        testTexture("Texture", 2D) = "white"{}
        testScale("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        //hsls
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxLayerCount = 8;//8 is max number of colors we can have
        const static float epsilon = 1E-4;//very small value

        int layerCount;
        float3 baseColors[maxLayerCount];//array like C# but [] is after var name
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColorStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        float minHeight;
        float maxHeight;

        sampler2D testTexture;
        float testScale;

        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        struct Input
        {
            float3 worldPos;//helps with finding world height
            float3 worldNormal;
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

        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
            //we need to make it so that the xyz doesn't stretch...to do this we need a lot of blending
            float3 scaledWorldPos = worldPos / scale;

            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;//allows us to sample a texture at a given point.
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            //add all up to create final color
            return xProjection + yProjection + zProjection;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)//called for every function that the pixel is visible
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);//we use absolute value bc we don't care if pos or neg
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;//makes it so our values don't exceed 1 and cause brightness

            for (int i = 0; i < layerCount; i++) {
                //set cur color if above corresponding base start height
                float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - baseStartHeights[i]);//blending with strength
                
                float3 baseColor = baseColors[i] * baseColorStrength[i];
                float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrength[i]);

                o.Albedo = o.Albedo *(1 - drawStrength) + (baseColor + textureColor) * drawStrength;//first half is to ensure that we don't get a black color bc negatives
            }


        }
        ENDCG
    }
    FallBack "Diffuse"
}
