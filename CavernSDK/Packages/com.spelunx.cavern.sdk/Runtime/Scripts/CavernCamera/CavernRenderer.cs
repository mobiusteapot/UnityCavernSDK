using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Spelunx {
    public class CavernRenderer : MonoBehaviour {
        public enum StereoscopicMode {
            Mono,
            Stereo,
        }

        public enum CubemapResolution {
            Low = 1024,
            Mid = 2048,
            High = 4096,
            VeryHigh = 8192,
        }

        private enum CubemapIndex {
            Left = 0, // Also used for monoscopic.
            Right,
            Front,
            Back,

            Num,
        }

        [Header("Camera Settings")]
        [SerializeField] private StereoscopicMode stereoMode = StereoscopicMode.Mono;
        [SerializeField] private CubemapResolution cubemapResolution = CubemapResolution.Mid;
        [SerializeField, Range(0.055f, 0.075f)] private float interpupillaryDistance = 0.065f; // IPD in metres.
        [SerializeField, Min(0.1f)] private float cavernHeight = 2.0f; // Cavern physical screen height in metres.
        [SerializeField, Min(0.1f)] private float cavernRadius = 3.0f; // Cavern physical screen radius in metres.
        [SerializeField, Min(0.1f)] private float cavernAngle = 270.0f; // Cavern physical screen angle in degrees.
        [SerializeField, Range(-0.5f, 0.5f)] private float cavernElevation = 0.0f; // Cavern physical screen elevation relative to where the player is standing.

        [Header("References")]
        [SerializeField] private Transform head;
        [SerializeField] private Camera eye;
        [SerializeField] private Shader shader;

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
        
        public float GetAspectRatio() {
            return ((cavernAngle / 360.0f) * 2.0f * cavernRadius * Mathf.PI) / cavernHeight;
        }

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
            material.SetTexture("_CubemapLeft", cubemaps[(int)CubemapIndex.Left]);
            material.SetTexture("_CubemapRight", cubemaps[(int)CubemapIndex.Right]);
            material.SetTexture("_CubemapFront", cubemaps[(int)CubemapIndex.Front]);
            material.SetTexture("_CubemapBack", cubemaps[(int)CubemapIndex.Back]);
        }

        private void Start() {
        }

        private void Update() {
            RenderEyes();

            // In editor mode, blit to the screen viewer.
#if UNITY_EDITOR
            Graphics.Blit(null, screenViewerTexture, material);
#endif
        }

        private void RenderEyes() {
            // TODO: Clean this code up. And document it so that it's not just some scribbles on paper.
            // Find the intersection points.
            // Solve quadratic equation for Line 1:
            float c = -(cavernRadius * cavernRadius - head.transform.localPosition.x * head.transform.localPosition.x - head.transform.localPosition.z * head.transform.localPosition.z) * 0.5f;
            float b1 = head.transform.localPosition.x + head.transform.localPosition.z;
            float t1 = (-b1 + Mathf.Sqrt(b1 * b1 - 4 * c)) / 2.0f;
            float t2 = (-b1 - Mathf.Sqrt(b1 * b1 - 4 * c)) / 2.0f;

            // TODO: Deal with cases when b^2 - 4ac < 0.

            // Solve quadratic equation for Line 2:
            float b2 = head.transform.localPosition.x - head.transform.localPosition.z;
            float s1 = (-b2 + Mathf.Sqrt(b2 * b2 - 4 * c)) / 2.0f;
            float s2 = (-b2 - Mathf.Sqrt(b2 * b2 - 4 * c)) / 2.0f;

            Vector3 A = new Vector3(Mathf.Max(t1, t2), 0.0f, Mathf.Max(t1, t2));
            Vector3 C = new Vector3(Mathf.Min(t1, t2), 0.0f, Mathf.Min(t1, t2));
            Vector3 B = new Vector3(Mathf.Min(s1, s2), 0.0f, -Mathf.Min(s1, s2));
            Vector3 D = new Vector3(Mathf.Max(s1, s2), 0.0f, -Mathf.Max(s1, s2));

            A += new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
            B += new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
            C += new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
            D += new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);

            const int leftMask = 1 << (int)CubemapFace.PositiveX;
            const int rightMask = 1 << (int)CubemapFace.NegativeX;
            const int topMask = 1 << (int)CubemapFace.PositiveY;
            const int bottomMask = 1 << (int)CubemapFace.NegativeY;
            const int frontMask = 1 << (int)CubemapFace.PositiveZ;
            const int backMask = 1 << (int)CubemapFace.NegativeZ;

            string debugStr = "Rendering: Front";

            int leftRightEyeMask = frontMask;
            if (Vector3.Angle(C, Vector3.forward) < cavernAngle * 0.5f || Vector3.Angle(D, Vector3.forward) < cavernAngle * 0.5f) {
                leftRightEyeMask |= backMask;
                debugStr += ", Back";
            }

            int frontBackEyeMask = 0;
            if (Vector3.Angle(A, Vector3.forward) < cavernAngle * 0.5f || Vector3.Angle(D, Vector3.forward) < cavernAngle * 0.5f) {
                frontBackEyeMask |= rightMask;
                debugStr += ", Right";
            }
            if (Vector3.Angle(B, Vector3.forward) < cavernAngle * 0.5f || Vector3.Angle(C, Vector3.forward) < cavernAngle * 0.5f) {
                frontBackEyeMask |= leftMask;
                debugStr += ", Left";
            }

            A -= new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
            B -= new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
            C -= new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);
            D -= new Vector3(head.transform.localPosition.x, 0.0f, head.transform.localPosition.z);

            Vector3 forwardQuadTopCheck = (A.z < B.z) ? A : B;
            forwardQuadTopCheck.y = cavernElevation + cavernHeight - head.transform.localPosition.y;
            Vector3 backQuadTopCheck = (C.z > D.z) ? C : D;
            backQuadTopCheck.y = cavernElevation + cavernHeight - head.transform.localPosition.y;
            
            if (Mathf.Abs(forwardQuadTopCheck.z) < Mathf.Abs(forwardQuadTopCheck.y) ||
                Mathf.Abs(backQuadTopCheck.z) < Mathf.Abs(backQuadTopCheck.y)) {
                leftRightEyeMask |= topMask;
                debugStr += ", Horizontal Top";
            }

            Vector3 forwardQuadBottomCheck = (A.z < B.z) ? A : B;
            forwardQuadBottomCheck.y = cavernElevation - head.transform.localPosition.y;
            Vector3 backQuadBottomCheck = (C.z > D.z) ? C : D;
            backQuadBottomCheck.y = cavernElevation - head.transform.localPosition.y;

            if (Mathf.Abs(forwardQuadBottomCheck.z) < Mathf.Abs(forwardQuadBottomCheck.y) ||
                Mathf.Abs(backQuadBottomCheck.z) < Mathf.Abs(backQuadBottomCheck.y)) {
                leftRightEyeMask |= bottomMask;
                debugStr += ", Horizontal Bottom";
            }

            Vector3 leftQuadTopCheck = (B.x > C.x) ? B : C;
            leftQuadTopCheck.y = cavernElevation + cavernHeight - head.transform.localPosition.y;
            Vector3 rightQuadTopCheck = (A.x < D.x) ? A : D;
            rightQuadTopCheck.y = cavernElevation + cavernHeight - head.transform.localPosition.y;
            
            if (Mathf.Abs(leftQuadTopCheck.x) < Mathf.Abs(leftQuadTopCheck.y) ||
                Mathf.Abs(rightQuadTopCheck.x) < Mathf.Abs(rightQuadTopCheck.y)) {
                frontBackEyeMask |= topMask;
                debugStr += ", Vertical Top";
            }

            Vector3 leftQuadBottomCheck = (B.x > C.x) ? B : C;
            leftQuadTopCheck.y = cavernElevation - head.transform.localPosition.y;
            Vector3 rightQuadBottomCheck = (A.x < D.x) ? A : D;
            rightQuadTopCheck.y = cavernElevation - head.transform.localPosition.y;

            if (Mathf.Abs(leftQuadBottomCheck.x) < Mathf.Abs(leftQuadBottomCheck.y) ||
                Mathf.Abs(rightQuadTopCheck.x) < Mathf.Abs(rightQuadTopCheck.y)) {
                frontBackEyeMask |= bottomMask;
                debugStr += ", Vertical Bottom";
            }

            // Debug.Log(debugStr);

            // Use Camera.MonoOrStereoscopicEye.Left or Camera.MonoOrStereoscopicEye.Right to ensure that the cubemap follows the camera's rotation.
            // Camera.MonoOrStereoscopicEye.Mono renders the cubemap to be aligned to the world's axes instead.
            switch (stereoMode) {
                case StereoscopicMode.Mono:
                    eye.stereoSeparation = 0.0f;
                    eye.transform.rotation = gameObject.transform.rotation; // Set eye's global orientation to the screen's orientation, regardless of the head's orientation.
                    eye.transform.localPosition = Vector3.zero;
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.Left], leftRightEyeMask | frontBackEyeMask, Camera.MonoOrStereoscopicEye.Left);
                    break;
                case StereoscopicMode.Stereo:
                    eye.stereoSeparation = 0.0f;
                    eye.transform.rotation = gameObject.transform.rotation; // Set eye's global orientation to the screen's orientation, regardless of the head's orientation.
                    eye.transform.localPosition = new Vector3(interpupillaryDistance * -0.5f, 0.0f, 0.0f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.Left], leftRightEyeMask, Camera.MonoOrStereoscopicEye.Left);
                    eye.transform.localPosition = new Vector3(interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.Right], leftRightEyeMask, Camera.MonoOrStereoscopicEye.Right);
                    eye.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * 0.5f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.Front], frontBackEyeMask, Camera.MonoOrStereoscopicEye.Left);
                    eye.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * -0.5f);
                    eye.RenderToCubemap(cubemaps[(int)CubemapIndex.Back], frontBackEyeMask, Camera.MonoOrStereoscopicEye.Right);
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