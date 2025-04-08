using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BlastManager : MonoBehaviour
{
    public static BlastManager Instance { get; private set; }
    GridManager gridManager;
    PoolManager poolManager;
    public float spacing;
    [SerializeField] private CubeMover cubeMover;
    public HashSet<int> pendingAffectedColumns = new HashSet<int>();
    private bool isShiftScheduled = false;

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

    public void BlastCubes(List<ColoredCube> matchedCubes)
    {
        HashSet<int> affectedColumns = new HashSet<int>();
        Vector2Int tntPosition = Vector2Int.zero;
        bool createTNT = matchedCubes.Count >= 5;

        foreach (var cube in matchedCubes)
        {
            Vector2Int pos = cube.GetGridPosition();
            gridManager.grid[pos.x, pos.y] = null; // Küpü grid'den kaldır
            RemoveCube(cube); // Küpü yok et
            affectedColumns.Add(pos.x);

            if (createTNT)
            {
                tntPosition += pos;
            }
        }

        if (createTNT)
        {
            tntPosition /= matchedCubes.Count; // Patlayan küplerin ortalama konumunu al
            TNT.CreateTNT(tntPosition.x, tntPosition.y);
        }

        // Obstacle'lara hasar verelim
        DamageAdjacentObstacles(matchedCubes, affectedColumns, gridManager.grid);

        // Shift ve Spawn işlemlerini sıraya sokalım
        StartCoroutine(HandleShiftAndSpawn(affectedColumns));
    }

    private IEnumerator HandleShiftAndSpawn(HashSet<int> affectedColumns)
    {

        yield return new WaitForSeconds(0.2f); // Küplerin yok olmasını bekleyelim (Animasyonlar varsa artır)

        foreach (int col in affectedColumns)
        {
            CubeMover.Instance.ShiftCubesDown(col); // Küpler aşağı düşsün
        }

        yield return new WaitForSeconds(0.5f); // Düşme işleminin tamamlanmasını bekle

        foreach (int col in affectedColumns)
        {
            CubeMover.Instance.SpawnNewCubes(col); // Yeni küpler spawn edilsin
        }

        yield return new WaitForSeconds(0.3f); // Yeni küplerin yerleşmesini bekleyelim

        // Yeni oluşan eşleşmeleri tekrar kontrol edelim (Zincirleme patlama için)
        CheckAndBlastAllMatches();
    }


    public List<ColoredCube> FindMatches()
    {
        Cube[,] grid = gridManager.grid;
        List<ColoredCube> matchedCubes = new List<ColoredCube>();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] is ColoredCube currentCube)
                {
                    List<ColoredCube> horizontalMatch = GetMatchInDirection(x, y, Vector2Int.right);
                    List<ColoredCube> verticalMatch = GetMatchInDirection(x, y, Vector2Int.up);

                    if (horizontalMatch.Count >= 3) matchedCubes.AddRange(horizontalMatch);
                    if (verticalMatch.Count >= 3) matchedCubes.AddRange(verticalMatch);
                }
            }
        }

        return matchedCubes;
    }

    private List<ColoredCube> GetMatchInDirection(int startX, int startY, Vector2Int direction)
    {
        Cube[,] grid = gridManager.grid;
        List<ColoredCube> matchList = new List<ColoredCube>();

        if (grid[startX, startY] is not ColoredCube startCube) return matchList;

        matchList.Add(startCube);

        for (int i = 1; i < grid.GetLength(0); i++)
        {
            int newX = startX + direction.x * i;
            int newY = startY + direction.y * i;

            if (newX >= grid.GetLength(0) || newY >= grid.GetLength(1) || newX < 0 || newY < 0)
                break;

            if (grid[newX, newY] is ColoredCube neighborCube && neighborCube.GetColor() == startCube.GetColor())
            {
                matchList.Add(neighborCube);
            }
            else
            {
                break;
            }
        }

        return matchList;
    }


    private List<Vector2Int> GetAdjacentPositions(int x, int y)
    {
        Cube[,] grid = gridManager.grid;
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            if (newX >= 0 && newX < grid.GetLength(0) && newY >= 0 && newY < grid.GetLength(1))
            {
                neighbors.Add(new Vector2Int(newX, newY));
            }
        }

        return neighbors;
    }

    private void RemoveCube(ColoredCube cube)
    {
        cube.gameObject.SetActive(false);

        string colorKey = cube.GetColor().ToString();
        poolManager.ReturnToPool(cube.gameObject, colorKey);
    }


    public void CheckAndBlastAllMatches()
    {
        Cube[,] grid = gridManager.grid;
        HashSet<ColoredCube> matchedCubes = new HashSet<ColoredCube>();

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] is ColoredCube cube)
                {
                    List<ColoredCube> horizontalMatch = GetMatchInDirection(x, y, Vector2Int.right);
                    List<ColoredCube> verticalMatch = GetMatchInDirection(x, y, Vector2Int.up);

                    if (horizontalMatch.Count >= 3) matchedCubes.UnionWith(horizontalMatch);
                    if (verticalMatch.Count >= 3) matchedCubes.UnionWith(verticalMatch);
                }
            }
        }

        if (matchedCubes.Count > 0)
        {
            BlastCubes(new List<ColoredCube>(matchedCubes));
        }
    }

    private void DamageAdjacentObstacles(List<ColoredCube> connectedCubes, HashSet<int> affectedColumns, Cube[,] grid)
    {
        HashSet<Vector2Int> processedObstacles = new HashSet<Vector2Int>();
        foreach (var cube in connectedCubes)
        {
            Vector2Int pos = cube.GetGridPosition();
            List<Vector2Int> adjacentPositions = GetAdjacentPositions(pos.x, pos.y);
            foreach (Vector2Int adjacent in adjacentPositions)
            {
                if (!processedObstacles.Contains(adjacent))
                {
                    if (grid[adjacent.x, adjacent.y] is ObstacleCube obstacle && obstacle.AffectedByBlast)
                    {
                        obstacle.TakeDamage();
                        // If the obstacle is destroyed, its TakeDamage() should set the grid cell to null.
                        if (grid[adjacent.x, adjacent.y] == null)
                        {
                            affectedColumns.Add(adjacent.x);
                        }
                    }
                    processedObstacles.Add(adjacent);
                }
            }
        }
    }

}
