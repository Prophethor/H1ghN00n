using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    GridGenerator gg;
    
    int width = 8, height = 5;
    int turns = 3;
    float minigameTime = 0;
    float currentTime = 0;
    Vector2Int playerCoord, enemyCoord;
    Vector3 playerDest, enemyDest;
    public GameObject playerPrefab, enemyPrefab;
    public Text playerChanceText, playerHpText, enemyChanceText, enemyHpText, timerText;
    GameObject player, enemy;
    private Vector3 playerOffset = new Vector3(0,0.3f,-1);
    private float moveSpeed = 8;
    int playerChance = 50, enemyChance = 50, playerHp = 3, enemyHp = 3;

    private void Start() {

        gg = GameObject.FindObjectOfType<GridGenerator>();
        gg.GenerateGrid(new Vector2Int(width, height));

        playerCoord = new Vector2Int(3, 2);
        enemyCoord = new Vector2Int(4, 2);
        player = Instantiate(playerPrefab, gg.GetWorldPos(playerCoord) + playerOffset, Quaternion.identity);
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
        enemy = Instantiate(enemyPrefab, gg.GetWorldPos(enemyCoord) + playerOffset, Quaternion.identity);
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
        playerDest = player.transform.position - playerOffset;
        enemyDest = enemy.transform.position - playerOffset;

        StartCoroutine(Move());
    }

    private void Update() {

        playerChanceText.text = playerChance.ToString();
        playerHpText.text = playerHp.ToString();
        enemyChanceText.text = enemyChance.ToString();
        enemyHpText.text = enemyHp.ToString();
        timerText.text = (Mathf.Min((int)(10 - currentTime)+1,10)).ToString();
        
    }

    IEnumerator Move() {
        Debug.Log("You have 10 seconds!");
        while (true) {
            yield return null;
            currentTime += Time.deltaTime;
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest + playerOffset, moveSpeed * Time.deltaTime);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest + playerOffset, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(player.transform.position, playerDest + playerOffset) < .05f) {
                if (currentTime > 10f) {
                    Debug.Log("Time run out!");
                    yield return StartCoroutine(Shoot());
                    break;
                } else if (Input.GetKeyDown(KeyCode.Space)) {
                    yield return StartCoroutine(Shoot());
                    break;
                } else {
                    if (Input.GetMouseButtonDown(0)) {
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);
                        if (hit.collider != null) {
                            int dist = gg.GetDistance(gg.hexGrid[playerCoord].GetTile(), hit.transform.gameObject);
                            if(dist <= 2) {
                                playerDest = hit.transform.position;
                                Vector2Int coordDif = gg.GetCoord(hit.transform.gameObject) - playerCoord;
                                gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Empty);
                                playerCoord = gg.GetCoord(hit.transform.gameObject);
                                gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
                                gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Empty);
                                enemyCoord = enemyCoord - coordDif;
                                gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
                                enemyDest = gg.hexGrid[enemyCoord].GetTile().transform.position + gg.hexGrid[playerCoord].GetTile().transform.position - playerDest;
                                turns -= dist;
                                if (turns < 0) playerChance = Mathf.Max(playerChance - 10 * dist, 0);
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator Shoot() {
        Debug.Log("Shoot!");
        foreach(GridGenerator.Hex hex in gg.hexGrid.Values) {
            if (hex.GetState() == GridGenerator.HexState.Empty) {
                hex.GetTile().GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }
        
        List<Vector2Int> tiles = new List<Vector2Int>(gg.hexGrid.Keys);
        while (true) {

            yield return StartCoroutine(Minigame());

            int playerShot = Random.Range(0, 100);
            if (playerShot < playerChance) {
                Debug.Log("Player shot the enemy");
                enemyHp -= 1;
            } else Debug.Log("Player missed");
            int enemyShot = Random.Range(0, 100);
            if (enemyShot < enemyChance) {
                Debug.Log("Enemy shot the player");
                playerHp -= 1;
            } else Debug.Log("Enemy missed");

            if (playerHp <= 0) {
                Debug.Log("Player lost!");
                break;
            } else if (enemyHp <= 0) {
                Debug.Log("Player won!");
                break;
            } else {
                Debug.Log("New round!");
                StartCoroutine(Reset());
                break;
            }
        }
    }

    IEnumerator Minigame() {
        minigameTime = 0;
        while (minigameTime < 3) {
            yield return StartCoroutine(TapTile());
        }
    }

    IEnumerator TapTile() {
        List<Vector2Int> tiles = new List<Vector2Int>(gg.hexGrid.Keys);
        GridGenerator.Hex activeTile = gg.hexGrid[tiles[Random.Range(0, tiles.Count)]];
        while(activeTile.GetState()!=GridGenerator.HexState.Empty) activeTile = gg.hexGrid[tiles[Random.Range(0, tiles.Count)]];
        activeTile.GetTile().GetComponent<SpriteRenderer>().color = Color.white;
        activeTile.SetState(GridGenerator.HexState.Active);
        while (true) {

            yield return null;
            minigameTime += Time.deltaTime;
            if (minigameTime > 3) break;

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);
                if (hit.collider != null && hit.transform.gameObject == activeTile.GetTile()) {
                    activeTile.GetTile().GetComponent<SpriteRenderer>().color = Color.grey;
                    activeTile.SetState(GridGenerator.HexState.Empty);
                    activeTile = gg.hexGrid[tiles[Random.Range(0, tiles.Count)]];
                    while (activeTile.GetState() != GridGenerator.HexState.Empty) activeTile = gg.hexGrid[tiles[Random.Range(0, tiles.Count)]];
                    activeTile.GetTile().GetComponent<SpriteRenderer>().color = Color.white;
                    activeTile.SetState(GridGenerator.HexState.Active);
                    playerChance += Random.Range(3,7);
                }
            }
        }
    }

    IEnumerator Reset() {

        foreach (GridGenerator.Hex hex in gg.hexGrid.Values) {
            if (hex.GetState() == GridGenerator.HexState.Active) {
                hex.SetState(GridGenerator.HexState.Empty);
            } else if (hex.GetState() == GridGenerator.HexState.Empty) {
                hex.GetTile().GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Empty);
        playerCoord = new Vector2Int(3, 2);
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
        playerDest = gg.GetWorldPos(playerCoord);

        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Empty);
        enemyCoord = new Vector2Int(4, 2);
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
        enemyDest = gg.GetWorldPos(enemyCoord);
        while (Vector3.Distance(player.transform.position, playerDest + playerOffset) > .05f) {
            yield return null;
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest + playerOffset, moveSpeed * Time.deltaTime);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest + playerOffset, moveSpeed * Time.deltaTime);
        }

        currentTime = 0;
        playerChance = 50;
        enemyChance = 50;
        turns = 3;
        StartCoroutine(Move());
    }

}


