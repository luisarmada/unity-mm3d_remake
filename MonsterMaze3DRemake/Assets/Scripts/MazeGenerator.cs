using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class MazeGenerator : MonoBehaviour
{
    public Material borderMaterial;
    public Material wallMaterial;

    [SerializeField] private int gridWidth, gridLength;

    // Start is called before the first frame update
    void Awake()
    {
        createMazeBorders();

        for (int i = 1; i < gridWidth; i += 2)
        {
            for (int j = 1; j < gridLength; j += 2)
            {
                CreateWallsAtCell(i, j);
            }
        }
    }

    private void createMazeBorders()
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
            wall.SetMaterial(wall.faces, borderMaterial);
            wall.Refresh();
            wall.ToMesh();
            wall.AddComponent<BoxCollider>();

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

    private void CreateWallsAtCell(int cx, int cy)
    {
        GameObject wallCell = new GameObject("WallCell");
        for(int i=0; i<4; i++)
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

            
        }
        wallCell.transform.position = new Vector3(cy * 10f, 0, cx * 10f);

    }
}
