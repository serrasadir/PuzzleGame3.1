using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColoredCube : Cube
{
    
    [SerializeField] private CubeColor cubeColor;

    private void OnMouseDown()
    {
        SwapManager.Instance.SelectCube(this);
    }


    public CubeColor GetColor()
    {
        return cubeColor;
    }
}


public enum CubeColor
{
    r,
    b,
    g,
    y
}
