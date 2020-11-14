using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void PlayGame()
    {
        ///// FOR TEST PURPOSES /////
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        /////////////////////

        SceneManager.LoadScene(1);
    }
    
}
