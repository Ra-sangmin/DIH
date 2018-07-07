using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMesh
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public Color[] colors;
    public int[] triangles;

    public SphereMesh(Vector3[] verts, Vector3[] norms, Vector2[] uvs, Color[] clrs, int[] tris)
    {
        vertices = verts;
        normals = norms;
        this.uvs = uvs;
        colors = clrs;
        triangles = tris;
    }
}

public delegate void SphereVertexDisplace(Vector3 dir, out Vector3 vertPos, out Vector3 vertNormal, out Vector2 uvs, out Color vertColor);

public static class SphereMesher
{
    public static SphereMesh GenerateSphere(int resolution, SphereVertexDisplace displace)
    {
        int vertCount = (resolution - 1) * (resolution - 1) * 6 + ((resolution) * 8 + (resolution - 1) * 4);
        int triCount = (resolution) * (resolution) * 36;

        Vector3[] verts = new Vector3[vertCount];
        Vector3[] nrms = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        Color[] clrs = new Color[vertCount];
        int[] tris = new int[triCount];

        int cIndex = 0;
        int cTri = 0;

        int i, j;
        int[] edgeOffsets = new int[12];

        for (i = 0; i < 8; i++)
        {
            verts[i] = corners[i];
            cIndex++;
        }
        for (i = 0; i < 12; i++)
        {
            Vector3 fCorner = corners[edgeCorners[i, 0]];
            Vector3 tCorner = corners[edgeCorners[i, 1]];
            Vector3 interval = (tCorner - fCorner) / resolution;
            Vector3 current = fCorner;
            edgeOffsets[i] = cIndex;
            for (j = 0; j < resolution - 1; j++)
            {
                current += interval;
                verts[cIndex] = current;
                cIndex++;
            }
        }

        //Z -
        GenerateFace(ref cIndex, ref cTri, edgeOffsets, 0, -2, -3, 1, verts, tris, vertCount, resolution);
        //Z +
        GenerateFace(ref cIndex, ref cTri, edgeOffsets, -8, 10, 9, -11, verts, tris, vertCount, resolution);
        //X -
        GenerateFace(ref cIndex, ref cTri, edgeOffsets, -4, -7, -11, -3, verts, tris, vertCount, resolution);
        //X +
        GenerateFace(ref cIndex, ref cTri, edgeOffsets, 5, 6, 1, 9, verts, tris, vertCount, resolution);
        //Y -
        GenerateFace(ref cIndex, ref cTri, edgeOffsets, 8, 0, -4, -5, verts, tris, vertCount, resolution);
        //Y +
        GenerateFace(ref cIndex, ref cTri, edgeOffsets, -2, -10, 7, 6, verts, tris, vertCount, resolution);

        for (i = 0; i < vertCount; i++)
        {
            Vector3 vertPos;
            Vector3 vertNrm;
            Vector2 vertUvs;
            Color vertColor;
            displace(verts[i].normalized, out vertPos, out vertNrm, out vertUvs, out vertColor);
            verts[i] = vertPos;
            nrms[i] = vertNrm;
            uvs[i] = vertUvs;
            clrs[i] = vertColor;
        }

        return new SphereMesh(verts, nrms, uvs, clrs, tris);
    }

    private static void GenerateFace(ref int cIndex, ref int cTri, int[] edgeOffsets, int bf, int tf, int lf, int rf, Vector3[] verts, int[] tris, int vertCount, int resolution)
    {
        int faceOffset = cIndex;

        int blc = GetLeftCorner(bf);
        int brc = GetRightCorner(bf);
        int tlc = GetLeftCorner(tf);

        Vector3 origin = corners[blc];
        Vector3 xinterval = (corners[brc] - origin) / resolution;
        Vector3 yinterval = (corners[tlc] - origin) / resolution;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                if (x != 0 && y != 0)
                {
                    verts[cIndex] = origin + (xinterval * x) + (yinterval * y);
                    cIndex++;
                }

                int i00 = GetCubeIndex(x, y, edgeOffsets, faceOffset, bf, tf, lf, rf, resolution);
                int i10 = GetCubeIndex(x + 1, y, edgeOffsets, faceOffset, bf, tf, lf, rf, resolution);
                int i11 = GetCubeIndex(x + 1, y + 1, edgeOffsets, faceOffset, bf, tf, lf, rf, resolution);
                int i01 = GetCubeIndex(x, y + 1, edgeOffsets, faceOffset, bf, tf, lf, rf, resolution);

                tris[cTri] = i00;
                cTri++;
                tris[cTri] = i01;
                cTri++;
                tris[cTri] = i11;
                cTri++;
                tris[cTri] = i00;
                cTri++;
                tris[cTri] = i11;
                cTri++;
                tris[cTri] = i10;
                cTri++;
            }
        }
    }

    private static int GetCubeIndex(int x, int y, int[] edgeOffsets, int faceOffset, int bf, int tf, int lf, int rf, int resolution)
    {
        if (x == 0)
        {
            if (y == 0)
                return GetLeftCorner(bf);
            else if (y == resolution)
                return GetLeftCorner(tf);
            else
                return edgeOffsets[Mathf.Abs(lf)] + (lf < 0 ? resolution - y - 1 : y - 1);
        }
        else if (x == resolution)
        {
            if (y == 0)
                return GetRightCorner(bf);
            else if (y == resolution)
                return GetRightCorner(tf);
            else
                return edgeOffsets[Mathf.Abs(rf)] + (rf < 0 ? resolution - y - 1 : y - 1);
        }
        else if (y == 0)
        {
            return edgeOffsets[Mathf.Abs(bf)] + (bf < 0 ? resolution - x - 1 : x - 1);
        }
        else if (y == resolution)
        {
            return edgeOffsets[Mathf.Abs(tf)] + (tf < 0 ? resolution - x - 1 : x - 1);
        }
        else
        {
            return faceOffset + ((y - 1) + (x - 1) * (resolution - 1));
        }
    }

    private static int GetLeftCorner(int f)
    {
        if (f < 0)
            return edgeCorners[-f, 1];
        else
            return edgeCorners[f, 0];
    }

    private static int GetRightCorner(int f)
    {
        if (f < 0)
            return edgeCorners[-f, 0];
        else
            return edgeCorners[f, 1];
    }

private static Vector3[] corners = new Vector3[8]
    {
            new Vector3(-1,-1,-1),
            new Vector3(1,-1,-1),
            new Vector3(1,1,-1),
            new Vector3(-1,1,-1),
            new Vector3(-1,-1,1),
            new Vector3(1,-1,1),
            new Vector3(1,1,1),
            new Vector3(-1,1,1),
    };

private static int[,] edgeCorners = new int[,]
    {
            { 0,1 },
            { 1,2 },
            { 2,3 },
            { 3,0 },
            { 0,4 },
            { 1,5 },
            { 2,6 },
            { 3,7 },
            { 4,5 },
            { 5,6 },
            { 6,7 },
            { 7,4 },
    };
}
