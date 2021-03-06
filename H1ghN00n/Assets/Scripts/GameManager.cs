using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GridGenerator gg;
    public enum Direction { UL, UR, L, C, R, DL, DR };
    public static Dictionary<Direction, Vector2Int> HexDir = new Dictionary<Direction, Vector2Int>(){
        { Direction.UL, new Vector2Int(-1,1) },
        { Direction.UR, new Vector2Int(0,1) },
        { Direction.L, new Vector2Int(-1,0) },
        { Direction.C, new Vector2Int(0,0) },
        { Direction.R, new Vector2Int(1,0) },
        { Direction.DL, new Vector2Int(0,-1) },
        { Direction.DR, new Vector2Int(1,-1) }
    };

    Vector2Int playerPosCoord, enemyPosCoord;
    Vector3 playerPos, enemyPos, playerDest, enemyDest;
    public GameObject playerPrefab, enemyPrefab;
    GameObject player, enemy;
    private Vector3 playerOffset = new Vector3(0,0.3f,-1);
    private float moveSpeed = 5;

    private void Start() {

        gg = GameObject.FindObjectOfType<GridGenerator>();
        gg.GenerateGrid(new Vector2Int(8,5));

        playerPosCoord = new Vector2Int(3, 2);
        playerPos = gg.GetWorldPos(playerPosCoord);
        enemyPosCoord = new Vector2Int(4, 2);
        enemyPos = gg.GetWorldPos(enemyPosCoord);
        player = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        playerDest = player.transform.position;
        enemyDest = enemy.transform.position;
    }


    private void Update() {

        player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest+playerOffset, moveSpeed * Time.deltaTime);
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest+playerOffset, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(player.transform.position, playerDest + playerOffset) < .05f ||
            Vector3.Distance(enemy.transform.position, enemyDest + playerOffset) < .05f) {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);
                if (hit.collider != null) {
                    playerDest = hit.transform.position;
                    enemyDest = gg.hexGrid[enemyPosCoord].transform.position + gg.hexGrid[playerPosCoord].transform.position - playerDest;
                }
            }
        }
        

    }

}
