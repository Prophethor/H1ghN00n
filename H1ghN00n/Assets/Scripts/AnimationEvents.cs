using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents: MonoBehaviour
{
    GridGenerator gg;

    private void Start() {
        gg = FindObjectOfType<GridGenerator>();
    }

    public void TurnAround() {
        GetComponent<SpriteRenderer>().flipX=!GetComponent<SpriteRenderer>().flipX;
        GetComponent<Animator>().SetBool("Aim", false);
    }

    public void DestroySelf() {
        Destroy(gameObject);
        gg.hexGrid[gg.GetCoord(transform.parent.gameObject)].SetState(GridGenerator.HexState.Empty);
    }
}
