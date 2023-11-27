using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix<T> where T : struct
{
    public T[,] elements;

    private int rows;
    private int columns;

    public string name = "";

    public Matrix(T[,] data)
    {
        elements = data;

        rows = data.GetLength(0);
        columns = data.GetLength(1);
    }

    public Matrix(int n, int m)
    {
        rows = n;
        columns = m;

        elements = new T[n, m];
    }

    public static Matrix<V> Multiplication<U, V>(Matrix<U> A, Matrix<V> B) where U : struct where V : struct
    {
        Matrix<V> result = new Matrix<V>(A.GetRowCount(), B.GetColumnCount());

        if (A.GetColumnCount() != B.GetRowCount())
        {
            Debug.Log($"Error matrix multiplication, number of columns of matrix {A.name} of dismension [{A.GetRowCount()}, {A.GetColumnCount()}] " +
                $"should be equal to number of rows of matrix {B.name} of dimension [{B.GetRowCount()}, {B.GetColumnCount()}]");
            return null;
        }

        for (int i = 0; i < A.GetRowCount(); i++)
        {
            for (int y = 0; y < B.GetColumnCount(); y++)
            {
                dynamic sum = default(V); // Initialize the sum to the default value of T

                for (int j = 0; j < A.GetColumnCount(); j++)
                {
                    // Add a check for numeric types to avoid runtime errors
                    if (typeof(U) == typeof(float) && typeof(V) == typeof(float))
                    {
                        sum += (dynamic)A.elements[i, j] * (dynamic)B.elements[j, y];
                    }
                    else if (typeof(U) == typeof(float) && typeof(V) == typeof(Vector3))
                    {
                        sum += (dynamic)A.elements[i, j] * (dynamic)B.elements[j, y];
                    }

                }

                result.elements[i, y] = (V)sum;
            }
        }

        return result;
    }

    public void PrintMatrix()
    {

        for(int i = 0; i < this.GetRowCount(); i++)
        {
            string row = "[ ";
            for (int j = 0; j < this.GetColumnCount(); j++)
            {
                row += this.elements[i, j].ToString() + ", ";
            }
            Debug.Log("]");
        }
    }
    public int GetColumnCount()
    {
        return columns;
    }

    public int GetRowCount()
    {
        return rows;
    }
}
