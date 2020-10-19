
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProximityDetector))]
public class ProximityDetectorEditor : Editor
{
    private ProximityDetector prox;

    private void OnEnable()
    {
        if (!target) return;
        prox = (ProximityDetector) target;
    }
    
    private void OnSceneGUI()
    {
        using (new Handles.DrawingScope())
        {
            Handles.color = prox.IsInRange ? Color.green : Color.red;
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(prox, "Changed Proximity Detector radius.");
            prox.DetectionRadius = Handles.RadiusHandle(Quaternion.identity, prox.transform.position, prox.DetectionRadius);
        }
    }
}
