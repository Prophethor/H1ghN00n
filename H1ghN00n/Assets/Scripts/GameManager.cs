using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    GridGenerator gg;
    
    int width = 8, height = 5;
    int turns = 3;
    Vector2Int playerCoord, enemyCoord;
    Vector3 playerDest, enemyDest;
    [SerializeField] GameObject playerPrefab, enemyPrefab;
    [SerializeField] Text playerHpText, enemyHpText, nowText;
    GameObject player, enemy;
    private Vector3 playerOffset = new Vector3(0,-0.3f,-1);
    private float moveSpeed = 7;
    int playerHp = 3, enemyHp = 3;

    private void Start() {

        gg = GameObject.FindObjectOfType<GridGenerator>();
        gg.GenerateGrid(new Vector2Int(width, height));

        playerCoord = new Vector2Int(width/2-1, height/2);
        enemyCoord = new Vector2Int(width/2, height/2);
        player = Instantiate(playerPrefab, gg.GetWorldPos(playerCoord) + playerOffset, Quaternion.identity);
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
        enemy = Instantiate(enemyPrefab, gg.GetWorldPos(enemyCoord) + playerOffset, Quaternion.identity);
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
        playerDest = player.transform.position - playerOffset;
        enemyDest = enemy.transform.position - playerOffset;


        StartCoroutine(MovePhase());
    }

    private void Update() {

        playerHpText.text = playerHp.ToString();
        enemyHpText.text = enemyHp.ToString();

        if (Input.GetKeyDown(KeyCode.G)) {
            enemy.GetComponent<Animator>().SetTrigger("Turn");
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            enemy.GetComponent<Animator>().SetTrigger("Jump");
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            enemy.GetComponent<Animator>().SetBool("Aim", true);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            enemy.GetComponent<Animator>().SetTrigger("Shoot");
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            enemy.GetComponent<Animator>().SetTrigger("Die");
        }

    }

    IEnumerator MovePhase() {

        Dictionary<int, int> halfRowLen = new Dictionary<int, int>();
        for(int i = 0; i < height; i++) {
            halfRowLen.Add(i, i%2==0?gg.GetRowLen(i)/2:gg.GetRowLen(i)/2+1);
        }

        Dictionary<int, int> firstInRow = new Dictionary<int, int>();
        for (int i = 0; i < height; i++) {
            firstInRow.Add(i, gg.GetFirstInRow(i).x);
        }

        while (true) {
            yield return null;
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest + playerOffset, moveSpeed * Time.deltaTime);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest + playerOffset, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(player.transform.position, playerDest + playerOffset) < .05f) {
                if (turns<=0) {
                    yield return StartCoroutine(SteadyPhase());
                    break;
                } else {
                    if (Input.GetMouseButtonDown(0)) {
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);
                        if (hit.collider != null) {
                            int distP = gg.GetDistance(gg.hexGrid[playerCoord].GetTile(), hit.transform.gameObject);
                            Vector2Int tileCoord = gg.GetCoord(hit.transform.gameObject);
                            if(tileCoord.y<=height/2) tileCoord -= new Vector2Int(firstInRow[tileCoord.y],0);
                            if (distP == 1 && tileCoord.x < halfRowLen[tileCoord.y]) {

                                player.GetComponent<Animator>().SetTrigger("Jump");
                                enemy.GetComponent<Animator>().SetTrigger("Jump");
                                
                                Vector2Int coordDif = gg.GetCoord(hit.transform.gameObject) - playerCoord;

                                gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Empty);
                                playerCoord = gg.GetCoord(hit.transform.gameObject);
                                gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);

                                gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Empty);
                                enemyCoord = enemyCoord - coordDif;
                                gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);

                                playerDest = hit.transform.position;
                                enemyDest = gg.hexGrid[enemyCoord].GetTile().transform.position + gg.hexGrid[playerCoord].GetTile().transform.position - playerDest;

                                turns -= 1;
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator SteadyPhase() {

        player.GetComponent<Animator>().SetTrigger("Turn");
        enemy.GetComponent<Animator>().SetTrigger("Turn");
        yield return new WaitForSeconds(.5f);


        foreach (GridGenerator.Hex hex in gg.hexGrid.Values) {
            if (hex.GetState() == GridGenerator.HexState.Empty) {
                hex.GetTile().GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }

        float currentTime = 0;
        float maxTime = Random.Range(1.5f, 3.0f);
        bool tooSoon = false;
        while (true) {
            yield return null;
            currentTime += Time.deltaTime;
            if (currentTime > maxTime) break;
            if (Input.GetMouseButtonDown(0)) {
                tooSoon = true;
                playerHp -= 1;
                break;
            }
        }

        if (!tooSoon) yield return StartCoroutine(ShootPhase());

        List<Vector2Int> tiles = new List<Vector2Int>(gg.hexGrid.Keys);
        while (true) {           

            if (playerHp <= 0 && enemyHp <= 0) {
                Debug.Log("Draw");
                break;
            } else if (enemyHp <= 0) {
                Debug.Log("Player Won");
                break;
            } else if (playerHp <= 0) {
                Debug.Log("Player Lost");
                break;
            } else {
                StartCoroutine(Reset());
                break;
            }
        }
    }

    IEnumerator ShootPhase() {
        Time.timeScale = 0.2f;
        nowText.text = "NOW!";
        float currentTime = 0;
        bool enemyShot = false;
        float distanceIncrement = gg.GetDistance(gg.hexGrid[playerCoord].GetTile(), gg.hexGrid[enemyCoord].GetTile()) * 0.01f;
        while (true) {
            yield return null;
            currentTime += Time.unscaledDeltaTime;
            if(!enemyShot && currentTime > 0.15f + distanceIncrement) {
                enemyShot = true;
                playerHp -= 1;
            }
            if(Input.GetMouseButtonDown(0)) {
                enemyHp -= 1;
                break;
            }
            if(currentTime > 0.2f + distanceIncrement) {
                break;
            }
        }
        Time.timeScale = 1;
        nowText.text = "";
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator Reset() {
        player.GetComponent<Animator>().SetTrigger("Turn");
        enemy.GetComponent<Animator>().SetTrigger("Turn");

        foreach (GridGenerator.Hex hex in gg.hexGrid.Values) {
            if (hex.GetState() == GridGenerator.HexState.Active) {
                hex.SetState(GridGenerator.HexState.Empty);
            } else if (hex.GetState() == GridGenerator.HexState.Empty) {
                hex.GetTile().GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Empty);
        playerCoord = new Vector2Int(width / 2 - 1, height / 2);
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
        playerDest = gg.GetWorldPos(playerCoord);

        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Empty);
        enemyCoord = new Vector2Int(width / 2, height / 2);
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
        enemyDest = gg.GetWorldPos(enemyCoord);
        while (Vector3.Distance(player.transform.position, playerDest + playerOffset) > .05f) {
            yield return null;
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest + playerOffset, moveSpeed * Time.deltaTime);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest + playerOffset, moveSpeed * Time.deltaTime);
        }

        turns = 3;
        StartCoroutine(MovePhase());
    }

}


