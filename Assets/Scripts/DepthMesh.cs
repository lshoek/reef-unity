
using UnityEngine;

public class DepthMesh
{
    //public Vector3[] OrigVerts { get; private set; }

    public Mesh mesh;
    public Vector3[] verts;
    public int[] triangles;
    public Vector2[] uv;

    public int Width, Height;
    public bool ApplyUVs = false;

    private Vector2 offset;

    public DepthMesh(int width, int height)
    {
        mesh = new Mesh();
        verts = new Vector3[width * height];
        triangles = new int[6 * (width - 1) * (height - 1)];
        uv = new Vector2[width * height];
        offset = new Vector2(0, 0);

        Width = width;
        Height = height;

        mesh.MarkDynamic();
    }

    public void Init(Vector2 size, bool generateUVs)
    {
        int triangleIndex = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = (y * Width) + x;

                float xPos = (x+offset.x) / Width * size.x;
                float yPos = (y+offset.y) / Height * size.y;

                verts[index] = new Vector3(xPos, -yPos, 0);

                if (generateUVs)
                {
                    uv[index] = new Vector2((float)x / Width, (float)y / Height);
                }
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

        if (generateUVs && !ApplyUVs)
        {
            mesh.uv = uv;
        }
        // set triangles once
        mesh.triangles = triangles;
    }

    public void SetOffset(Vector2 offset)
    {
        this.offset = offset;
    }

    public void Apply()
    {
        mesh.vertices = verts;

        if (ApplyUVs) {
            mesh.uv = uv;
        }
    }
}
