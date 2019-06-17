using System.Collections;
using UnityEngine;

public class ObstacleController : MonoBehaviour {

    public ObstacleType obstacleType;

    private GameObject player;
    private Vector3 size = Vector3.zero;
    private float checkingDistance;
    private float handlingTime;
    private void OnEnable()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>().gameObject;
        if (size == Vector3.zero)
        {
            Renderer rdr = GetComponent<Renderer>();
            if (rdr != null)
                size = rdr.bounds.size;
        }

        checkingDistance = Random.Range(GameManager.Instance.minCheckingDistance, GameManager.Instance.maxCheckingDistance);
        handlingTime = Random.Range(GameManager.Instance.minObstacleHandlingTime, GameManager.Instance.maxObstacleHandlingTime);

        StartCoroutine(CheckAndDoFunction());
    }
   
    IEnumerator CheckAndDoFunction()
    {
        yield return null; //Wait till next frame then check the distance
        while (true)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= checkingDistance)
            {
                if (obstacleType == ObstacleType.BOXES)
                    DoHide();
                else if (obstacleType == ObstacleType.ROLLING_ROCK)
                    DoRoll();
                else if (obstacleType == ObstacleType.LEFT_BARRIER)
                    DoRotateRight();
                else if (obstacleType == ObstacleType.RIGHT_BARRIER)
                    DoRotateLeft();
                else if (obstacleType == ObstacleType.PYRAMID)
                    DoGrow();
                else if (obstacleType == ObstacleType.ROTATING_WALL)
                    DoRotating();
                else if (obstacleType == ObstacleType.SPINDLE)
                    DoSpin();
                else if (obstacleType == ObstacleType.HINGE)
                    DoRotate(handlingTime);
                else if (obstacleType == ObstacleType.BOMB)
                    DoBounce();
                else if (obstacleType == ObstacleType.ROLLING_BALL)
                    RollTheBall();
                yield break;
            }
            yield return null;
        }
    }

    #region Boxes
    void DoHide()
    {
        if (Random.value <= 0.7f) //Hide 2 box
        {
            //Renderer
           // Renderer box_1 = transform.GetChild(Random.Range(0, transform.childCount)).GetComponent<Renderer>();
           // Renderer box_2 = transform.GetChild(Random.Range(0, transform.childCount)).GetComponent<Renderer>();
            Transform box_1 = transform.GetChild(Random.Range(0, transform.childCount));
            Transform box_2 = transform.GetChild(Random.Range(0, transform.childCount));
            while (box_2 == box_1)
            {
                //box_2 = transform.GetChild(Random.Range(0, transform.childCount)).GetComponent<Renderer>();
                box_2 = transform.GetChild(Random.Range(0, transform.childCount));
            }
            StartCoroutine(RollOut(box_1));
            StartCoroutine(RollOut(box_2));

            //StartCoroutine(FadingOutBox(box_1));
            //StartCoroutine(FadingOutBox(box_2));
        }
        else //Hide one box
        {
            //Renderer box = transform.GetChild(Random.Range(0, transform.childCount)).GetComponent<Renderer>();
            Transform box = transform.GetChild(Random.Range(0, transform.childCount));

           // StartCoroutine(FadingOutBox(box));
            StartCoroutine(RollOut(box));
        }
    }
    IEnumerator RollOut(Transform box)
    {
      
        float time = 1f;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            box.transform.Translate(-Vector3.up *6* Time.deltaTime);     
            yield return null;
        }       
    }

    IEnumerator FadingOutBox(Renderer box)
    {
        MeshCollider collider = box.GetComponent<MeshCollider>();
        collider.enabled = false;
        box.material.shader = Shader.Find("Transparent/Diffuse");
        box.material.SetColor("_Color", Color.white);
        Color startColor = box.material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        float time = 1f;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            box.material.SetColor("_Color", Color.Lerp(startColor, endColor, factor));
            yield return null;
        }
        box.gameObject.SetActive(false);
        collider.enabled = true;
        box.sharedMaterial = GameManager.Instance.gamePack;
    }

    #endregion
    #region Rolling Rock
    void DoRoll()
    {
        if (Mathf.Round(transform.position.z) == -6f) //Roll left
        {
            Vector3 rollingPoint = transform.position + Vector3.down * size.y / 2f +
                                                        Vector3.forward * size.z / 2f;
            StartCoroutine(Rotating(rollingPoint, Vector3.right, 90f));
        }
        else if (Mathf.Round(transform.position.z) == 6f) //Roll right
        {
            Vector3 rollingPoint = transform.position + Vector3.down * size.y / 2f +
                                                        Vector3.back * size.z / 2f;
            StartCoroutine(Rotating(rollingPoint, Vector3.left, 90f));
        }
        else
        {
            float distance = Mathf.Round(player.transform.position.z - transform.position.z);
            if (distance < 0)
            {
                Vector3 rollingPoint = transform.position + Vector3.down * size.y / 2f +
                                                        Vector3.back * size.z / 2f;
                StartCoroutine(Rotating(rollingPoint, Vector3.left, 90f));
            }
            else if (distance > 0)
            {
                Vector3 rollingPoint = transform.position + Vector3.down * size.y / 2f +
                                                       Vector3.forward * size.z / 2f;
                StartCoroutine(Rotating(rollingPoint, Vector3.right, 90f));
            }
            else
            {
                if (Random.value <= 0.5) //Roll left
                {
                    Vector3 rollingPoint = transform.position + Vector3.down * size.y / 2f +
                                                           Vector3.forward * size.z / 2f;
                    StartCoroutine(Rotating(rollingPoint, Vector3.right, 90f));
                }
                else //Roll right
                {
                    Vector3 rollingPoint = transform.position + Vector3.down * size.y / 2f +
                                                           Vector3.back * size.z / 2f;
                    StartCoroutine(Rotating(rollingPoint, Vector3.left, 90f));
                }
            }

        }
    }   
    #endregion
    #region Left Barrier
    void DoRotateRight()
    {
        StartCoroutine(Rotating(transform.position, Vector3.left, 90f));
    }
    #endregion
    #region Right Barrier
    void DoRotateLeft()
    {
        StartCoroutine(Rotating(transform.position, Vector3.right, 90f));
    }
    #endregion
    #region Pyramid
    void DoGrow()
    {
        StartCoroutine(GrowUp());
    }
    IEnumerator GrowUp()
    {
        float startY = transform.localScale.y;
        float endY = 1f;
        float t = 0;
        while (t < handlingTime)
        {
            t += Time.deltaTime;
            float factor = t / handlingTime;
            float newY = Mathf.Lerp(startY, endY, EaseType.EaseOutQuart(factor));
            Vector3 scale = transform.localScale;
            scale.y = newY;
            transform.localScale = scale;
            yield return null;
        }
    }
    #endregion
    #region Rotating Wall
    void DoRotating()
    {
        float z = Mathf.Round(transform.position.z);
        if (z == 6) //Rotate right
        {
            StartCoroutine(Rotating(transform.position, Vector3.up, 90));
        }
        else if (z == -6) //Rotate left
        {
            StartCoroutine(Rotating(transform.position, Vector3.down, 90));
        }
        else 
        {
            if (player.transform.position.z < z) //Rotate right
                StartCoroutine(Rotating(transform.position, Vector3.up, 90));
            else if (player.transform.position.z > z)//Rotate right
                StartCoroutine(Rotating(transform.position, Vector3.down, 90));
            else
            {
                //Random 
                if (Random.value <= 0.5f)
                    StartCoroutine(Rotating(transform.position, Vector3.up, 90));
                else
                    StartCoroutine(Rotating(transform.position, Vector3.down, 90));
            }
        }
    }
    #endregion
    #region Spindle
    void DoSpin()
    {
        StartCoroutine(Spinning());
    }
    IEnumerator Spinning()
    {
        Vector3 rotateDir = (Random.value <= 0.5f) ? (Vector3.up) : (Vector3.down);
        while (true)
        {
            transform.Rotate(rotateDir, 4f);
            yield return null;
        }
    }
    #endregion
    #region Hingle
    void DoRotate(float handlingTime)
    {
        StartCoroutine(Rotating(transform.position, Vector3.forward, 90));
    }
    #endregion
    #region Bomb
    void DoBounce()
    {
        StartCoroutine(Bouncing());
    }
    IEnumerator Bouncing()
    {
        while (true)
        {
            Vector3 rotateDir = (Random.value <= 0.5f) ? (Vector3.up) : (Vector3.down);
            while (true)
            {
                transform.Rotate(rotateDir, 4f);
                yield return null;
            }
        }
    }
    #endregion
    #region Rolling Ball
    void RollTheBall()
    {
        StartCoroutine(Rolling());
    }
    IEnumerator Rolling()
    {
        float speed = Mathf.Abs(transform.position.z - (-transform.position.z)) / handlingTime;
        int currentZ = Mathf.RoundToInt(transform.position.z);
        if (currentZ == 6)
        {
            while (transform.position.z > -6f)
            {
                transform.eulerAngles -= new Vector3(speed*12 * Time.deltaTime, 0, 0);
                transform.position -= new Vector3(0, 0, speed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            while (transform.position.z < 6f)
            {
                transform.eulerAngles += new Vector3(speed *12* Time.deltaTime, 0, 0);
                transform.position += new Vector3(0, 0, speed * Time.deltaTime);
                yield return null;
            }
        }
    }
    #endregion
    IEnumerator Rotating(Vector3 rotatingPoint, Vector3 axis, float angle)
    {
        float startY = transform.position.y;

        float tStep = Mathf.Ceil(handlingTime * 30f);
        float tAngle = angle / tStep;

        for (int i = 0; i < tStep; i++)
        {
            transform.RotateAround(rotatingPoint, axis, tAngle);
            yield return null;
        }

        //Snaping the position 
        Vector3 position = transform.position;
        position.y = startY;
        transform.position = position;

        // Snaping the rotation to 90 degrees.    
        Vector3 angles = transform.eulerAngles;
        angles.x = Mathf.Round(angles.x / 90) * 90;
        angles.y = Mathf.Round(angles.y / 90) * 90;
        angles.z = Mathf.Round(angles.z / 90) * 90;
        transform.eulerAngles = angles;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (obstacleType == ObstacleType.BOMB)
            {
                GameManager.Instance.ObstacleExplore(transform.position);
                gameObject.SetActive(false);
            }
        }
    }

}
