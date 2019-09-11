using UnityEngine;

public static class BoidHelper
{
    const int numViewDirections = 128;
    public static readonly Vector2[] directions;

    static BoidHelper()
    {
        directions = new Vector2[numViewDirections];

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float x = Mathf.Sin((t-0.5f) * Mathf.PI);
            float y = Mathf.Cos((t - 0.5f) * Mathf.PI);
            directions[i] = new Vector2(x, y);
        }
    }
}
