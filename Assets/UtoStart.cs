using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtoStart : MonoBehaviour
{
    public GameManager uima;
    private void Start()
    {
        uima = GameManager.Instance;
        StartCoroutine(Stars());
    }

    IEnumerator Stars()
    {
        yield return new WaitForSeconds(3f);
        GameManager.Instance.StartGame();
        }
    private void Update()
    {
        if (uima.GUGU)
        {
            uima.GUGU = false;
            Restart();
        }
    }

    void Restart()
    {
        GameManager.Instance.RestartGame(0.2f);
    }
}
