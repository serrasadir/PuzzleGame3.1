using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class TNT : Cube
{
    private bool hasExploded = false;
    [SerializeField] private int explosionRadius = 1;


    private void OnMouseDown()
    {
        GridManager.Instance.DecreaseMoveCount();
        Explode();
    }

    public void Explode()
    {
        if (hasExploded)
            return;
        hasExploded = true;

        Vector2Int pos = GetGridPosition();
        Cube[,] grid = GridManager.Instance.grid;
        grid[pos.x, pos.y] = null;

        HashSet<int> affectedColumns = new HashSet<int>();
        affectedColumns.Add(pos.x);

        for (int i = pos.x - explosionRadius; i <= pos.x + explosionRadius; i++)
        {
            for (int j = pos.y - explosionRadius; j <= pos.y + explosionRadius; j++)
            {
                if (i >= 0 && i < grid.GetLength(0) && j >= 0 && j < grid.GetLength(1))
                {
                    Cube cube = grid[i, j];
                    if (cube != null)
                    {
                        affectedColumns.Add(i);

                        if (cube is TNT otherTNT)
                        {
                            if (!otherTNT.hasExploded)
                            {
                                otherTNT.Explode();
                            }
                        }
                        else if (cube is ObstacleCube obstacle)
                        {
                            obstacle.TakeDamage();
                        }
                        else if (cube is ColoredCube coloredCube)
                        {
                            grid[i, j] = null;
                            string colorKey = coloredCube.GetColor().ToString();
                            PoolManager.Instance.ReturnToPool(coloredCube.gameObject, colorKey);
                        }
                        else
                        {
                            grid[i, j] = null;
                            cube.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        Debug.Log("TNT explosion");
        Debug.Log("Affected Columns: " + string.Join(",", affectedColumns));
        // Coroutine çalıştır ama TNT'yi hemen yok etme
        StartCoroutine(HandleShiftAndDestroy(affectedColumns));
    }

    private IEnumerator HandleShiftAndDestroy(HashSet<int> affectedColumns)
    {
        Debug.Log("Coroutine başladı: TNT Shift ve Spawn işlemi yapıyor.");
        yield return new WaitForSeconds(0.2f);

        foreach (int col in affectedColumns)
        {
            Debug.Log($"Küpler aşağı düşüyor, sütun: {col}");
            CubeMover.Instance.ShiftCubesDown(col);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (int col in affectedColumns)
        {
            Debug.Log($"Yeni küpler spawn ediliyor, sütun: {col}");
            CubeMover.Instance.SpawnNewCubes(col);
        }

        yield return new WaitForSeconds(0.3f);

        Debug.Log("Zincirleme patlama kontrolü başlatılıyor...");
        BlastManager.Instance.CheckAndBlastAllMatches();

        yield return new WaitForSeconds(0.2f);

        Debug.Log("TNT yok ediliyor.");
        Destroy(gameObject); // TNT'yi en sona taşıdık!
    }

    public static void CreateTNT(int x, int y)
    {
        TNT tntPrefab = Resources.Load<TNT>("TNT");
        Vector2 worldPos = GridManager.Instance.GetWorldPosition(x, y);

        TNT newTNT = Instantiate(tntPrefab, worldPos, Quaternion.identity, GridManager.Instance.GetGridParent());
        newTNT.SetGridPosition(x, y);

        GridManager.Instance.grid[x, y] = newTNT;
    }

}
