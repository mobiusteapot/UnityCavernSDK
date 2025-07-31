using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spelunx.Vive
{
    /// <summary>
    /// This class loads the vive tracker mesh, to be used by ViveTracker when rendering a gizmo.
    /// </summary>
    public static class ViveDebugRenderer
    {

#if UNITY_EDITOR
        public static readonly Mesh trackerMesh = (Mesh)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Models/vr_tracker_vive_3_0.obj", typeof(Mesh));
        public static readonly Mesh indexControllerLeftMesh = (Mesh)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Models/index_controller_left.obj", typeof(Mesh));
        public static readonly Mesh indexControllerRightMesh = (Mesh)AssetDatabase.LoadAssetAtPath("Packages/com.spelunx.cavern.vive-trackers/Models/index_controller_right.obj", typeof(Mesh));

#endif
    }
}
