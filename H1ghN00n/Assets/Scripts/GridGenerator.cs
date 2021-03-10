using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{

    //Class specific types and enums
    public enum HexState { Empty, Player, Enemy, Obstacle }
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
        public void SetObstacle(GameObject obstacle) {
            this.obstacle = obstacle;
        }
        public HexState GetState() {
            return state;
        }
        public GameObject GetTile() {
            return tile;
        }
        public GameObject GetObstacle() {
            return obstacle;
        }

        private GameObject tile;
        private HexState state;
        private GameObject obstacle;
    }

    //Game objects and parameters set up from editor
    [SerializeField] GameObject hexPrefab, obstaclePrefab;
    [SerializeField] float xOffset = 0.465f, yOffset = 0.83f;

    //Public parameters used by game manager
    public Dictionary<Vector2Int,Hex> hexGrid;
    public int width, height;

    //Acts as a constructor initializing grid, cutting it's corner so it's hexagon shaped and storing info about tiles in a dictionary
    public void GenerateGrid(int width, int height) {

        hexGrid = new Dictionary<Vector2Int,Hex>();
        this.width = width;
        this.height = height;

        //X offset of instantiated tiles accumulates while y offset is always the same 
        float xOffsetSum = 0;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                GameObject hex = Instantiate(hexPrefab, new Vector3(j*2*xOffset + xOffsetSum, i * yOffset, 1), Quaternion.identity, transform);
                hex.name = $"[{j},{i}]";
                hexGrid[new Vector2Int(j, i)] = new Hex(hex,HexState.Empty);
            }
            xOffsetSum += xOffset;
        }
        //Cut corners
        for (int i = 0; i < height / 2; i++) {
            for (int j = 0; j < height / 2 - i; j++) {
                //This if statement is a quick fix for index error when width is smaller than height/2 and it won't
                //work if tiles are not named exactly how they're named. Either fix later or don't rename tiles
                if (GameObject.Find($"[{i},{j}]") && GameObject.Find($"[{width - 1 - i},{height - 1 - j}]")) {
                    GameObject tmp = hexGrid[new Vector2Int(i, j)].GetTile();
                    hexGrid.Remove(GetCoord(tmp));
                    Destroy(tmp);
                    tmp = hexGrid[new Vector2Int(width - 1 - i, height - 1 - j)].GetTile();
                    hexGrid.Remove(GetCoord(tmp));
                    Destroy(tmp);
                }
            }
        }

    }

    //Place obstacles in random empty places
    public void GenerateObstacles(int count) {
        List<Vector2Int> tiles = new List<Vector2Int>();
        foreach (Vector2Int key in hexGrid.Keys) if (hexGrid[key].GetState() == HexState.Empty) tiles.Add(key);
        for(int i=0; i<count; i++) {
            int randomTile = Random.Range(0, tiles.Count);
            GameObject obstacle = Instantiate(obstaclePrefab, GetWorldPos(tiles[randomTile]) + new Vector3(0,-.3f,-.5f), Quaternion.identity, hexGrid[tiles[randomTile]].GetTile().transform);
            //Trigger 0 is for cactus, 1 is for crate, could also be done using SetInt or SetBool, but this is easier
            obstacle.GetComponent<Animator>().SetTrigger(Random.Range(0, 2).ToString());
            obstacle.GetComponent<Animator>().SetFloat("Speed", 0);
            hexGrid[tiles[randomTile]].SetState(HexState.Obstacle);
            hexGrid[tiles[randomTile]].SetObstacle(obstacle);
            tiles.RemoveAt(randomTile);

        }
    }

    //For each obstacle initialise animation which will result in destroying it
    public void RemoveObstacles() {
        foreach (Hex tile in hexGrid.Values) {
            if (tile.GetState() == HexState.Obstacle) {
                tile.GetObstacle().GetComponent<Animator>().SetFloat("Speed", 1);
            }
        }
    }

    public Vector3 GetWorldPos(Vector2Int coord) {
        return hexGrid[coord].GetTile().transform.position;
    }

    public Vector2Int GetCoord(GameObject hex) {
        //There will be only 1 value for each key so this is fine, kind of inefficient though, but has to be done like this because of raycasting
        foreach (KeyValuePair<Vector2Int,Hex> myHex in hexGrid) {
            if (myHex.Value.GetTile() == hex) return myHex.Key;
        }
        //I forgot about this and it caused me many problems :)
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

    public int GetHalfRowLen(int y) {
        int count = 0;
        foreach(Vector2 c in hexGrid.Keys) {
            if (c.y == y) count++;
        }
        if (y % 2 == 0) return count / 2;
        else return count / 2 + 1;
    }

    public Vector2Int GetFirstInRow(int y) {
        return new Vector2Int(Mathf.Abs(height/2-y),y);
    }
}
