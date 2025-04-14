// Header Guards
#ifndef CAVERN_PROJECTION_MAPPING_HLSL
#define CAVERN_PROJECTION_MAPPING_HLSL

// Define Built-In Macros
#define SHADERPASS SHADERPASS_BLIT

// Include URP library functions.
// URP library functions can be found via the Unity Editor in "Packages/Universal RP/Shader Library/".
// HLSL shader files for URP are in the "Packages/com.unity.render-pipelines.universal/ShaderLibrary/" directory in your project.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Vertex attributes.
// Fields are automatically populated according to their semantic. (https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html)
struct Attributes { // We can name this struct anything we want.
    float3 positionOS : POSITION; // Position in object space.
    float2 uv : TEXCOORD0; // Material texture UVs.
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Data passed from the vertex function to the fragment function.
struct Vert2Frag { // We can name this struct anything we want.
    float4 positionCS : SV_POSITION; // Clip space position must have the semantics SV_POSITION.
    float2 uv : TEXCOORD0; // Render texture UV coordinates.
    UNITY_VERTEX_OUTPUT_STEREO
};

// The vertex function, runs once per vertex.
Vert2Frag Vertex(Attributes input) {
    // Helper function from ShaderVariableFunctions.hlsl in the URP package
    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS);

    // Since this shader is only ever used for blitting, use FullScreenTriangle functions.
    Vert2Frag output;
    output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
    output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
    return output;
}

// The fragment function, runs once per pixel on the screen.
// It must have a float4 return type and have the SV_TARGET semantic.
float4 Fragment(Vert2Frag input) : SV_TARGET {
    return float4(0, 1, 0, 1);
}

#endif // CAVERN_PROJECTION_MAPPING_HLSL