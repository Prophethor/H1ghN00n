using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents: MonoBehaviour
{
    public void TurnAround() {
        GetComponent<SpriteRenderer>().flipX=!GetComponent<SpriteRenderer>().flipX;
        GetComponent<Animator>().SetBool("Aim", false);
    }
}
