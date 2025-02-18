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

// Other Material Properties
int _EnableStereo;

float _CavernHeight;
float _CavernRadius;
float _CavernAngle;
float _CavernElevation;

// Head Tracking Properties
float3 _HeadPosition; // Vector3

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

// The fragment function, runs once per pixel on the screen.
// It must have a float4 return type and have the SV_TARGET semantic.
// Values in the Vert2Frag have been interpolated based on each pixel's position.
float4 Fragment(Vert2Frag input) : SV_TARGET {
    // Split the screen into 2 halves, top and bottom.
    // For stereoscopic rendering, the top will render the left eye, the bottom will render the right eye.
    // For monoscopic rendering, both halves will render the same thing.
    const bool isLeftEye = 0.5 < input.uv.y;

    float2 ratio = input.uv;
    // Convert the UV.x from the [0, 1] range to the [-1, 1] range.
    ratio.x = ratio.x * 2.0f - 1.0f;
    // For the left eye, convert the UV's y component from the [0.5, 1] range to the [0, 1] range.
    // For the right eye, convert the UV's y component from the [0, 0.5] range to the [0, 1] range.
    ratio.y = isLeftEye ? (ratio.y - 0.5) * 2.0f : ratio.y * 2.0f;

    // Take note that angle 0 points down the Z-axis, not the X-axis.
    float screenAngle = ratio.x * _CavernAngle * 0.5f; // Horizontal Screen Angle (Degrees)
    float screenAngleRad = radians(screenAngle);
    float3 eyeToScreen = normalize(float3(_CavernRadius * sin(screenAngleRad) - _HeadPosition.x,
                                          ratio.y * _CavernHeight + _CavernElevation - _HeadPosition.y,
                                          _CavernRadius * cos(screenAngleRad) - _HeadPosition.z));

    // Monoscopic
    if (!_EnableStereo) {
        return SAMPLE_TEXTURECUBE(_CubemapNorth, sampler_CubemapNorth, eyeToScreen);
    }

    float3 forwardDir = float3(0.0f, 0.0f, 1.0f);
    float3 eyeToScreenXZ = normalize(float3(eyeToScreen.x, 0.0f, eyeToScreen.z));
    float directionAngle = degrees(acos(dot(forwardDir, eyeToScreenXZ))) * ((eyeToScreenXZ.x > 0.0f) ? 1.0f : -1.0f);

    // For debugging purposes.
    bool debugColour = false;
    float4 rightColour = float4(1.0f, 0.0f, 0.0f, 1.0f);
    float4 leftColour = float4(0.0f, 1.0f, 0.0f, 1.0f);
    float4 frontColour = float4(0.0f, 0.0f, 1.0f, 1.0f);
    float4 backColour = float4(1.0f, 0.0f, 1.0f, 1.0f);
    
    // Left Eye
    if (isLeftEye) {
        // Rear direction, relative to player.
        if (directionAngle > 135.0f || directionAngle < -135.0f) {
            if (debugColour) return rightColour;
            return SAMPLE_TEXTURECUBE(_CubemapEast, sampler_CubemapEast, eyeToScreen);
        }
        
        // Left direction, relative to player.
        if (directionAngle < -45.0f) {
            if (debugColour) return backColour;
            return SAMPLE_TEXTURECUBE(_CubemapSouth, sampler_CubemapSouth, eyeToScreen);
        }
        
        // Physcial screen right quadrant relative to head position.
        if (directionAngle > 45.0f) {
            if (debugColour) return frontColour;
            return SAMPLE_TEXTURECUBE(_CubemapNorth, sampler_CubemapNorth, eyeToScreen);
        }
        
        // Physcial screen front quadrant relative to head position.
        if (debugColour) return leftColour;
        return SAMPLE_TEXTURECUBE(_CubemapWest, sampler_CubemapWest, eyeToScreen);
    }
    
    // Right Eye
    // Rear direction, relative to player.
    if (directionAngle > 135.0f || directionAngle < -135.0f) {
        if (debugColour) return leftColour;
        return SAMPLE_TEXTURECUBE(_CubemapWest, sampler_CubemapWest, eyeToScreen);
    }
        
    // Left direction, relative to player.
    if (directionAngle < -45.0f) {
        if (debugColour) return frontColour;
        return SAMPLE_TEXTURECUBE(_CubemapNorth, sampler_CubemapNorth, eyeToScreen);
    }
    
    // Physcial screen right quadrant relative to head position.
    if (directionAngle > 45.0f) {
        if (debugColour) return backColour;
        return SAMPLE_TEXTURECUBE(_CubemapSouth, sampler_CubemapSouth, eyeToScreen);
    }

    // Physcial screen front quadrant relative to head position.
    if (debugColour) return rightColour;
    return SAMPLE_TEXTURECUBE(_CubemapEast, sampler_CubemapEast, eyeToScreen);
}

#endif // CAVERN_PROJECTION_HLSL