using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Spelunx {
    public class CavernRenderer : MonoBehaviour {
        public enum StereoscopicMode {
            Mono, // Monoscopic mode. No 3D effect.
            Stereo, // Stereoscopic mode. Gives a 3D-movie effect when wearing 3D glasses.
        }

        public enum CubemapResolution {
            Low = 1024,
            Mid = 2048,
            High = 4096,
            VeryHigh = 8192,
        }

        private enum CubemapIndex {
            North = 0, // Also used for monoscopic.
            South,
            East,
            West,

            Num,
        }

        [Header("Camera Settings")]
        /// Stereoscopic mode to render the 
        [SerializeField] private StereoscopicMode stereoMode = StereoscopicMode.Mono;
        [SerializeField] private CubemapResolution cubemapResolution = CubemapResolution.Mid;
        /// Interpupillary Distance (IPD) in metres.
        [SerializeField, Range(0.055f, 0.075f)] private float interpupillaryDistance = 0.065f;
        /// Cavern physical screen height in metres.
        [SerializeField, Min(0.1f)] private float cavernHeight = 2.0f;
        /// Cavern physical screen radius in metres.
        [SerializeField, Min(0.1f)] private float cavernRadius = 3.0f;
        /// Cavern physical screen angle in degrees.
        [SerializeField, Min(0.1f)] private float cavernAngle = 270.0f;
        /// Cavern physical screen elevation in metres, relative to the player's feet.
        [SerializeField, Range(-0.5f, 0.5f)] private float cavernElevation = 0.0f;

        [Header("Head Tracking")]
        /// If set to true, the head position will be clamped to within the the radius of the screen.
        [SerializeField] private bool clampHeadPosition = true;
        /// Sets the clamping radius of the head. For example, if clampHeadRatio = 0.8 and cavernRadius = 3, the head will be clamped to a radius of 2.4.
        [SerializeField, Range(0.0f, 1.0f)] private float clampHeadRatio = 0.8f;

        [Header("References (Do not edit!)")]
        [SerializeField] private Transform head;
        [SerializeField] private Camera eye;
        [SerializeField] private Shader shader;

        [Header("For Debugging Purposes")]
        [SerializeField] private bool enableDirectionDebug = false;

        // Internal variables.
        private RenderTexture[] cubemaps;
        private RenderTexture screenViewerTexture;
        private Material material;

        public CubemapResolution GetCubemapResolution() { return cubemapResolution; }
        public StereoscopicMode GetStereoscopicMode() { return stereoMode; }
        public float GetIPD() { return interpupillaryDistance; }
        public float GetCavernHeight() { return cavernHeight; }
        public float GetCavernRadius() { return cavernRadius; }
        public float GetCavernAngle() { return cavernAngle; }
        public float GetCavernElevation() { return cavernElevation; }
        public RenderTexture GetScreenViewerTexture() { return screenViewerTexture; }
        public float GetAspectRatio() { return ((cavernAngle / 360.0f) * Mathf.PI * cavernRadius * 2.0f) / cavernHeight; }
        public GameObject GetHead() { return head.gameObject; }

        private void OnEnable() {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable() {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void Awake() {
            // Initialise render textures.
            cubemaps = new RenderTexture[(int)CubemapIndex.Num];
            for (int i = 0; i < (int)CubemapIndex.Num; ++i) {
                cubemaps[i] = new RenderTexture((int)cubemapResolution, (int)cubemapResolution, 32, RenderTextureFormat.ARGB32);
                cubemaps[i].dimension = TextureDimension.Cube;
                cubemaps[i].wrapMode = TextureWrapMode.Clamp;
            }

            screenViewerTexture = new RenderTexture(512, 512, 32, RenderTextureFormat.ARGB32); // Use a low resolution to minimise performance affected by debugging.
            screenViewerTexture.dimension = TextureDimension.Tex2D;
            screenViewerTexture.wrapMode = TextureWrapMode.Clamp;

            // Initialise material.
            material = new Material(shader);
            material.SetTexture("_CubemapNorth", cubemaps[(int)CubemapIndex.North]);
            material.SetTexture("_CubemapSouth", cubemaps[(int)CubemapIndex.South]);
            material.SetTexture("_CubemapEast", cubemaps[(int)CubemapIndex.East]);
            material.SetTexture("_CubemapWest", cubemaps[(int)CubemapIndex.West]);
        }

        private void Start() {
        }

        private void Update() {
            RenderEyes();

#if UNITY_EDITOR
            // In editor mode, blit to the screen viewer.
            Graphics.Blit(null, screenViewerTexture, material);
#endif
        }

        private void LogDirection(string message) {
            if (!enableDirectionDebug) return;
            Debug.Log(message);
        }

        private void GetRenderDirections(Vector3 headPosition, out int eastWestMask, out int northSouthMask) {
            const int leftMask = 1 << (int)CubemapFace.PositiveX;
            const int rightMask = 1 << (int)CubemapFace.NegativeX;
            const int topMask = 1 << (int)CubemapFace.PositiveY;
            const int bottomMask = 1 << (int)CubemapFace.NegativeY;
            const int frontMask = 1 << (int)CubemapFace.PositiveZ;
            const int backMask = 1 << (int)CubemapFace.NegativeZ;

            eastWestMask = 0;
            northSouthMask = 0;
            Vector3 southWestBoundary = Vector3.zero;
            Vector3 northEastBoundary = Vector3.zero;
            Vector3 northWestBoundary = Vector3.zero;
            Vector3 southEastBoundary = Vector3.zero;

            // Get North-East and South-West boundaries where the sampled cubemap switches for stereoscopic rendering.
            List<float> xIntersectSouthWestToNorthEast = MathsUtil.SolveQuadraticEquation(
                1.0f,
                headPosition.x + headPosition.z,
                -0.5f * (cavernRadius * cavernRadius - headPosition.x * headPosition.x - headPosition.z * headPosition.z));
            // Get North-West and South-East boundaries where the sampled cubemap switches for stereoscopic rendering.
            List<float> xIntersectNorthWestToSouthEast = MathsUtil.SolveQuadraticEquation(
                1.0f,
                headPosition.x - headPosition.z,
                -0.5f * (cavernRadius * cavernRadius - headPosition.x * headPosition.x - headPosition.z * headPosition.z));
            if (xIntersectSouthWestToNorthEast.Count == 1) {
                northEastBoundary = new Vector3(xIntersectSouthWestToNorthEast[0], 0.0f, xIntersectSouthWestToNorthEast[0]);
                southWestBoundary = new Vector3(xIntersectSouthWestToNorthEast[0], 0.0f, xIntersectSouthWestToNorthEast[0]);
            } else if (xIntersectSouthWestToNorthEast.Count == 2) {
                northEastBoundary = new Vector3(xIntersectSouthWestToNorthEast[1], 0.0f, xIntersectSouthWestToNorthEast[1]);
                southWestBoundary = new Vector3(xIntersectSouthWestToNorthEast[0], 0.0f, xIntersectSouthWestToNorthEast[0]);
            }

            if (xIntersectNorthWestToSouthEast.Count == 1) {
                northWestBoundary = new Vector3(xIntersectNorthWestToSouthEast[0], 0.0f, -xIntersectNorthWestToSouthEast[0]);
                southEastBoundary = new Vector3(xIntersectNorthWestToSouthEast[0], 0.0f, -xIntersectNorthWestToSouthEast[0]);
            } else if (xIntersectNorthWestToSouthEast.Count == 2) {
                northWestBoundary = new Vector3(xIntersectNorthWestToSouthEast[0], 0.0f, -xIntersectNorthWestToSouthEast[0]);
                southEastBoundary = new Vector3(xIntersectNorthWestToSouthEast[1], 0.0f, -xIntersectNorthWestToSouthEast[1]);
            }

            // Edge Case 1: Head is moved out of the screen area and there are no intersects.
            // This means that the screen is entirely in one quadrant relative to the head.
            if (xIntersectSouthWestToNorthEast.Count == 0 && xIntersectNorthWestToSouthEast.Count == 0) {
                // Screen is south of the head.
                if (0.0f < head.localPosition.z &&
                    Mathf.Abs(head.localPosition.x) < Mathf.Abs(head.localPosition.z)) {
                    // For edge cases, fuck it just render the top and the bottom too.
                    // I can't be bothered to check those as well, since this should rarely ever happen.
                    eastWestMask |= (backMask | topMask | bottomMask);
                    LogDirection("[East-West] Back, Top, Bottom");
                    return;
                }

                // Screen is north of the head.
                if (head.localPosition.z < 0.0f &&
                    Mathf.Abs(head.localPosition.x) < Mathf.Abs(head.localPosition.z)) {
                    eastWestMask |= (frontMask | topMask | bottomMask);
                    LogDirection("[East-West] Front, Top, Bottom");
                    return;
                }

                // Screen is east of the head.
                if (head.localPosition.x < 0.0f &&
                    Mathf.Abs(head.localPosition.z) < Mathf.Abs(head.localPosition.x)) {
                    northSouthMask |= (rightMask | topMask | bottomMask);
                    LogDirection("[North-South] Right, Top, Bottom");
                    return;
                }

                // Screen is west of the head.
                if (head.localPosition.x > 0.0f &&
                    Mathf.Abs(head.localPosition.z) < Mathf.Abs(head.localPosition.x)) {
                    northSouthMask |= (leftMask | topMask | bottomMask);
                    LogDirection("[North-South] Left, Top, Bottom");
                    return;
                }
            }

            // Edge Case 2: Head is moved out of the screen and only the South-West to North-East line intersects.
            if (xIntersectSouthWestToNorthEast.Count > 0 && xIntersectNorthWestToSouthEast.Count == 0) {
                // Screen is south-west of the head.
                if (Vector3.Dot(new Vector3(1.0f, 1.0f), new Vector2(head.localPosition.x, head.localPosition.z)) > 1.0f) {
                    eastWestMask |= (backMask | topMask | bottomMask);
                    northSouthMask |= (leftMask | topMask | bottomMask);
                    LogDirection("[East-West] Back, Top, Bottom\n[North-South] Left, Top, Bottom");
                    return;
                }

                // Screen is north-east of the head.
                if (Vector3.Dot(new Vector3(-1.0f, -1.0f), new Vector2(head.localPosition.x, head.localPosition.z)) > 1.0f) {
                    eastWestMask |= (frontMask | topMask | bottomMask);
                    northSouthMask |= (rightMask | topMask | bottomMask);
                    LogDirection("[East-West] Front, Top, Bottom\n[North-South] Right, Top, Bottom");
                    return;
                }
            }

            // Edge Case 3: Head is moved out of the screen and only the North-West to South-East line intersects.
            if (xIntersectSouthWestToNorthEast.Count == 0 && xIntersectNorthWestToSouthEast.Count > 0) {
                // Screen is north-west of the head.
                if (Vector3.Dot(new Vector3(1.0f, -1.0f), new Vector2(head.localPosition.x, head.localPosition.z)) > 1.0f) {
                    eastWestMask |= (frontMask | topMask | bottomMask);
                    northSouthMask |= (leftMask | topMask | bottomMask);
                    LogDirection("[East-West] Front, Top, Bottom\n[North-South] Left, Top, Bottom");
                    return;
                }

                // Screen is south-east of the head.
                if (Vector3.Dot(new Vector3(-1.0f, 1.0f), new Vector2(head.localPosition.x, head.localPosition.z)) > 1.0f) {
                    eastWestMask |= (backMask | topMask | bottomMask);
                    northSouthMask |= (rightMask | topMask | bottomMask);
                    LogDirection("[East-West] Back, Top, Bottom\n[North-South] Right, Top, Bottom");
                    return;
                }
            }

            // Regular Case: The head is within the screen area.
            string eastWestDebugMsg = "[East-West] Front";
            string northSouthDebugMsg = "\n[North-South]";
            eastWestMask |= frontMask; // Always render the front.
            northSouthMask = 0;

            float screenTop = cavernElevation + cavernHeight - headPosition.y;
            float screenBottom = cavernElevation - headPosition.y;
            Vector3 headOffset = new Vector3(headPosition.x, 0.0f, headPosition.z);

            // Calculate the render mask for the East and West cubemaps.
            if (Vector3.Angle(headOffset + southWestBoundary, Vector3.forward) < cavernAngle * 0.5f || Vector3.Angle(headOffset + southEastBoundary, Vector3.forward) < cavernAngle * 0.5f) {
                eastWestMask |= backMask;
                eastWestDebugMsg += ", Back";
            }
            if (Mathf.Abs(northEastBoundary.z) < Mathf.Abs(screenTop) ||
                Mathf.Abs(northWestBoundary.z) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southEastBoundary.z) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southWestBoundary.z) < Mathf.Abs(screenTop)) {
                eastWestMask |= topMask;
                eastWestDebugMsg += ", Top";
            }
            if (Mathf.Abs(northEastBoundary.z) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(northWestBoundary.z) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southEastBoundary.z) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southWestBoundary.z) < Mathf.Abs(screenBottom)) {
                eastWestMask |= bottomMask;
                eastWestDebugMsg += ", Bottom";
            }

            // Calculate the render mask for the North and South cubemaps.
            if (Vector3.Angle(headOffset + northEastBoundary, Vector3.forward) < cavernAngle * 0.5f || Vector3.Angle(headOffset + southEastBoundary, Vector3.forward) < cavernAngle * 0.5f) {
                northSouthMask |= rightMask;
                northSouthDebugMsg += " Right";
            }
            if (Vector3.Angle(headOffset + northWestBoundary, Vector3.forward) < cavernAngle * 0.5f || Vector3.Angle(headOffset + southWestBoundary, Vector3.forward) < cavernAngle * 0.5f) {
                northSouthMask |= leftMask;
                northSouthDebugMsg += ", Left";
            }
            if (Mathf.Abs(northEastBoundary.x) < Mathf.Abs(screenTop) ||
                Mathf.Abs(northWestBoundary.x) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southEastBoundary.x) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southWestBoundary.x) < Mathf.Abs(screenTop)) {
                northSouthMask |= topMask;
                northSouthDebugMsg += ", Top";
            }
            if (Mathf.Abs(northEastBoundary.x) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(northWestBoundary.x) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southEastBoundary.x) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southWestBoundary.x) < Mathf.Abs(screenBottom)) {
                northSouthMask |= bottomMask;
                northSouthDebugMsg += ", Bottom";
            }

            LogDirection(eastWestDebugMsg + northSouthDebugMsg);
        }

        private void RenderEyes() {
            // If clampHeadPosition is true, limit the head position to be within the bounds of the circle.
            if (clampHeadPosition) {
                Vector2 horizontalPosition = new Vector2(head.transform.localPosition.x, head.transform.localPosition.z);
                if (horizontalPosition.sqrMagnitude > clampHeadRatio * clampHeadRatio * cavernRadius * cavernRadius) {
                    horizontalPosition = horizontalPosition.normalized * clampHeadRatio * cavernRadius;
                    head.transform.localPosition = new Vector3(horizontalPosition.x, head.transform.localPosition.y, horizontalPosition.y);
                }
            }

            // Use Camera.MonoOrStereoscopicEye.Left or Camera.MonoOrStereoscopicEye.Right to ensure that the cubemap follows the camera's rotation.
            // Camera.MonoOrStereoscopicEye.Mono renders the cubemap to be aligned to the world's axes instead.
            int eastWestMask = 0;
            int northSouthMask = 0;
            GetRenderDirections(head.transform.localPosition, out eastWestMask, out northSouthMask);

            switch (stereoMode) {
                case StereoscopicMode.Mono:
                    eye.stereoSeparation = 0.0f;
                    eye.transform.rotation = gameObject.transform.rotation; // Set eye's global orientation to the screen's orientation, regardless of the head's orientation.
                    eye.transform.localPosition = Vector3.zero;
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.North], eastWestMask | northSouthMask, Camera.MonoOrStereoscopicEye.Left);
                    break;
                case StereoscopicMode.Stereo:
                    eye.stereoSeparation = 0.0f;
                    eye.transform.rotation = gameObject.transform.rotation; // Set eye's global orientation to the screen's orientation, regardless of the head's orientation.
                    eye.transform.localPosition = new Vector3(interpupillaryDistance * -0.5f, 0.0f, 0.0f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.West], eastWestMask, Camera.MonoOrStereoscopicEye.Left);
                    eye.transform.localPosition = new Vector3(interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.East], eastWestMask, Camera.MonoOrStereoscopicEye.Right);
                    eye.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * 0.5f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.North], northSouthMask, Camera.MonoOrStereoscopicEye.Left);
                    eye.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * -0.5f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.South], northSouthMask, Camera.MonoOrStereoscopicEye.Right);
                    eye.transform.localPosition = Vector3.zero;
                    break;
            }

            material.SetInteger("_EnableStereo", stereoMode == StereoscopicMode.Stereo ? 1 : 0);
            material.SetFloat("_CavernHeight", cavernHeight);
            material.SetFloat("_CavernRadius", cavernRadius);
            material.SetFloat("_CavernAngle", cavernAngle);
            material.SetFloat("_CavernElevation", cavernElevation);
            material.SetVector("_HeadPosition", head.transform.localPosition);
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) { }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras) { }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) { }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == eye) {
                Graphics.Blit(null, material);
            }
        }
    }
}