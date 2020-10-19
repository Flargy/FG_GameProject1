using System;
using UnityEngine;

[Serializable]
public class BalloonString : IDisposable
{
    private readonly GameObject clownHand;
    public readonly GameObject unicorn;
    private readonly Transform connectionPoint;
    private LineRenderer lineRenderer;

    private readonly float maxDistance;
    private readonly int detail;
    private readonly float maximumSag;
    
    static GameObject emptyObject = new GameObject();
    
    private Vector3[] positions;
    public BalloonString(GameObject clownHand, CapturedEnemy capturedEnemy, float maxDistance, float maximumSag,
        float lineWidth, Material stringMaterial, Color stringColor, int detail)
    {
        this.clownHand = GameObject.Instantiate(emptyObject, clownHand.transform);
        this.unicorn = capturedEnemy.gameObject;
        this.connectionPoint = capturedEnemy.stringConnectionPoint;
        this.maximumSag = maximumSag;
        this.maxDistance = maxDistance;
        this.detail = detail;

        positions = new Vector3[detail];
        InitializeLineRenderer(lineWidth, stringMaterial, stringColor);
     }

    private void InitializeLineRenderer(float lineWidth, Material stringMaterial, Color color)
    {
        lineRenderer = clownHand.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.material = stringMaterial;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    public void Dispose()
    {
        GameObject.Destroy(lineRenderer);
        GameObject.Destroy(clownHand);
    }

    public void Update()
    {
        Vector3 clownPoint = clownHand.transform.position;
        Vector3 unicornPoint = connectionPoint.position;

        CalculateTangents(clownPoint, unicornPoint, out Vector3 tangentA, out Vector3 tangentB);

        for (int i = 0; i < detail - 1; i++)
        {
            // Traversing this backwards so that the textures pop less in the place where we can see them.
            float t = (float) (detail -i) / detail;

            positions[i] = SampleCubicBezierPoint(t, clownPoint, unicornPoint, tangentA, tangentB);
        }
        positions[detail - 1] = SampleCubicBezierPoint(0.0f, clownPoint, unicornPoint, tangentA, tangentB);
        
        lineRenderer.positionCount = detail;
        lineRenderer.SetPositions(positions);
    }

    public GameObject TrackedUnicorn()
    {
        return unicorn.gameObject;
    }
    
    void CalculateTangents(Vector3 clownPoint, Vector3 unicornPoint, out Vector3 tangentA, out Vector3 tangentB)
    {
        float distance = Vector3.Distance(clownPoint, unicornPoint);
        if (distance > maxDistance)
        {
            // This should technically never happen, but I'm handling it just in case.
            // We are further away than we should be and the unicorn should have popped to us.
            tangentA = clownPoint;
            tangentB = unicornPoint;
        }
        else
        {
            // @NOTE: Reader Beware: Even I don't know what this is doing geometrically, and I wrote the damn thing.
            float remainingDistance = maxDistance - distance;
            float triangleT = remainingDistance / maxDistance;

            Vector3 pointC = (clownPoint + unicornPoint) / 2.0f + (Vector3.down * remainingDistance);
            
            float minPoint = Mathfs.Min(unicornPoint.y, clownPoint.y);
            float saggiest = minPoint - maximumSag;
            
            tangentA = Vector3.Lerp(clownPoint, pointC, triangleT + (triangleT * remainingDistance));
            tangentB = unicornPoint + (unicornPoint - pointC) * -(Mathfs.Abs((saggiest - tangentA.y).AtMost(0.0f)));
            
            tangentA.y = Mathfs.Max(tangentA.y, saggiest);
            tangentB.y = Mathfs.Max(tangentB.y, saggiest);
        }
    }

    private static Vector3 SampleCubicBezierPoint(float t, Vector3 clownPoint, Vector3 unicornPoint, Vector3 tangentA, Vector3 tangentB)
    {
        float u = 1.0f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 result = uuu * clownPoint;
        result += 3.0f * uu * t * tangentA;
        result += 3.0f * u * tt * tangentB;
        result += ttt * unicornPoint;
        return result;
    }
}
