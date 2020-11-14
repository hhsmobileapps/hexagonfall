using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Text scoreText, numOfmovesText;
    public int scorePerHexagon = 5;
    public int defaultBombScore = 1000;
    public GameObject gameOverPanel;
    public AudioClip gameOverSfx;

    int currentScore, numOfMoves, bombScoreChecker;    

    void Start()
    {
        currentScore = 0;
        scoreText.text = currentScore.ToString();
        numOfMoves = 0;
        numOfmovesText.text = numOfMoves.ToString();
        bombScoreChecker = PlayerPrefs.GetInt("BombScoreChecker", defaultBombScore);
        gameOverPanel.SetActive(false);
    }
    
    // This methods increments the score and updates the UI text
    public void IncrementScore(int numOfMatches)
    {
        currentScore += scorePerHexagon * numOfMatches;
        scoreText.text = currentScore.ToString();
    }

    // Checks whether it is time for adding a bomb to the scene
    public bool IsBombTime()
    {
        if (currentScore >= bombScoreChecker)
        {
            bombScoreChecker = PlayerPrefs.GetInt("BombScoreChecker", defaultBombScore) + defaultBombScore;
            PlayerPrefs.SetInt("BombScoreChecker", bombScoreChecker);
            PlayerPrefs.Save();
            return true;
        }
        else
        {
            return false;
        }
    }

    // increments the number of moves that user made so far
    public void IncrementMoves()
    {
        numOfMoves++;
        numOfmovesText.text = numOfMoves.ToString();
    }
    
    // when game is over, show game over panel and reset all values (playerprefs)
    public void GameOver()
    {
        StartCoroutine(gameOverAsyc());
    }

    IEnumerator gameOverAsyc()
    {
        yield return new WaitForSeconds(.5f);
        SoundManager.Instance.PlaySFX(gameOverSfx);
        gameOverPanel.SetActive(true);
        PlayerPrefs.DeleteAll();
    }

    // button click for restart
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    
}
