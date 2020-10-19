using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloatingPlatform))]
public class FloatingPlatformEditor : Editor
{
    private FloatingPlatform platform;
    private BoxCollider collider;
    
    private void OnEnable()
    {
        if (!target) return;
        platform = (FloatingPlatform) target;
        collider = platform.GetComponent<BoxCollider>();

        platform.MakeWaypointsArraySafe();
        platform.waypoints[0].position = platform.transform.position;
    }
    
    private void OnSceneGUI()
    {
        platform.MakeWaypointsArraySafe();
        if(!Application.isPlaying)
            platform.waypoints[0].position = platform.transform.position;
        
        DrawResizeHandle();
        
        int numIterations = platform.waypoints.Length -1;
        if (platform.movementStyle == FloatingPlatform.LoopType.LoopBackToTheStart)
            numIterations++;

        using(new Handles.DrawingScope())
        {
            for (int i = 0; i < numIterations; i++)
            {
                Handles.color = new Color(0.8f, 0.0f, 0.0f, 0.6f);
                DrawWaypointLines(i);
            }

            for (int i = 0; i < platform.waypoints.Length; i++)
            {
                DrawWaypoints(i);
            }
        }
    }

    private void DrawWaypointLines(int index)
    {
        Vector3 currentPosition = platform.waypoints[index].position;
        int next = (index + 1) % platform.waypoints.Length;
        
        Handles.DrawLine(currentPosition, platform.waypoints[next].position);
    }

    private void DrawWaypoints(int index)
    {
        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 12;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        var startColor = new Color(0.1f, 1.0f, 0.2f, 0.7f);
        var endColor = new Color(0.1f, 0.2f, 1.0f, 0.7f);
        
        Vector3 currentPosition = platform.waypoints[index].position;

        Handles.color = Color.Lerp(startColor, endColor, (float) index / platform.waypoints.Length);
        Handles.DrawWireCube(currentPosition + Vector3.down * (platform.platformHeight * 0.5f), collider.size);

        string waypointText = index == 0 ? "Starting Position" : $"Waypoint {index}";
        Handles.Label(currentPosition,$"{waypointText}\nWait Time: {platform.waypoints[0].waitTime} sec", style);

        // Early-out waypoint zero.
        if (index == 0 && !Application.isPlaying) return;
        
        EditorGUI.BeginChangeCheck();
        var newPoint = Handles.DoPositionHandle(currentPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(platform, "Moved EnemyPath Point");
            EditorUtility.SetDirty(platform);
            platform.waypoints[index].position = newPoint;// + platform.transform.position;
        }
    }

    private void DrawResizeHandle()
    {
        var transform = platform.transform;
        var position = transform.position;

        // @NOTE THE SCALING SHENNANIGANS ON THIS MATRIX HERE.
        Matrix4x4 matrix = Matrix4x4.TRS(position, transform.rotation, transform.localScale * 0.5f);
        // Unity's implementation of Slider2D is janky as all hell. If you don't scale the matrix, you end up either
        // passing size*0.5f into Slider2d(handlePos) resulting in weird popping behaviour when you first press the
        // mouse, or you pass in Size and have the handle be placed miles away from the corner.
        using (new Handles.DrawingScope(Color.red, matrix))
        {
            EditorGUI.BeginChangeCheck();
            
            var style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;
            style.alignment = TextAnchor.LowerCenter;
            style.normal.textColor = Color.white;
            
            var flatColliderSize = new Vector3(collider.size.x, 0.0f, collider.size.z);
            
            var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.35f;
            
            Handles.Label(collider.size + (Vector3.up *  handleSize), "Resize..", style);
            
            Vector3 DraggedHandle = Handles.Slider2D(flatColliderSize,
                transform.up, transform.right, transform.forward,
                handleSize, Handles.CubeHandleCap, 0.1f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(platform, "Resized moving platform");
                var newSize = new Vector3(
                    Mathfs.Clamp(DraggedHandle.x, 0, Single.PositiveInfinity),
                    platform.platformHeight,
                    Mathfs.Clamp(DraggedHandle.z, 0, Single.PositiveInfinity));

                platform.platformVisual.transform.localScale = newSize;
                platform.platformSize = new Vector2(newSize.x, newSize.z);
                collider.size = newSize;
            }
        }
    }
}
