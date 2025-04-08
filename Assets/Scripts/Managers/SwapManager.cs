using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapManager : MonoBehaviour
{
    public static SwapManager Instance { get; private set; }
    private GridManager gridManager;
    private ColoredCube firstSelectedCube = null;
    private ColoredCube secondSelectedCube = null;

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
    }

    public void SelectCube(ColoredCube cube)
    {
        if (firstSelectedCube == null)
        {
            firstSelectedCube = cube;
        }
        else
        {
            secondSelectedCube = cube;
            TrySwap();
        }
    }

    private void TrySwap()
    {
        if (firstSelectedCube == null || secondSelectedCube == null)
            return;

        Vector2Int firstPos = firstSelectedCube.GetGridPosition();
        Vector2Int secondPos = secondSelectedCube.GetGridPosition();

        if (AreAdjacent(firstPos, secondPos))
        {
            StartCoroutine(SwapAndCheck(firstSelectedCube, secondSelectedCube));
        }
        else
        {
            Debug.Log("Cubes are not adjacent!");
        }

        firstSelectedCube = null;
        secondSelectedCube = null;
    }

    private bool AreAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y) == 1;
    }

    private IEnumerator SwapAndCheck(ColoredCube cube1, ColoredCube cube2)
    {
        Vector2Int pos1 = cube1.GetGridPosition();
        Vector2Int pos2 = cube2.GetGridPosition();

        // Swap positions in the grid
        gridManager.grid[pos1.x, pos1.y] = cube2;
        gridManager.grid[pos2.x, pos2.y] = cube1;

        // Move visually
        Vector3 tempPos = cube1.transform.position;
        cube1.MoveToTargetPos(cube2.transform.position);
        cube2.MoveToTargetPos(tempPos);

        cube1.SetGridPosition(pos2.x, pos2.y);
        cube2.SetGridPosition(pos1.x, pos1.y);

        yield return new WaitForSeconds(0.3f); 

        // Check for matches
        List<ColoredCube> matchedCubes = BlastManager.Instance.FindMatches();

        if (matchedCubes.Count >= 3)
        {
            BlastManager.Instance.BlastCubes(matchedCubes);
        }
        else
        {
            // Swap back if no match
            gridManager.grid[pos1.x, pos1.y] = cube1;
            gridManager.grid[pos2.x, pos2.y] = cube2;

            cube1.MoveToTargetPos(tempPos);
            cube2.MoveToTargetPos(cube1.transform.position);

            cube1.SetGridPosition(pos1.x, pos1.y);
            cube2.SetGridPosition(pos2.x, pos2.y);
        }
        yield return new WaitForSeconds(0.3f);
        BlastManager.Instance.CheckAndBlastAllMatches();

    }

}
