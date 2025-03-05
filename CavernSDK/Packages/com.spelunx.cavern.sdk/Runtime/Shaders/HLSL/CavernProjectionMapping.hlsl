// Header Guards
#ifndef CAVERN_PROJECTION_HLSL
#define CAVERN_PROJECTION_HLSL

// Pull in URP library functions and our own common functions.
// URP library functions can be found via the Unity Editor in "Packages/Universal RP/Shader Library/".
// The HLSL shader files for the URP are in the Packages/com.unity.render-pipelines.universal/ShaderLibrary/ folder in your project.
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Textures
TEXTURECUBE(_CubemapNorth); // Also used for monoscopic rendering.
SAMPLER(sampler_CubemapNorth);
float4 _CubemapNorth_ST;

TEXTURECUBE(_CubemapSouth);
SAMPLER(sampler_CubemapSouth);
float4 _CubemapSouth_ST;

TEXTURECUBE(_CubemapEast);
SAMPLER(sampler_CubemapEast);
float4 _CubemapEast_ST;

TEXTURECUBE(_CubemapWest);
SAMPLER(sampler_CubemapWest);
float4 _CubemapWest_ST;

// Cavern Dimensions Uniforms
float _CavernHeight;
float _CavernRadius;
float _CavernAngle;
float _CavernElevation;

// Head Tracking Uniforms
float3 _HeadPosition;

// Stereoscopic Rendering Uniforms
int _IsStereoscopic;
int _EnableHighAccuracy;
float _InterpupillaryDistance;

// This attributes struct receives data about the mesh we are currently rendering.
// Data is automatically placed in the fields according to their semantic.
// List of available semantics: https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
struct Attributes { // We can name this struct anything we want.
    float3 positionOS : POSITION; // Position in object space.
    float2 uv : TEXCOORD0; // Material texture UVs.
};

// A struct to define the variables we will pass from the vertex function to the fragment function.
struct Vert2Frag { // We can name this struct anything we want.
    // The output variable of the vertex shader must have the semantics SV_POSITION.
    // This value should contain the position in clip space when output from the vertex function.
    // It will be transformed into pixel position of the current fragment on the screen when read from the fragment function.
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0; // By the variable a TEXCOORDN semantic, Unity will automatically interpolate it for each fragment.
};

// The vertex function, runs once per vertex.
Vert2Frag Vertex(Attributes input) {
    // GetVertexPositionInputs is from ShaderVariableFunctions.hlsl in the URP package.
    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS); // Apply the model-view-projection transformations onto our position.

    Vert2Frag output;
    output.positionCS = positionInputs.positionCS; // Set the clip space position.
    output.uv = input.uv;

    return output;
}

float4 SampleLeftEye(float3 headToScreen, float angleToFragment, float3 ipdOffsetNorth, float3 ipdOffsetEast)
{
    // Physcial screen rear quadrant relative to head position.
    if (angleToFragment > 135.0f || angleToFragment < -135.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapEast, sampler_CubemapEast, headToScreen - ipdOffsetEast);
    }
    // Physcial screen left quadrant relative to head position.
    if (angleToFragment < -45.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapSouth, sampler_CubemapSouth, headToScreen + ipdOffsetNorth);
    }
    // Physcial screen right quadrant relative to head position.
    if (angleToFragment > 45.0f)  {
        return SAMPLE_TEXTURECUBE(_CubemapNorth, sampler_CubemapNorth, headToScreen - ipdOffsetNorth);
    }
    // Physcial screen front quadrant relative to head position.
    return SAMPLE_TEXTURECUBE(_CubemapWest, sampler_CubemapWest, headToScreen + ipdOffsetEast);
}

float4 SampleRightEye(float3 headToScreen, float angleToFragment, float3 ipdOffsetNorth, float3 ipdOffsetEast)
{
    // Physcial screen rear quadrant relative to head position.
    if (angleToFragment > 135.0f || angleToFragment < -135.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapWest, sampler_CubemapWest, headToScreen + ipdOffsetEast);
    }
    // Physcial screen left quadrant relative to head position.
    if (angleToFragment < -45.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapNorth, sampler_CubemapNorth, headToScreen - ipdOffsetNorth);
    }
    // Physcial screen right quadrant relative to head position.
    if (angleToFragment > 45.0f) {
        return SAMPLE_TEXTURECUBE(_CubemapSouth, sampler_CubemapSouth, headToScreen + ipdOffsetNorth);
    }
    // Physcial screen front quadrant relative to head position.
    return SAMPLE_TEXTURECUBE(_CubemapEast, sampler_CubemapEast, headToScreen - ipdOffsetEast);
}

// The fragment function, runs once per pixel on the screen.
// It must have a float4 return type and have the SV_TARGET semantic.
// Values in the Vert2Frag have been interpolated based on each pixel's position.
float4 Fragment(Vert2Frag input) : SV_TARGET {
    // Split the screen into 2 halves, top and bottom.
    // For stereoscopic rendering, the top will render the left eye, the bottom will render the right eye.
    // For monoscopic rendering, both halves will render the same thing.
    const bool isLeftEye = 0.5f < input.uv.y;

    float2 ratio = input.uv;
    // Convert the UV.x from the [0, 1] range to the [-1, 1] range.
    ratio.x = ratio.x * 2.0f - 1.0f;
    // For the left eye, convert the UV's y component from the [0.5, 1] range to the [0, 1] range.
    // For the right eye, convert the UV's y component from the [0, 0.5] range to the [0, 1] range.
    ratio.y = isLeftEye ? (ratio.y - 0.5) * 2.0f : ratio.y * 2.0f;

    // Take note that angle 0 points down the Z-axis, not the X-axis.
    float screenAngle = radians(ratio.x * _CavernAngle * 0.5f);
    float3 headToScreen = float3(_CavernRadius * sin(screenAngle), ratio.y * _CavernHeight, _CavernRadius * cos(screenAngle)) - _HeadPosition;

    // Monoscopic mode.
    if (!_IsStereoscopic) {
        return SAMPLE_TEXTURECUBE(_CubemapNorth, sampler_CubemapNorth, headToScreen);
    }

    // Stereoscopic mode.
    const float3 forwardDir = float3(0.0f, 0.0f, 1.0f);
    float3 headToScreenXZ = normalize(float3(headToScreen.x, 0.0f, headToScreen.z));
    float angleToFragment = degrees(acos(dot(forwardDir, headToScreenXZ))) * ((headToScreenXZ.x > 0.0f) ? 1.0f : -1.0f);
    const float3 ipdOffsetNorth = _EnableHighAccuracy ? float3(0.0f, 0.0f, _InterpupillaryDistance * 0.5f) : float3(0.0f, 0.0f, 0.0f);
    const float3 ipdOffsetEast = _EnableHighAccuracy ? float3(_InterpupillaryDistance * 0.5f, 0.0f, 0.0f) : float3(0.0f, 0.0f, 0.0f);
    
    return isLeftEye ? SampleLeftEye(headToScreen, angleToFragment, ipdOffsetNorth, ipdOffsetEast) : SampleRightEye(headToScreen, angleToFragment, ipdOffsetNorth, ipdOffsetEast);
}

#endif // CAVERN_PROJECTION_HLSL