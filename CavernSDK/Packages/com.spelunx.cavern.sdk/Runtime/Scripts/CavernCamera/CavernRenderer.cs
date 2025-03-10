using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;

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
        [SerializeField, Range(1.0f, 360.0f)] private float cavernAngle = 270.0f;
        /// Cavern physical screen elevation in metres, relative to the player's feet.
        [SerializeField, Range(-0.5f, 0.5f)] private float cavernElevation = 0.0f;
        /// Increase accuracy at the cost of significant performance.
        [SerializeField] private bool enableConvergence = false;

        // All these are mostly just exposed for testing atm
        // Temporary, for evaluating RG
        public bool UseRenderGraph = false;
        // Maybe you don't want to expose this?
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRendering;
        private CavernRenderPass cavernRenderPass;
        // Need to create a dummy camera due to how URP handles queuing passes
        private Camera dummyCamera;


        [Header("Head Tracking")]
        /// If set to true, the ear will follow the head.
        [SerializeField] private bool tetherEar = true;
        /// If set to true, the head position will be clamped to within the the radius of the screen.
        [SerializeField] private bool clampHeadPosition = true;
        /// <summary>
        /// Sets the clamping radius of the head, if clampHeadPosition = true. 
        /// For example, if clampHeadRatio = 0.8 and cavernRadius = 3, the head will be clamped to a radius of 2.4.
        /// </summary>
        [SerializeField, Range(0.0f, 1.0f)] private float clampHeadRatio = 0.8f;

        [Header("References (Do NOT edit!)")]
        [SerializeField] private Transform head;
        [SerializeField] private Camera eye;
        [SerializeField] private AudioListener ear;
        [SerializeField] private Shader shader;

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
        public GameObject GetEye() { return eye.gameObject; }
        public GameObject GetEar() { return ear.gameObject; }

        private void OnEnable() {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

            cavernRenderPass = new CavernRenderPass();
            // Todo: Dynamically update setup, if settings changed
            cavernRenderPass.Setup(material, cubemaps);
            cavernRenderPass.renderPassEvent = renderPassEvent;
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

            if(UseRenderGraph)
            {
                // Create a dummy camera, parented to the "eye" camera
                // Prevents wasted draw calls for base scene
                // Todo: figure out best paradigm for handling this at runtime and edit time
                dummyCamera = new GameObject("Dummy Camera").AddComponent<Camera>();
                dummyCamera.transform.SetParent(eye.transform);
                dummyCamera.CopyFrom(eye);
                dummyCamera.cullingMask = 0;
                eye.enabled = false;
            }
        }

        private void Start() {
        }

        private void Update() {
            if (tetherEar) {
                ear.gameObject.transform.position = head.transform.position;
                ear.gameObject.transform.rotation = head.transform.rotation;
            }

            RenderEyes();
            // Todo: pass anything to render pass that may have changed
#if UNITY_EDITOR
            // In editor mode, blit to the screen viewer.
            Graphics.Blit(null, screenViewerTexture, material);
#endif
        }

        private void GetRenderFaces(out int monoMask, out int northMask, out int southMask, out int eastMask, out int westMask) {
            const int rightMask = 1 << (int)CubemapFace.PositiveX;
            const int leftMask = 1 << (int)CubemapFace.NegativeX;
            const int topMask = 1 << (int)CubemapFace.PositiveY;
            const int bottomMask = 1 << (int)CubemapFace.NegativeY;
            const int frontMask = 1 << (int)CubemapFace.PositiveZ;
            const int backMask = 1 << (int)CubemapFace.NegativeZ;

            monoMask = 0; northMask = 0; southMask = 0; eastMask = 0; westMask = 0;

            Vector3 headPosition = head.transform.localPosition;
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

            // For edge cases, assume that the top and bottom faces are not visible.
            // It should be correct for most cases if the Cavern has sane dimensions.
            // Edge Case 1: Head is moved out of the screen area and there are no intersects.
            // This means that the screen is entirely in one quadrant relative to the head.
            if (xIntersectSouthWestToNorthEast.Count == 0 && xIntersectNorthWestToSouthEast.Count == 0) {
                // Screen is entirely south of the head.
                if (0.0f < headPosition.z &&
                    Mathf.Abs(headPosition.x) < Mathf.Abs(headPosition.z)) {
                    monoMask |= backMask;
                    eastMask |= backMask;
                    westMask |= backMask;
                    return;
                }

                // Screen is entirely north of the head.
                if (headPosition.z < 0.0f &&
                    Mathf.Abs(headPosition.x) < Mathf.Abs(headPosition.z)) {
                    monoMask |= frontMask;
                    eastMask |= frontMask;
                    westMask |= frontMask;
                    return;
                }

                // Screen is entirely east of the head.
                if (headPosition.x < 0.0f &&
                    Mathf.Abs(headPosition.z) < Mathf.Abs(headPosition.x)) {
                    monoMask |= rightMask;
                    northMask |= rightMask;
                    southMask |= rightMask;
                    return;
                }

                // Screen is entirely west of the head.
                if (headPosition.x > 0.0f &&
                    Mathf.Abs(headPosition.z) < Mathf.Abs(headPosition.x)) {
                    monoMask |= leftMask;
                    northMask |= leftMask;
                    southMask |= leftMask;
                    return;
                }
            }

            // Edge Case 2: Head is moved out of the screen and only the South-West to North-East line intersects.
            if (xIntersectSouthWestToNorthEast.Count > 0 && xIntersectNorthWestToSouthEast.Count == 0) {
                // Screen is entirely south-west of the head.
                if (Vector3.Dot(new Vector3(1.0f, 1.0f), new Vector2(headPosition.x, headPosition.z)) > 1.0f) {
                    monoMask |= (backMask | leftMask);
                    eastMask |= backMask;
                    westMask |= backMask;
                    northMask |= leftMask;
                    southMask |= leftMask;
                    return;
                }

                // Screen is entirely north-east of the head.
                if (Vector3.Dot(new Vector3(-1.0f, -1.0f), new Vector2(headPosition.x, headPosition.z)) > 1.0f) {
                    monoMask = (frontMask | rightMask);
                    eastMask |= frontMask;
                    westMask |= frontMask;
                    northMask |= rightMask;
                    southMask |= rightMask;
                    return;
                }
            }

            // Edge Case 3: Head is moved out of the screen and only the North-West to South-East line intersects.
            if (xIntersectSouthWestToNorthEast.Count == 0 && xIntersectNorthWestToSouthEast.Count > 0) {
                // Screen is entirely north-west of the head.
                if (Vector3.Dot(new Vector3(1.0f, -1.0f), new Vector2(headPosition.x, headPosition.z)) > 1.0f) {
                    monoMask = (frontMask | leftMask);
                    eastMask |= frontMask;
                    westMask |= frontMask;
                    northMask |= leftMask;
                    southMask |= leftMask;
                    return;
                }

                // Screen is entirely south-east of the head.
                if (Vector3.Dot(new Vector3(-1.0f, 1.0f), new Vector2(headPosition.x, headPosition.z)) > 1.0f) {
                    monoMask = (backMask | rightMask);
                    eastMask |= backMask;
                    westMask |= backMask;
                    northMask |= rightMask;
                    southMask |= rightMask;
                    return;
                }
            }

            // Regular Case: The head is within the screen area.
            float screenTop = cavernElevation + cavernHeight - headPosition.y;
            float screenBottom = cavernElevation - headPosition.y;
            Vector3 headOffset = new Vector3(headPosition.x, 0.0f, headPosition.z);

            /******************* Looking North *******************/
            monoMask |= frontMask;
            westMask |= frontMask | (enableConvergence ? rightMask : 0); // Left Eye
            eastMask |= frontMask | (enableConvergence ? leftMask : 0); // Right Eye

            /******************* Looking South *******************/
            if (Vector3.Angle(headOffset + southWestBoundary, Vector3.forward) < cavernAngle * 0.5f ||
                Vector3.Angle(headOffset + southEastBoundary, Vector3.forward) < cavernAngle * 0.5f) {
                monoMask |= backMask;
                eastMask |= backMask; // Left Eye
                westMask |= backMask; // Right Eye
            }

            /******************* Looking East *******************/
            if (Vector3.Angle(headOffset + northEastBoundary, Vector3.forward) < cavernAngle * 0.5f ||
                Vector3.Angle(headOffset + southEastBoundary, Vector3.forward) < cavernAngle * 0.5f) {
                monoMask |= rightMask;
                northMask |= rightMask | (enableConvergence ? backMask : 0); // Left Eye
                southMask |= rightMask | (enableConvergence ? frontMask : 0); // Right Eye
            }

            /******************* Looking West *******************/
            if (Vector3.Angle(headOffset + northWestBoundary, Vector3.forward) < cavernAngle * 0.5f ||
                Vector3.Angle(headOffset + southWestBoundary, Vector3.forward) < cavernAngle * 0.5f) {
                monoMask |= leftMask;
                southMask |= leftMask | (enableConvergence ? frontMask : 0); // Left Eye
                northMask |= leftMask | (enableConvergence ? backMask : 0); // Right Eye
            }

            /******************* Top & Bottom Faces *******************/
            if (Mathf.Abs(northEastBoundary.z) < Mathf.Abs(screenTop) ||
                Mathf.Abs(northWestBoundary.z) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southEastBoundary.z) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southWestBoundary.z) < Mathf.Abs(screenTop)) {
                monoMask |= topMask;
                eastMask |= topMask;
                westMask |= topMask;
            }
            if (Mathf.Abs(northEastBoundary.z) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(northWestBoundary.z) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southEastBoundary.z) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southWestBoundary.z) < Mathf.Abs(screenBottom)) {
                monoMask |= bottomMask;
                eastMask |= bottomMask;
                westMask |= bottomMask;
            }
            if (Mathf.Abs(northEastBoundary.x) < Mathf.Abs(screenTop) ||
                Mathf.Abs(northWestBoundary.x) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southEastBoundary.x) < Mathf.Abs(screenTop) ||
                Mathf.Abs(southWestBoundary.x) < Mathf.Abs(screenTop)) {
                monoMask |= topMask;
                northMask |= topMask;
                southMask |= topMask;
            }
            if (Mathf.Abs(northEastBoundary.x) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(northWestBoundary.x) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southEastBoundary.x) < Mathf.Abs(screenBottom) ||
                Mathf.Abs(southWestBoundary.x) < Mathf.Abs(screenBottom)) {
                monoMask |= topMask;
                northMask |= bottomMask;
                southMask |= bottomMask;
            }
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
            int monoMask = 0; int northMask = 0; int southMask = 0; int eastMask = 0; int westMask = 0;
            GetRenderFaces(out monoMask, out northMask, out southMask, out eastMask, out westMask);
            switch (stereoMode) {
                case StereoscopicMode.Mono:
                    eye.stereoSeparation = 0.0f;
                    eye.transform.rotation = gameObject.transform.rotation; // Set eye's global orientation to the screen's orientation, regardless of the head's orientation.
                    eye.transform.localPosition = Vector3.zero;
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.North], monoMask, Camera.MonoOrStereoscopicEye.Left);
                    break;
                case StereoscopicMode.Stereo:
                    eye.stereoSeparation = 0.0f;
                    eye.transform.rotation = gameObject.transform.rotation; // Set eye's global orientation to the screen's orientation, regardless of the head's orientation.
                    eye.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * 0.5f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.North], northMask, Camera.MonoOrStereoscopicEye.Left);
                    eye.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * -0.5f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.South], southMask, Camera.MonoOrStereoscopicEye.Right);
                    eye.transform.localPosition = new Vector3(interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.East], eastMask, Camera.MonoOrStereoscopicEye.Right);
                    eye.transform.localPosition = new Vector3(interpupillaryDistance * -0.5f, 0.0f, 0.0f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.West], westMask, Camera.MonoOrStereoscopicEye.Left);
                    eye.transform.localPosition = Vector3.zero;
                    break;
            }

            // Cavern Dimensions Uniforms
            material.SetFloat("_CavernHeight", cavernHeight);
            material.SetFloat("_CavernRadius", cavernRadius);
            material.SetFloat("_CavernAngle", cavernAngle);
            material.SetFloat("_CavernElevation", cavernElevation);

            // Head Tracking Uniforms
            material.SetVector("_HeadPosition", head.transform.localPosition);

            // Stereoscopic Rendering Uniforms
            material.SetInteger("_EnableStereoscopic", stereoMode == StereoscopicMode.Stereo ? 1 : 0);
            material.SetInteger("_EnableConvergence", enableConvergence ? 1 : 0);
            material.SetFloat("_InterpupillaryDistance", interpupillaryDistance);
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) { }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras) { }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) {
            if(UseRenderGraph)
            {
                if(camera.cameraType != CameraType.Game)
                {
                    return;
                }
                if(camera == dummyCamera)
                {
                    
                    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(cavernRenderPass);
                }
            }

        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if(!UseRenderGraph)
            {
                if(camera == eye)
                {
                    Graphics.Blit(null, material);
                }
            }
        }
    }
}