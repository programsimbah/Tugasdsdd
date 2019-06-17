using UnityEngine;
using System.Collections;
using SgLib;

public class ItemController : MonoBehaviour
{
    public ItemType itemType;

    private PlayerController playerControl;
    // Use this for initialization
    void OnEnable()
    {
        if (playerControl == null)
            playerControl = FindObjectOfType<PlayerController>();
        StartCoroutine(Rotate());
        if (itemType == ItemType.COIN)
            StartCoroutine(WaitForMagnetMode());
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.item);
            GameManager.Instance.ItemExplore(other.transform.position);
            if (itemType == ItemType.COIN)
                CoinManager.Instance.AddCoins(1);
            gameObject.SetActive(false);
        }
    }
    IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(Vector3.up * 5f);
            yield return null;
        }
    }

    IEnumerator WaitForMagnetMode()
    {
        while (GameManager.Instance.GameState == GameState.Playing)
        {
            if (playerControl.inMagnetMode)
            {
                float distance = Mathf.Abs(transform.position.x - playerControl.transform.position.x);
                if (distance <= GameManager.Instance.magnetModeRadius)
                {
                    StartCoroutine(MoveToPlayer());
                    yield break;
                }
            }
            yield return null;
        }
    }

    IEnumerator MoveToPlayer()
    {
        while (GameManager.Instance.GameState == GameState.Playing)
        {
            Vector3 dir = Vector3.Normalize(playerControl.transform.position - transform.position);
            transform.position += dir * playerControl.speed * 2 * Time.deltaTime;
            yield return null;
        }
    }

}
