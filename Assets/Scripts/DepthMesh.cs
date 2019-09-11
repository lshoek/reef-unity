
using UnityEngine;

public class DepthMesh
{
    public Vector3[] OrigVerts { get; private set; }

    public Mesh mesh;
    public Vector3[] verts;
    public int[] triangles;

    public int Width, Height;

    public DepthMesh(int width, int height)
    {
        mesh = new Mesh();
        verts = new Vector3[width * height];
        triangles = new int[6 * ((width - 1) * (height - 1))];

        OrigVerts = new Vector3[width * height];

        Width = width;
        Height = height;

        mesh.MarkDynamic();
        CreateMesh();
    }

    private void CreateMesh()
    {
        int triangleIndex = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = (y * Width) + x;

                verts[index] = new Vector3(x, -y, 0);
                OrigVerts[index] = new Vector3(x, -y, 0);

                // Skip the last row/col
                if (x != (Width - 1) && y != (Height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + Width;
                    int bottomRight = bottomLeft + 1;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }
        Apply();

        // set triangles once
        mesh.triangles = triangles;
    }

    public void Apply()
    {
        mesh.vertices = verts;
    }
}
