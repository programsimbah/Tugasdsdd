using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectResetter : MonoBehaviour
{

    private PlayerController playerControl;
    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        if (layer == LayerMask.NameToLayer("CharacterSelectionUI"))
        {
            return;
        }

        if (other.transform.parent != null)
        {
            GameObject parent = other.transform.parent.gameObject;
            if (parent.CompareTag("Path"))
            {
                SgLib.ScoreManager.Instance.AddScore(1);
                GameManager.Instance.CreatePath(false);
            }
            ResetObject(parent);
        }
        else
        {
            ResetObject(other.gameObject);
        }
    }

    void ResetObject(GameObject ob)
    {
        for (int i = 0; i < ob.transform.childCount; i++)
        {
            ob.transform.GetChild(i).gameObject.SetActive(true);
        }
        ob.transform.eulerAngles = Vector3.zero;
        ob.transform.position = Vector3.zero;
        ob.transform.SetParent(GameManager.Instance.transform);
        ob.SetActive(false);
    }
}
