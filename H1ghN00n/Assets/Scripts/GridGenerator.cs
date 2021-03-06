using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] GameObject hexPrefab;
    //Dependant of hex prefab in world space
    float xOffset = 0.5f , yOffset = 0.85f;
    public Dictionary<Vector2Int,GameObject> hexGrid; 

    public void GenerateGrid(Vector2Int dim) {

        hexGrid = new Dictionary<Vector2Int,GameObject>();

        float xOffsetSum = 0;
        for (int i = 0; i < dim.y; i++) {
            for (int j = 0; j < dim.x; j++) {
                GameObject hex = Instantiate(hexPrefab, new Vector3(j + xOffsetSum, i * yOffset, 1), Quaternion.identity, transform);
                hex.name = $"[{j},{i}]";
                hexGrid[new Vector2Int(j,i)] = hex;
            }
            xOffsetSum += xOffset;
        }
        //Cut corners
        for (int i = 0; i < dim.y / 2; i++) {
            for (int j = 0; j < dim.y / 2 - i; j++) {
                //This if statement is a quick fix for index error when width is smaller than height/2 and it won't
                //work if tiles are not named exactly how they're named. Either fix later or don't rename tiles
                if (GameObject.Find($"[{i},{j}]") && GameObject.Find($"[{dim.x - 1 - i},{dim.y - 1 - j}]")) {
                    GameObject tmp = hexGrid[new Vector2Int(i, j)];
                    hexGrid.Remove(GetCoord(tmp));
                    Destroy(tmp);
                    tmp = hexGrid[new Vector2Int(dim.x - 1 - i, dim.y - 1 - j)];
                    hexGrid.Remove(GetCoord(tmp));
                    Destroy(tmp);
                }
            }
        }
    }

    public Vector3 GetWorldPos(Vector2Int coord) {
        return hexGrid[coord].transform.position;
    }

    public Vector2Int GetCoord(GameObject hex) {
        //There will be only 1 value for each key so this is fine
        foreach (KeyValuePair<Vector2Int,GameObject> myHex in hexGrid) {
            if (myHex.Value == hex) return myHex.Key;
        }
        return new Vector2Int(-1,-1);
    }
}
