using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMaker
{
    public static Mesh Cylinder(float width, float height, int nSegments, int nSides)
    {
        float segmentLength = height / nSegments;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(nSegments + 1) * nSides];
        List<int> triangles = new List<int>();
        const float Tau = 2 * Mathf.PI;

        for (int i = 0; i <= nSegments; i++)
        {
            for (int j = 0; j < nSides; j++)
            {
                vertices[i * nSides + j] = new Vector3(width / 2 * Mathf.Cos(Tau * j / nSides),
                                                       -i * segmentLength,
                                                       width / 2 * Mathf.Sin(Tau * j / nSides));
            }
        }

        for (int i = 0; i < nSegments; i++)
        {
            for (int j = 0; j < nSides; j++)
            {
                /*
                 *         i*n      i*nSides + j        i*nSides + j + 1
                 *                                ____
                 *                               |\   |
                 *                               |  \ |
                 *                               |___\|
                 *     (i+1)*n      (i+1)*nSides + j    (i+1)*nSides + j + 1
                 *
                 * */

                triangles.Add(i * nSides + (j + 1) % nSides);
                triangles.Add((i + 1) * nSides + (j + 1) % nSides);
                triangles.Add(i * nSides + j);

                triangles.Add(i * nSides + j);
                triangles.Add((i + 1) * nSides + (j + 1) % nSides);
                triangles.Add((i + 1) * nSides + j);
            }
        }
        for (int i = 2; i < nSides; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i - 1);
        }
        for (int i = 2; i < nSides; i++)
        {
            triangles.Add(vertices.Length - nSides);
            triangles.Add(vertices.Length - nSides + i - 1);
            triangles.Add(vertices.Length - nSides + i);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }
}