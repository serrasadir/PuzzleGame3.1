using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMover : MonoBehaviour
{
    public static CubeMover Instance { get; private set; }
    GridManager gridManager;
    PoolManager poolManager;
    public float spacing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
        poolManager = PoolManager.Instance;
        spacing = gridManager.spacingSize * gridManager.cubeSize;
    }
    public void ShiftCubesDown(int x)
    {
        Cube[,] grid = gridManager.grid;

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            if (grid[x, y] == null)
            {
                int aboveY = y + 1;

                while (aboveY < grid.GetLength(1) && grid[x, aboveY] == null)
                {
                    aboveY++; //find the topmost cube
                }

                if (aboveY < grid.GetLength(1) && grid[x, aboveY] != null)
                {
                    Cube fallingCube = grid[x, aboveY];
                    grid[x, y] = fallingCube;
                    grid[x, aboveY] = null;

                    Vector3 targetPos = gridManager.GetWorldPosition(x, y);
                    fallingCube.MoveToTargetPos(targetPos);
                    fallingCube.SetGridPosition(x, y);
                }
            }
        }
    }

    public void SpawnNewCubes(int x)
    {
        Cube[,] grid = gridManager.grid;
        Debug.Log($"Spawning new cubes in column {x}");

        for (int y = grid.GetLength(1) - 1; y >= 0; y--)
        {
            if (grid[x, y] == null)
            {
                string[] randomColors = gridManager.randomColors;
                string randomType = randomColors[UnityEngine.Random.Range(0, randomColors.Length)];
                GameObject newCubeObj = PoolManager.Instance.GetFromPool(randomType);

                if (newCubeObj != null)
                {
                    Vector3 spawnPosition = gridManager.GetWorldPosition(x, grid.GetLength(1));
                    newCubeObj.transform.position = spawnPosition;

                    Cube newCube = newCubeObj.GetComponent<Cube>();
                    grid[x, y] = newCube;

                    if (newCube != null)
                    {
                        newCube.SetGridPosition(x, y);
                        Vector3 targetPosition = gridManager.GetWorldPosition(x, y);
                        newCube.MoveToTargetPos(targetPosition);
                    }
                }
                else
                {
                    Debug.LogError("Could not spawn new cube, pool returned null!");
                }
            }
        }
    }

}
