using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BezierCurve : MonoBehaviour
{
    public int numberOfPoints = 100;

    private PointController currentControlPoint;
    private List<Curve> curves = new List<Curve>();

    private Color[] colors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    private Gradient gradient;

    private PointController jointPoint;

    public Vector3[] controlPointsPositions = new Vector3[]
    {
        new Vector3(-2.0f, -2.0f, 0.0f),
        new Vector3(-1.0f, 1.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(2.0f, -2.0f, 0.0f)
    };

    private void Start()
    {
        CreateControlPoints(controlPointsPositions);
        currentControlPoint = curves[0].P0;
    }

    private void Update()
    {

        var input = Input.inputString;

        // Manage selection
        switch (input.ToUpper())
        {
            case "0":
                currentControlPoint = curves[0].P0;
                break;
            case "1":
                currentControlPoint = curves[0].P1;
                break;
            case "2":
                currentControlPoint = curves[0].P2;
                break;
            case "3":
                currentControlPoint = curves[0].P3;
                break;
            default:
                break;
        }

        // Manage translate
        var input_x = Input.GetAxis("Horizontal");
        var input_y = Input.GetAxis("Vertical");

        float sensibility = 5.0f * Time.deltaTime;
        currentControlPoint.transform.position += input_x * sensibility * Vector3.right + input_y * sensibility * Vector3.up;
    }

    public void AlignTangents()
    {
        float alpha = 1.0f;

        for (int i = 0; i < curves.Count - 1; i++)
        {
            // Calculate current tangent vectors
            curves[i + 1].P1.transform.position = curves[i].P3.transform.position + (curves[i].P3.transform.position - curves[i].P2.transform.position) * alpha;  // P4 = P3 + (P3 - P2)alpha
            Vector3 tangentP = 3 * (curves[i].P3.transform.position - curves[i].P2.transform.position);
            Vector3 tangentQ = 3 * (curves[i].P1.transform.position - curves[i].P0.transform.position);

            // Calculate adjustment vector
            Vector3 adjustment = tangentP - tangentQ;
        }
    }
    private void OnDrawGizmos()
    {
        if (curves.Count > 0)
        {
            for (int i = 0; i < curves.Count; i++)
            {
                DrawPolygone(i);
                DrawBezier(i);
                DrawCircleSelected();
            }
        }

    }

    private void DrawCircleSelected()
    {
        Handles.color = currentControlPoint.color;
        Handles.DrawWireDisc(currentControlPoint.transform.position, -1 * Vector3.forward, 0.5f);
    }



    // Define a delegate for the computation method
    public delegate Vector3 ComputeMethod(List<PointController> controlPoints, float t);

    // Use the delegate to define the computation method
    private ComputeMethod computeMethod;

    public enum DelegateType
    {
        ComputeBerstein,
        ComputeMatrix,
        ComputeDeCasteljau
    }

    [SerializeField] private DelegateType _methodtype;

    private Vector3 ComputeMatrix(List<PointController> controlPoints, float t)
    {
        float[,] powerSeries_data =
        {
            { 1, t, t * t, t * t * t }
        };


        float[,] characteristic_data =
        {
            { 1, 0, 0, 0 },
            { -3, 3, 0, 0 },
            { 3, -6, 3, 0 },
            {-1, 3, -3, 1 }
        };

        Vector3[,] controlPoints_data =
        {
            { controlPoints[0].transform.position },
            { controlPoints[1].transform.position },
            { controlPoints[2].transform.position },
            { controlPoints[3].transform.position }
        };


        Matrix<float> powerSeriesMatrix = new Matrix<float>(powerSeries_data);
        powerSeriesMatrix.name = "power series";
        Matrix<float> characteristicMatrix = new Matrix<float>(characteristic_data);
        characteristicMatrix.name = "characteristic";
        Matrix<Vector3> controlPointsMatrix = new Matrix<Vector3>(controlPoints_data);
        controlPointsMatrix.name = "control points";

        Matrix<float> berstein_poly = Matrix<float>.Multiplication(powerSeriesMatrix, characteristicMatrix); // Berstein polynomials
        berstein_poly.name = "berstein poly";
        return Matrix<Vector3>.Multiplication(berstein_poly, controlPointsMatrix).elements[0, 0];
    }


    private Vector3 ComputeBerstein(List<PointController> controlPoints, float t)
    {
        return controlPoints[0].transform.position * (-1 * (t * t * t) + 3 * t * t - 3 * t + 1) // P0
            + controlPoints[1].transform.position * (3 * (t * t * t) - 6 * t * t + 3 * t) // P1
            + controlPoints[2].transform.position * (-3 * (t * t * t) + 3 * t * t) // P2
            + controlPoints[3].transform.position * t * t * t; // P3
    }

    private Vector3 ComputeDeCasteljau(List<PointController> controlPoints, float t)
    {
        List<Vector3> lerpPoints = new List<Vector3>();

        // Populate lerp points
        for (int i = 0; i < controlPoints.Count; i++)
        {
            lerpPoints.Add(controlPoints[i].transform.position);
        }

        while (lerpPoints.Count > 1)
        {
            List<Vector3> newLerpPoints = new List<Vector3>();

            // Compute Lerp
            for (int i = 0; i < lerpPoints.Count - 1; i++)
            {
                Vector3 u = lerpPoints[i];
                Vector3 v = lerpPoints[i + 1];

                Vector3 w = Vector3.Lerp(u, v, t);
                newLerpPoints.Add(w);
            }

            lerpPoints = newLerpPoints;
        }

        return lerpPoints[0];
    }
    private void DrawBezier(int curveIndex)
    {

        switch (_methodtype)
        {
            case DelegateType.ComputeBerstein:
                computeMethod = ComputeBerstein;
                break;
            case DelegateType.ComputeMatrix:
                computeMethod = ComputeMatrix;
                break;
            case DelegateType.ComputeDeCasteljau:
                computeMethod = ComputeDeCasteljau;
                break;
            default:
                computeMethod = ComputeBerstein;
                break;
        }
        

        // Init gradient
        gradient = new Gradient();

        // Create color keys array
        GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];

        // Set color keys dynamically
        for (int i = 0; i < colors.Length; i++)
        {
            colorKeys[i].color = colors[i];
            colorKeys[i].time = i / (float)(colors.Length - 1); // Distribute colors evenly along the gradient timeline
        }

        // Define alpha keys (you can adjust this part as needed)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f),
            new GradientAlphaKey(1.0f, 1.0f),
            new GradientAlphaKey(1.0f, 1.0f)
        };

        // Set the color keys and alpha keys to the Gradient
        gradient.SetKeys(colorKeys, alphaKeys);


        Vector3 previous_pt = curves[curveIndex].P0.transform.position;

        for (int i = 1; i < numberOfPoints; i++)
        {
            Vector3 pt = computeMethod(curves[curveIndex].GetControlPoints(), (float)i / numberOfPoints);
            Gizmos.color = gradient.Evaluate((float)i / numberOfPoints);
            Gizmos.DrawLine(previous_pt, pt);
            previous_pt = pt;
        }
    }

    private void DrawPolygone(int curveIndex)
    {
        for (int i = 0; i < curves[curveIndex].GetControlPoints().Count - 1; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(curves[curveIndex].GetControlPoints()[i].transform.position, curves[curveIndex].GetControlPoints()[i + 1].transform.position);
        }
    }

    public void CreateControlPoints(Vector3[] controlPointPositions)
    {
        Curve curve = new Curve();
        
        for (int i = 0; i < 4; i++)
        {
            PointController controlPoint = new GameObject().AddComponent<PointController>();
            controlPoint.transform.parent = transform;
            controlPoint.gameObject.name = $"P{i}";
            controlPoint.color = colors[i];
            controlPoint.transform.position = (i == 0 && curves.Count > 0) ? jointPoint.transform.position : controlPointPositions[i];
            curve.AddControlPoint(controlPoint);
        }
        curves.Add(curve);
        jointPoint = curve.P3;
    }
}


public class Curve
{
    private List<PointController> controlPoints = new List<PointController>();

    public PointController P0 { get { return controlPoints[0]; } }
    public PointController P1 { get { return controlPoints[1]; } }
    public PointController P2 { get { return controlPoints[2]; } }
    public PointController P3 { get { return controlPoints[3]; } }

    public List<PointController> GetControlPoints()
    {
        return controlPoints;
    }
    public void AddControlPoint(PointController controlPoint)
    {
        controlPoints.Add(controlPoint);
    }
}
