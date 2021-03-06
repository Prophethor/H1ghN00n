using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] GameObject hexPrefab;
    public enum HexState { Empty, Player, Enemy, Cover, Pickup, Active }
    public class Hex {
        public Hex(GameObject tile, HexState state) {
            this.tile = tile;
            this.state = state;
        }
        public void SetTile(GameObject tile) {
            this.tile = tile;
        }
        public void SetState(HexState state) {
            this.state = state;
        }
        public HexState GetState() {
            return state;
        }
        public GameObject GetTile() {
            return tile;
        }
        private GameObject tile;
        private HexState state;
    }

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
    //Dependant of hex prefab in world space
    float xOffset = 0.5f , yOffset = 0.85f;
    public Dictionary<Vector2Int,Hex> hexGrid; 

    public void GenerateGrid(Vector2Int dim) {

        hexGrid = new Dictionary<Vector2Int,Hex>();

        float xOffsetSum = 0;
        for (int i = 0; i < dim.y; i++) {
            for (int j = 0; j < dim.x; j++) {
                GameObject hex = Instantiate(hexPrefab, new Vector3(j + xOffsetSum, i * yOffset, 1), Quaternion.identity, transform);
                hex.name = $"[{j},{i}]";
                hexGrid[new Vector2Int(j, i)] = new Hex(hex,HexState.Empty);
            }
            xOffsetSum += xOffset;
        }
        //Cut corners
        for (int i = 0; i < dim.y / 2; i++) {
            for (int j = 0; j < dim.y / 2 - i; j++) {
                //This if statement is a quick fix for index error when width is smaller than height/2 and it won't
                //work if tiles are not named exactly how they're named. Either fix later or don't rename tiles
                if (GameObject.Find($"[{i},{j}]") && GameObject.Find($"[{dim.x - 1 - i},{dim.y - 1 - j}]")) {
                    GameObject tmp = hexGrid[new Vector2Int(i, j)].GetTile();
                    hexGrid.Remove(GetCoord(tmp));
                    Destroy(tmp);
                    tmp = hexGrid[new Vector2Int(dim.x - 1 - i, dim.y - 1 - j)].GetTile();
                    hexGrid.Remove(GetCoord(tmp));
                    Destroy(tmp);
                }
            }
        }
    }

    public Vector3 GetWorldPos(Vector2Int coord) {
        return hexGrid[coord].GetTile().transform.position;
    }

    public Vector2Int GetCoord(GameObject hex) {
        //There will be only 1 value for each key so this is fine
        foreach (KeyValuePair<Vector2Int,Hex> myHex in hexGrid) {
            if (myHex.Value.GetTile() == hex) return myHex.Key;
        }
        return new Vector2Int(-1,-1);
    }

    public int GetDistance(GameObject hex1, GameObject hex2) {
        Vector2Int coord1 = GetCoord(hex1), coord2 = GetCoord(hex2);
        int count = 0;
        while(true) {
            if(coord1.y==coord2.y) { 
                if(coord1.x==coord2.x) {
                    return count;
                } else if (coord1.x<coord2.x) {
                    coord1 += HexDir[Direction.R];
                } else {
                    coord1 += HexDir[Direction.L];
                }
            } else if (coord1.y<coord2.y) {
                if (coord1.x == coord2.x) {
                    coord1 += HexDir[Direction.UR];
                } else if (coord1.x < coord2.x) {
                    coord1 += HexDir[Direction.R];
                } else {
                    coord1 += HexDir[Direction.UL];
                }
            } else {
                if (coord1.x == coord2.x) {
                    coord1 += HexDir[Direction.DL];
                } else if (coord1.x < coord2.x) {
                    coord1 += HexDir[Direction.DR];
                } else {
                    coord1 += HexDir[Direction.L];
                }
            }
            count += 1;
        }
    }
}
