using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {

    private Material bgMat;

    void Start() {
        bgMat = GetComponent<MeshRenderer>().material;
    }

    void Update() {
        bgMat.mainTextureOffset += Vector2.one * 0.01f * Time.deltaTime;
    }
}
