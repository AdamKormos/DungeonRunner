using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Bound
{
    [SerializeField] int min, max;

    public bool Fits(int val)
    {
        return val >= min && val <= max;
    }
}

public struct Bound2D
{
    [SerializeField] public int minX, maxX, minY, maxY;

    public Bound2D(int minX, int maxX, int minY, int maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    public Bound2D GetIntersection(Bound2D other)
    {
        Bound2D topLeft, bottomRight;

        if (other.minX > this.minX)
        {
            topLeft = this;
            bottomRight = other;
        }
        else
        {
            topLeft = other;
            bottomRight = this;
        }

        if (topLeft.maxY < bottomRight.maxY)
        {
            Bound2D temp = bottomRight;
            bottomRight = topLeft;
            topLeft = temp;
        }

        //Debug.Log("(" + topLeft.minX + " " + topLeft.maxY + "), (" + bottomRight.minX + ", " + bottomRight.maxY + ")");
        Bound2D result = new Bound2D(bottomRight.minX, bottomRight.maxX > topLeft.maxX ? topLeft.maxX : bottomRight.maxX, topLeft.minY, bottomRight.maxY > topLeft.maxY ? topLeft.maxY : bottomRight.maxY);
        //Debug.Log(result.minX + " " + result.minY + " " + result.maxX + " " + result.maxY);
        return result;
    }

    public bool Fits(Vector2Int vec)
    {
        return vec.x >= minX && vec.x <= maxX && vec.y >= minY && vec.y <= maxX;
    }
}

public struct BoundF
{
    [SerializeField] float min, max;

    public bool Fits(float val)
    {
        return val >= min && val <= max;
    }
}
