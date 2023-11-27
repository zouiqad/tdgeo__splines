using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawHermiteCurve : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Vector3 startTangent;
    public Vector3 endTangent;
    public int numberOfPoints = 100;


    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPoint.position, startPoint.position + startTangent);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(endPoint.position, endPoint.position + endTangent);

        DrawCurve();
    }


    public List<Vector3> DrawCurve()
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 1; i < numberOfPoints - 1; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            Vector3 pointPosition = CalculateHermitePoint(t, startPoint.position, endPoint.position, startTangent, endTangent);
            
/*            GameObject pointGO = new GameObject($"P{i}");
            pointGO.AddComponent<PointController>();
            pointGO.transform.parent = transform;
            pointGO.transform.position = pointPosition;
            pointGO.gameObject.SetActive(false);*/
            points.Add(pointPosition);
        }
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.blue, (float)i / points.Count);
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        return points;
    }

    Vector3 CalculateHermitePoint(float t, Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float h0 = 2 * t3 - 3 * t2 + 1;
        float h1 = -2 * t3 + 3 * t2;
        float h2 = t3 - 2 * t2 + t;
        float h3 = t3 - t2;

        return h0 * p0 + h1 * p1 + h2 * m0 + h3 * m1;
    }
}
