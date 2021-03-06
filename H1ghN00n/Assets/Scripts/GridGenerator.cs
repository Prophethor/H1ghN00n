using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{

    public enum Direction { UL, UR, L, C, R, DL, DR };
    public Dictionary<Direction, Vector2Int> HexDir = new Dictionary<Direction, Vector2Int>(){
        { Direction.UL, new Vector2Int(-1,1) },
        { Direction.UR, new Vector2Int(0,1) },
        { Direction.L, new Vector2Int(-1,0) },
        { Direction.C, new Vector2Int(0,0) },
        { Direction.R, new Vector2Int(1,0) },
        { Direction.DL, new Vector2Int(0,-1) },
        { Direction.DR, new Vector2Int(1,-1) }
    };

    public GameObject hexPrefab;
    int width = 8, height = 5;
    float xOffset = 0.5f , yOffset = 0.85f;
    public GameObject[,] hexGrid;
    public 

    // Start is called before the first frame update
    void Start()
    {
        hexGrid = new GameObject[width, height];

        float xOffsetSum = 0;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                GameObject hex = Instantiate(hexPrefab, new Vector3(j+xOffsetSum,i*yOffset,0), Quaternion.identity,transform);
                hex.name = $"[{j},{i}]";
                hexGrid[j,i] = hex;
            }
            xOffsetSum += xOffset;
        }
        CutCorners();
        Debug.Log(hexGrid[1, 0].transform.position);
    }

    void CutCorners() {
        for (int i = 0; i < height / 2; i++) {
            for (int j = 0; j < height / 2 - i; j++) {
                //This if statement is a quick fix for index error when width is smaller than height/2 and it won't
                //work if tiles are not named exactly how they're named. Either fix later or don't rename tiles
                if(GameObject.Find($"[{i},{j}]") && GameObject.Find($"[{width - 1 - i},{height - 1 - j}]")) {
                    Destroy(hexGrid[i, j]);
                    Destroy(hexGrid[width - 1 - i, height - 1 - j]);
                }
            }
        }
    }

}
