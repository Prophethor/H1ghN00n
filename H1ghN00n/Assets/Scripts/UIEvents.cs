using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEvents : MonoBehaviour
{

    [SerializeField] GameObject MenuPanel, TutorialPanel,BackButton, Map, GameUI, YouWonPanel, YouLostPanel;

    private void Show(GameObject go) {
        go.GetComponent<CanvasGroup>().alpha = 1;
        go.GetComponent<CanvasGroup>().interactable = true;
        go.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    private void Hide(GameObject go) {
        go.GetComponent<CanvasGroup>().alpha = 0;
        go.GetComponent<CanvasGroup>().interactable = false;
        go.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void StartGame() {
        Show(GameUI);
        Hide(MenuPanel);
        FindObjectOfType<GameManager>().GetComponent<GameManager>().StartGame();
    }

    public void TutButtonPress() {
        Hide(MenuPanel);
        Show(TutorialPanel);        
    }

    public void BackButtonPress() {
        Hide(TutorialPanel);
        Hide(YouWonPanel);
        Hide(YouLostPanel);
        Show(MenuPanel);
    }

    public void YouWon() {
        Hide(GameUI);
        Show(YouWonPanel);
    }

    public void YouLost() {
        Hide(GameUI);
        Show(YouLostPanel);
    }
}
