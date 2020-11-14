using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is just for keeping track of the remaining moves and the bomb's text
public class Bomb : MonoBehaviour
{
    public Renderer textRenderer;
    public TextMesh textMesh;

    public int defRemainingMoves = 6;

    int remamingMoves;

    void Start()
    {
        textRenderer.sortingOrder = 20; // to show the text in front of the bomb

        remamingMoves = PlayerPrefs.GetInt("RemainingMoves", defRemainingMoves) + 1;
        textMesh.text = remamingMoves.ToString();
    }

    // decrement the counter in front of the bomb and check whether game is over
    public void CheckGameOver()
    {
        remamingMoves--;
        textMesh.text = remamingMoves.ToString();
        PlayerPrefs.SetInt("RemainingMoves", remamingMoves);
        PlayerPrefs.Save();
        if (remamingMoves <= 0)
        {
            Destroy(gameObject, 0.25f);
            FindObjectOfType<GameManager>().GameOver();
        }
    }

    // used when a bomb is destroyed by matching
    public void ResetRemainingCounter()
    {
        PlayerPrefs.SetInt("RemainingMoves", defRemainingMoves);
        PlayerPrefs.Save();
    }
    
}
