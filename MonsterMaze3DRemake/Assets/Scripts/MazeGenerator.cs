using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Random = UnityEngine.Random;


public class MazeGenerator : MonoBehaviour
{
    public Material borderMaterial;
    public Material wallMaterial;

    [SerializeField] private int gridWidth, gridLength;

    [SerializeField] private GameObject player, monster;

    private GameObject[,] wallCells;

    private ProBuilderMesh groundObj;

    
    void Awake()
    {
        int cl = PlayerPrefs.GetInt("CurrentLevel");
        gridWidth = 7 + ((cl/2) * 2);
        gridLength = gridWidth + 2;
        CreateMazeBorders();

        wallCells = new GameObject[gridWidth, gridLength];
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridLength; j++)
            {
                if (i % 2 == 0 && j % 2 == 0) wallCells[i, j] = null;
                else wallCells[i, j] = CreateWallsAtCell(i, j);
            }
        }

        MazeRecurse(null, null, (0, 0));

        groundObj.GetComponent<NavMeshSurface>().BuildNavMesh();

        // Set player and monster position to opposite corners of maze
        player.transform.position = new Vector3(5f, 1.3f, 5f);

        monster.transform.position = new Vector3(gridLength * 10f - 5f, 2.5f, gridWidth * 10f - 5f);
        
    }

    private void CreateMazeBorders()
    {
        // Ground and Ceiling
        for (int i = 0; i < 2; i++)
        {
            ProBuilderMesh ground = ProBuilderMesh.Create(
                new Vector3[]
                {
                new Vector3(0f, 0f, 0f),
                new Vector3(gridLength * 10f, 0f, 0f),
                new Vector3(0f, 0f, gridWidth * 10f),
                new Vector3(gridLength * 10f, 0f, gridWidth * 10f)
                },
                new Face[] {
            new Face(new int[] { 0, 2, 1 }),
            new Face(new int[] { 2, 3, 1}) });
            ground.SetMaterial(ground.faces, borderMaterial);
            ground.Refresh();
            ground.ToMesh();
            if (i == 0)
            {
                ground.AddComponent<BoxCollider>();
                ground.AddComponent<NavMeshSurface>();
                groundObj = ground;
                
            }
            else
            {
                ground.transform.position = new Vector3(gridLength * 10f, 5f, 0);
                ground.transform.rotation = Quaternion.Euler(0, 0, -180f);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            ProBuilderMesh wall = ProBuilderMesh.Create(
                    new Vector3[]
                    {
                new Vector3(Mathf.Max(gridLength, gridWidth) * 10f, 0f, 0f),
                new Vector3(0f, 0f, 0f),
                new Vector3(Mathf.Max(gridLength, gridWidth) * 10f, 5f, 0),
                new Vector3(0f, 5f, 0)
                    },
                    new Face[] {
            new Face(new int[] { 0, 2, 1 }),
            new Face(new int[] { 2, 3, 1}) });
            wall.SetMaterial(wall.faces, wallMaterial);
            wall.Refresh();
            wall.ToMesh();
            wall.AddComponent<BoxCollider>();
            wall.AddComponent<NavMeshObstacle>();
            wall.GetComponent<NavMeshObstacle>().carving = true;
            wall.GetComponent<NavMeshObstacle>().size = new Vector3(Mathf.Max(gridLength, gridWidth) * 10f, 5f, 5f);

            switch (i)
            {
                case 1: // bottom
                    wall.transform.position = new Vector3(gridLength * 10f, 0, 0);
                    wall.transform.rotation = Quaternion.Euler(0, -90f, 0);
                    break;
                case 2: // right
                    wall.transform.position = new Vector3(gridLength * 10f, 0, gridWidth * 10f);
                    wall.transform.rotation = Quaternion.Euler(0, -180f, 0);
                    break;
                case 3: // top
                    wall.transform.position = new Vector3(0, 0, gridWidth * 10f);
                    wall.transform.rotation = Quaternion.Euler(0, 90f, 0);
                    break;

            }
        }


    }

    private GameObject CreateWallsAtCell(int cx, int cy)
    {
        GameObject wallCell = new GameObject("WallCell");
        for (int i = 0; i < 4; i++)
        {
            ProBuilderMesh wall = ProBuilderMesh.Create(
            new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(10f, 0f, 0f),
                new Vector3(0f, 5f, 0f),
                new Vector3(10f, 5f, 0f)
            },
            new Face[] {
            new Face(new int[] { 0, 2, 1 }),
            new Face(new int[] { 2, 3, 1})
            });
            wall.SetMaterial(wall.faces, wallMaterial);
            wall.Refresh();
            wall.ToMesh();
            wall.AddComponent<BoxCollider>();

            switch (i)
            {
                case 1: // bottom
                    wall.transform.position = new Vector3(10f, 0, 0);
                    wall.transform.rotation = Quaternion.Euler(0, -90f, 0);
                    break;
                case 2: // right
                    wall.transform.position = new Vector3(10f, 0, 10f);
                    wall.transform.rotation = Quaternion.Euler(0, -180f, 0);
                    break;
                case 3: // top
                    wall.transform.position = new Vector3(0, 0, 10f);
                    wall.transform.rotation = Quaternion.Euler(0, 90f, 0);
                    break;

            }
            wall.transform.SetParent(wallCell.transform, true);
            wall.AddComponent<NavMeshObstacle>();
            wall.GetComponent<NavMeshObstacle>().carving= true;
            wall.GetComponent<NavMeshObstacle>().center = new Vector3(5f, 2.5f, 1.5f);
            wall.GetComponent<NavMeshObstacle>().size = new Vector3(15f, 5f, 8f);
        }
        wallCell.transform.position = new Vector3(cy * 10f, 0, cx * 10f);
        return wallCell;
    }

    // Aldous-Broder Algorithm
    private void MazeRecurse(List<(int, int)> visitedTiles, List<(int, int)> unfinishedTiles, (int cx, int cy) t)
    {
        if (visitedTiles == null) {
            visitedTiles = new List<(int, int)>();
            unfinishedTiles = new List<(int, int)>();
        }
        visitedTiles.Add((t.cx, t.cy));

        List<((int cx, int y) t, GameObject wall)> possibleDir = new List<((int cx, int y) t, GameObject wall)>();

        if(t.cx + 2 < gridWidth) // Right
        {
            if(!visitedTiles.Contains((t.cx + 2, t.cy)))
            {
                possibleDir.Add(((t.cx + 2, t.cy), wallCells[t.cx + 1, t.cy]));
            }
        }

        if (t.cx - 2 >= 0) // Left
        {
            if (!visitedTiles.Contains((t.cx - 2, t.cy)))
            {
                possibleDir.Add(((t.cx - 2, t.cy), wallCells[t.cx - 1, t.cy]));
            }
        }

        if (t.cy - 2 >= 0) // Up
        {
            if (!visitedTiles.Contains((t.cx, t.cy - 2)))
            {
                possibleDir.Add(((t.cx, t.cy - 2), wallCells[t.cx, t.cy - 1]));
            }
        }


        if (t.cy + 2 < gridLength) // Down
        {
            if (!visitedTiles.Contains((t.cx, t.cy + 2)))
            {
                possibleDir.Add(((t.cx, t.cy + 2), wallCells[t.cx, t.cy + 1]));
            }
        }

        if (possibleDir.Count > 0)
        {
            if (possibleDir.Count > 1)
            {
                int chance = Random.Range(0, 5);

                if(chance < 3) // Chance to branch out twice
                {
                    unfinishedTiles.Add((t.cx, t.cy));
                } else
                {
                    GameObject.Destroy(possibleDir[possibleDir.Count - 1].wall);
                    visitedTiles.Add(possibleDir[possibleDir.Count - 1].t);
                    MazeRecurse(visitedTiles, unfinishedTiles, possibleDir[possibleDir.Count - 1].t);
                    possibleDir.RemoveAt(possibleDir.Count - 1);
                }
            }
            int r = Random.Range(0, possibleDir.Count); // choose random direction to branch
            GameObject.Destroy(possibleDir[r].wall);
            visitedTiles.Add(possibleDir[r].t);
            MazeRecurse(visitedTiles, unfinishedTiles, possibleDir[r].t);
        } else if (unfinishedTiles.Count > 1)
        { // add to array for backtracking
            (int cx, int cy) backtrackTile = unfinishedTiles[unfinishedTiles.Count - 1];
            unfinishedTiles.RemoveAt(unfinishedTiles.Count - 1);
            MazeRecurse(visitedTiles, unfinishedTiles, backtrackTile);
        }

    }

    public GameObject[,] getWallCells()
    {
        return wallCells;
    }

    public int getGridWidth() { return gridWidth; }
    public int getGridLength() { return gridLength; }
}
