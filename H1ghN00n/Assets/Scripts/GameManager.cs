using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Game objects set from editor
    [SerializeField] GameObject playerPrefab, enemyPrefab;
    [SerializeField] Text playerHpText, enemyHpText, nowText;

    //Game object that are automatically found or initialized from this script
    GridGenerator gg;
    Animator playerAnim, enemyAnim;
    GameObject player, enemy;

    //Difficulty parameters set from editor
    [SerializeField] int turns = 3, coverChanceReduction = 15, obstacleCount = 8;
    [SerializeField] float enemyReactionTime = 0.14f;

    //Parameters that await initialization
    Vector2Int playerCoord, enemyCoord;
    Vector3 playerDest, enemyDest;
    float distanceIncrement;

    //Parameters that don't need to be set from editor
    int gridWidth = 8, gridHeight = 5, playerHp = 3, enemyHp = 3;
    private Vector3 playerOffset = new Vector3(0,-0.3f,-1);
    private float moveSpeed = 7;

    private void Start() {

        //Generate grid
        gg = FindObjectOfType<GridGenerator>();
        gg.GenerateGrid(gridWidth, gridHeight);

        //Initialise player
        playerCoord = new Vector2Int(gridWidth / 2 - 1, gridHeight / 2);
        player = Instantiate(playerPrefab, gg.GetWorldPos(playerCoord) + playerOffset, Quaternion.identity); 
        playerAnim = player.GetComponent<Animator>();
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
        playerDest = player.transform.position - playerOffset;

        //Initialize enemy
        enemyCoord = new Vector2Int(gridWidth / 2, gridHeight / 2);
        enemy = Instantiate(enemyPrefab, gg.GetWorldPos(enemyCoord) + playerOffset, Quaternion.identity);
        enemyAnim = enemy.GetComponent<Animator>();
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
        enemyDest = enemy.transform.position - playerOffset;

        //Generate obstacles
        gg.GenerateObstacles(obstacleCount);

        //Start the game
        StartCoroutine(MovePhase());
    }

    

    private void Update() {
        playerHpText.text = playerHp.ToString();
        enemyHpText.text = enemyHp.ToString();
    }

    IEnumerator MovePhase() {

        //Game loop
        while (true) {
            yield return null;
            //Update enemy and player positions
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest + playerOffset, moveSpeed * Time.deltaTime);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest + playerOffset, moveSpeed * Time.deltaTime);
            //When they arrive to their destinations you can take another input
            if (Vector3.Distance(player.transform.position, playerDest + playerOffset) < .05f) {
                //Check if all moves are spent
                if (turns<=0) {
                    yield return StartCoroutine(SteadyPhase());
                    break;
                } else {
                    //Tap the tile to move
                    if (Input.GetMouseButtonDown(0)) {
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), -Vector2.up);
                        if (hit.collider != null) {
                            //Get tapped tile position in it's row and distance from player so we can calculate movement restrictions
                            int distP = gg.GetDistance(gg.hexGrid[playerCoord].GetTile(), hit.transform.gameObject);
                            Vector2Int rowCoord = gg.GetCoord(hit.transform.gameObject);
                            //Upper rows start from 0, bottom ones don't because we cut grid corners while generating it
                            if(rowCoord.y<=gridHeight/2) rowCoord -= new Vector2Int(gg.GetFirstInRow(rowCoord.y).x,0);
                            //Movement restrictions: If tile is 2 spaces from player, if player has enough moves and if tile is in left side of the grid
                            if (distP <=2 && turns-distP >=0 &&  rowCoord.x < gg.GetHalfRowLen(rowCoord.y) && gg.hexGrid[gg.GetCoord(hit.transform.gameObject)].GetState() == GridGenerator.HexState.Empty) {

                                player.GetComponent<Animator>().SetTrigger("Jump");
                                enemy.GetComponent<Animator>().SetTrigger("Jump");
                                
                                //Caclulate relative coordinates of tapped tile and player tile and move player
                                Vector2Int coordDif = gg.GetCoord(hit.transform.gameObject) - playerCoord;

                                gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Empty);
                                playerCoord = gg.GetCoord(hit.transform.gameObject);
                                gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);

                                //Check if enemy can move in opposite direction and if so move it
                                if (gg.hexGrid.ContainsKey(enemyCoord - coordDif) && gg.hexGrid[enemyCoord-coordDif].GetState() == GridGenerator.HexState.Empty) {
                                    gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Empty);
                                    enemyCoord = enemyCoord - coordDif;
                                    gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
                                    
                                }
                                    
                                //Update movement destinations and decrease number of moves player has left
                                playerDest = hit.transform.position;
                                enemyDest = gg.hexGrid[enemyCoord].GetTile().transform.position + gg.hexGrid[playerCoord].GetTile().transform.position - playerDest;
                                turns -= distP;
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
        yield return new WaitForSeconds(.55f);

        nowText.gameObject.SetActive(true);
        nowText.text = "Steady...";
        nowText.color = Color.white;

        //Grayout all empty tiles
        foreach (GridGenerator.Hex hex in gg.hexGrid.Values) {
            if (hex.GetState() == GridGenerator.HexState.Empty) {
                hex.GetTile().GetComponent<SpriteRenderer>().color = Color.gray;
            } 
        }

        //If player shoots before Shoot phase he automatically loses that round
        float currentTime = 0;
        float maxTime = Random.Range(2f, 4f);
        bool tooSoon = false;
        while (true) {
            yield return null;
            currentTime += Time.deltaTime;
            if (currentTime > maxTime) break;
            if (Input.GetMouseButtonDown(0)) {
                player.GetComponent<Animator>().SetTrigger("Aim");
                enemy.GetComponent<Animator>().SetTrigger("Aim");
                player.GetComponent<Animator>().SetTrigger("Hurt");
                enemy.GetComponent<Animator>().SetTrigger("Shoot");
                tooSoon = true;
                playerHp -= 1;
                if (playerHp <= 0) {
                    player.GetComponent<Animator>().SetBool("Die", true);
                }
                yield return new WaitForSeconds(.5f);
                nowText.gameObject.SetActive(false);
                break;
            }
        }

        //If player hasn't shot before Shoot phase initialize Shoot phase
        if (!tooSoon) yield return StartCoroutine(ShootPhase());

        //If player and enemy are alive, initialize another round
        if (playerHp > 0 && enemyHp > 0) {
            StartCoroutine(Reset());
            yield return new WaitForSeconds(.5f);
        }
    }

    IEnumerator ShootPhase() {

        player.GetComponent<Animator>().SetTrigger("Aim");
        enemy.GetComponent<Animator>().SetTrigger("Aim");

        nowText.color = Color.red;
        nowText.text = "Shoot!";

        float currentTime = 0;
        bool enemyHasShot = false;
        distanceIncrement = (gg.GetDistance(gg.hexGrid[playerCoord].GetTile(), gg.hexGrid[enemyCoord].GetTile()) - 1) / 2 * enemyReactionTime / 3;

        while (true) {
            yield return null;
            currentTime += Time.unscaledDeltaTime;
            //If player reacted in time frame try to deal damage to the enemy and proceed
            if (Input.GetMouseButtonDown(0)) {
                player.GetComponent<Animator>().SetTrigger("Shoot");
                int playerShot = Random.Range(0, 100);
                if (playerShot > CalculateCover(playerCoord)) {
                    enemy.GetComponent<Animator>().SetTrigger("Hurt");
                    enemyHp -= 1;
                    if (enemyHp <= 0) {
                        enemy.GetComponent<Animator>().SetBool("Die", true);
                    }
                } else {
                    RandomObstacleHurt(playerCoord);
                }
                break;
            }
            //If player hasn't reacted in time try to deal damage to the player but don't proceed
            if (!enemyHasShot && currentTime > enemyReactionTime + distanceIncrement) {
                enemy.GetComponent<Animator>().SetTrigger("Shoot");
                enemyHasShot = true;
                nowText.color = Color.blue;
                int enemyShot = Random.Range(0, 100), enemyChance = CalculateCover(enemyCoord);
                if( enemyShot > enemyChance) {
                    player.GetComponent<Animator>().SetTrigger("Hurt"); 
                    playerHp -= 1;
                    if (playerHp <= 0) {
                        player.GetComponent<Animator>().SetBool("Die", true);
                    }
                } else {
                    RandomObstacleHurt(enemyCoord);
                }
            }
            //If player hasn't reacted at all proceed
            if(currentTime > 4 * enemyReactionTime) {
                break;
            }
        }
        nowText.gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
    }

    

    IEnumerator Reset() {

        player.GetComponent<Animator>().SetTrigger("Turn");
        enemy.GetComponent<Animator>().SetTrigger("Turn");

        //Return all grayed out empty spaces to their normal state
        foreach (GridGenerator.Hex hex in gg.hexGrid.Values) {
            if (hex.GetState() == GridGenerator.HexState.Empty) {
                hex.GetTile().GetComponent<SpriteRenderer>().color = Color.white;
            } 
        }        

        //Move player back to the center of the grid
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Empty);
        playerCoord = new Vector2Int(gridWidth / 2 - 1, gridHeight / 2);
        gg.hexGrid[playerCoord].SetState(GridGenerator.HexState.Player);
        playerDest = gg.GetWorldPos(playerCoord);

        //Move enemy back to the center of the grid
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Empty);
        enemyCoord = new Vector2Int(gridWidth / 2, gridHeight / 2);
        gg.hexGrid[enemyCoord].SetState(GridGenerator.HexState.Enemy);
        enemyDest = gg.GetWorldPos(enemyCoord);

        //Wait for them to return
        while (Vector3.Distance(player.transform.position, playerDest + playerOffset) > .05f) {
            yield return null;
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerDest + playerOffset, moveSpeed * Time.deltaTime);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, enemyDest + playerOffset, moveSpeed * Time.deltaTime);
        }

        //Remove all obstacles and generate new ones
        gg.RemoveObstacles();
        gg.GenerateObstacles(obstacleCount);

        //Reset the game
        turns = 3;
        StartCoroutine(MovePhase());
    }

    //coord parameter is just used to decide if we called this for player or enemy
    private int CalculateCover(Vector2Int coord) {
        Vector2 playerPos = gg.GetWorldPos(playerCoord), enemyPos = gg.GetWorldPos(enemyCoord);
        //Cast 3 rays and count how many times they hit an obstacle that's at least 2 tiles away from coord tile
        RaycastHit2D[] hitsMiddle = Physics2D.RaycastAll(playerPos, enemyPos - playerPos, (enemyPos - playerPos).magnitude);
        RaycastHit2D[] hitsUpper = Physics2D.RaycastAll(playerPos + new Vector2(0, 0.2f), enemyPos - playerPos, (enemyPos - playerPos).magnitude);
        RaycastHit2D[] hitsBottom = Physics2D.RaycastAll(playerPos - new Vector2(0, 0.2f), enemyPos - playerPos, (enemyPos - playerPos).magnitude);
        int count = 0;
        foreach (RaycastHit2D hit in hitsUpper) {
            GridGenerator.Hex hitTile = gg.hexGrid[gg.GetCoord(hit.transform.gameObject)];
            if (hitTile.GetState() == GridGenerator.HexState.Obstacle && gg.GetDistance(hitTile.GetTile(), gg.hexGrid[coord].GetTile()) > 1) {
                count += 1;
            }
        }
        foreach (RaycastHit2D hit in hitsMiddle) {
            GridGenerator.Hex hitTile = gg.hexGrid[gg.GetCoord(hit.transform.gameObject)];
            if (hitTile.GetState() == GridGenerator.HexState.Obstacle && gg.GetDistance(hitTile.GetTile(), gg.hexGrid[coord].GetTile()) > 1) {
                count += 1;
            }
        }
        foreach (RaycastHit2D hit in hitsBottom) {
            GridGenerator.Hex hitTile = gg.hexGrid[gg.GetCoord(hit.transform.gameObject)];
            if (hitTile.GetState() == GridGenerator.HexState.Obstacle && gg.GetDistance(hitTile.GetTile(), gg.hexGrid[coord].GetTile()) > 1) {
                count += 1;
            }
        }
        //coverChanceReduction is manually set to manage difficulty
        return count * coverChanceReduction;
    }

    void RandomObstacleHurt(Vector2Int coord) {
        Vector2 playerPos = gg.GetWorldPos(playerCoord), enemyPos = gg.GetWorldPos(enemyCoord);
        RaycastHit2D[] hitsMiddle = Physics2D.RaycastAll(playerPos, enemyPos - playerPos, (enemyPos - playerPos).magnitude);
        RaycastHit2D[] hitsUpper = Physics2D.RaycastAll(playerPos + new Vector2(0, 0.2f), enemyPos - playerPos, (enemyPos - playerPos).magnitude);
        RaycastHit2D[] hitsBottom = Physics2D.RaycastAll(playerPos - new Vector2(0, 0.2f), enemyPos - playerPos, (enemyPos - playerPos).magnitude);

        List<GameObject> obstacles = new List<GameObject>();
        foreach (RaycastHit2D hit in hitsUpper) {
            GridGenerator.Hex hitTile = gg.hexGrid[gg.GetCoord(hit.transform.gameObject)];
            if (hitTile.GetState() == GridGenerator.HexState.Obstacle && gg.GetDistance(hitTile.GetTile(), gg.hexGrid[coord].GetTile()) > 1) {
                obstacles.Add(hitTile.GetObstacle());
            }
        }
        foreach (RaycastHit2D hit in hitsMiddle) {
            GridGenerator.Hex hitTile = gg.hexGrid[gg.GetCoord(hit.transform.gameObject)];
            if (hitTile.GetState() == GridGenerator.HexState.Obstacle && gg.GetDistance(hitTile.GetTile(), gg.hexGrid[coord].GetTile()) > 1) {
                obstacles.Add(hitTile.GetObstacle());
            }
        }
        foreach (RaycastHit2D hit in hitsBottom) {
            GridGenerator.Hex hitTile = gg.hexGrid[gg.GetCoord(hit.transform.gameObject)];
            if (hitTile.GetState() == GridGenerator.HexState.Obstacle && gg.GetDistance(hitTile.GetTile(), gg.hexGrid[coord].GetTile()) > 1) {
                obstacles.Add(hitTile.GetObstacle());
            }
        }
        //Initiate animation for a random obstacle which will result in destroying it
        obstacles[Random.Range(0, obstacles.Count)].GetComponent<Animator>().SetFloat("Speed", 1);
    }


}


