using System.Collections;
using System.Collections.Generic;
using SgLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public enum ObstacleType
{
    BOXES,
    ROLLING_ROCK,
    LEFT_BARRIER,
    RIGHT_BARRIER,
    PYRAMID,
    ROTATING_WALL,
    SPINDLE,
    HINGE,
    BOMB,
    ROLLING_BALL,
}

public enum ItemType
{
    COIN,
    LASER,
    MAGNET,
}

[System.Serializable]
public struct Environment
{
    [SerializeField]
    public List<GameObject> listLeftBG;
    [SerializeField]
    public List<GameObject> listRightBG;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;
    public GameState getGame()
    {
        return _gameState;
    }

    public static int GameCount
    {
        get { return _gameCount; }
        private set { _gameCount = value; }
    }

    private static int _gameCount = 0;
    private static bool isRestarting = false;

    [Header("Check to enable premium features (require EasyMobile plugin)")]
    public bool enablePremiumFeatures = true;

    [Header("Set the target frame rate for this game")]
    [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
    public int targetFrameRate = 30;

    // List of public variable for gameplay tweaking
    [Header("Gameplay Config")]
    [SerializeField]
    private int firstPathNumber = 5;
    [SerializeField]
    private int initialPath = 10;
    [SerializeField]
    private int maxCoinPathNumber = 10;
    [SerializeField]
    private int minCoinPathNumber = 5;
    [SerializeField]
    private int changeEnvironmentScore = 30;

    public float minCheckingDistance = 30f;
    public float maxCheckingDistance = 40f;
    public float minObstacleHandlingTime = 0.8f;
    public float maxObstacleHandlingTime = 1.1f;
    public float laserModeTime = 8f;
    public float magnetModeTime = 10f;
    public float magnetModeRadius = 10f;
    public float laserLength = 40f;
    [SerializeField]
    private float minSkyColorLerpTime = 20f;
    [SerializeField]
    private float maxSkyColorLerpTime = 30f;
    [SerializeField]
    private Color[] skyColors;
    [Range(0f, 1f)]
    [SerializeField]
    private float coinFrequency = 0.1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float coinPathFrequency = 0.1f;
    [Range(0f, 1f)]
    [SerializeField]
    private float laserFrequency = 0.1f;
    [Range(0f, 1f)]
    [SerializeField]
    private float magnetFrequency = 0.1f;

    // List of public variables referencing other objects
    [Header("Object References")]
    public PlayerController playerController;
    public GameObject pathPackPrefab;
    public GameObject boxesPrefab;
    public GameObject rollingRockPrefab;
    public GameObject pyramidPrefab;
    public GameObject rotatingWallPrefab;
    public GameObject spindlePrefab;
    public GameObject hingePrefab;
    public GameObject leftBarrierPrefab;
    public GameObject rightBarrierPrefab;
    public GameObject bombPrefab;
    public GameObject rollingBallPrefab;
    public GameObject coinPrefab;
    public GameObject laserPrefab;
    public GameObject magnetPrefab;
    public GameObject firstPathPack;
    public GameObject hitItem;
    public GameObject hitObstacle;
    public GameObject blackHolePrefab;
    public Material skyBox;
    public Material gamePack;
    public Material playerExplore;
    public Environment[] environments;

    private List<GameObject> listPathPack = new List<GameObject>();
    private List<GameObject> listBoxes = new List<GameObject>();
    private List<GameObject> listRollingRock = new List<GameObject>();
    private List<GameObject> listPyramid = new List<GameObject>();
    private List<GameObject> listRotatingWall = new List<GameObject>();
    private List<GameObject> listSpindle = new List<GameObject>();
    private List<GameObject> listHinge = new List<GameObject>();
    private List<GameObject> listLeftBarrier = new List<GameObject>();
    private List<GameObject> listRightBarrier = new List<GameObject>();
    private List<GameObject> listBomb = new List<GameObject>();
    private List<GameObject> listRollingBall = new List<GameObject>();
    private List<GameObject> listCoin = new List<GameObject>();
    private List<GameObject> listLaser = new List<GameObject>();
    private List<GameObject> listMagnet = new List<GameObject>();
    private List<GameObject> listHitItem = new List<GameObject>();
    private List<GameObject> listHitObstacle = new List<GameObject>();
    private List<Environment> listEnvironment = new List<Environment>();
    private List<GameObject> listBlackHole = new List<GameObject>();
    private GameObject currentPath;
    private Vector3 pathSize;
    private Vector3 rollingObstacleSize;
    private int currentSkyColorIndex;
    private int currentBGColorIndex;
    private int environmentIndex;
    private int pathPackNumber;
    private int coinNumber = 20;
    private int obstacleNumber = 10;
    private int otherItemNumber = 5;
    private int coinPathNumber = 0;
    private int pathChildIndex = 0;
    private bool isCreatingCoinPath = false;

    void OnEnable()
    {
        PlayerController.PlayerDied += PlayerController_PlayerDied;
    }

    void OnDisable()
    {
        PlayerController.PlayerDied -= PlayerController_PlayerDied;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Use this for initialization
    void Start()
    {
        // Initial setup
        Application.targetFrameRate = targetFrameRate;
        ScoreManager.Instance.Reset();
        PrepareGame();
    }

    // Listens to the event when player dies and call GameOver
    void PlayerController_PlayerDied()
    {
        GameOver();
    }

    // Make initial setup and preparations before the game can be played
    public void PrepareGame()
    {
        GameState = GameState.Prepare;

        pathPackNumber = initialPath + firstPathNumber + 5;
        currentPath = firstPathPack;
        pathSize = pathPackPrefab.transform.GetChild(0).GetComponent<Renderer>().bounds.size;
        rollingObstacleSize = rollingRockPrefab.GetComponent<Renderer>().bounds.size;

        InitialObject();//Instantiate objects for pooling
        environmentIndex = Random.Range(0, listEnvironment.Count);

        //Create environment
        GetObject(listEnvironment[environmentIndex].listLeftBG).transform.position = firstPathPack.transform.position + Vector3.forward * 7.5f;
        GetObject(listEnvironment[environmentIndex].listRightBG).transform.position = firstPathPack.transform.position + Vector3.back * 7.5f;

        for (int i = 0; i < firstPathNumber; i++) //Create some path first (only the path)
        {
            CreatePath(true);
        }

        for (int i = 0; i < initialPath; i++) //Create path with obstacles
        {
            CreatePath(false);
        }

        //Random sky color
        currentSkyColorIndex = Random.Range(0, skyColors.Length);
        skyBox.SetColor("_Tint", skyColors[currentSkyColorIndex]);
        RenderSettings.fogColor = skyColors[currentSkyColorIndex];
        StartCoroutine(CheckAndLerpSkyboxColor());

        if (isRestarting)
        {
            isRestarting = false;
            StartGame();
        }
    }

    // A new game official starts
    public void StartGame()
    {
        GameState = GameState.Playing;
    }
    public bool GUGU = false;
    // Called when the player died
    public void GameOver()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver, true);
        GUGU = true;
        GameState = GameState.GameOver;
        GameCount++;

        // Add other game over actions here if necessary
    }

    // Start a new game
    public void RestartGame(float delay = 0)
    {
        StartCoroutine(CRRestartGame(delay));
    }

    IEnumerator CRRestartGame(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        isRestarting = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void InitialObject()
    {
        //Initial path packs
        InstantiateObject(pathPackPrefab, listPathPack, pathPackNumber);
        InstantiateObject(blackHolePrefab, listBlackHole, pathPackNumber);

        //Initial environments
        int leftBGIndex = -1;
        int rightBGIndex = -1;
        for (int i = 0; i < environments.Length; i++)
        {
            Environment ent = new Environment();
            ent.listLeftBG = new List<GameObject>();
            ent.listRightBG = new List<GameObject>();
            for (int a = 0; a < firstPathNumber + initialPath + 5; a++)
            {
                leftBGIndex = (leftBGIndex + 1 == environments[i].listLeftBG.Count - 1) ? (0) : (leftBGIndex + 1);
                InstantiateObject(environments[i].listLeftBG[leftBGIndex], ent.listLeftBG, 1);
            }
            for (int a = 0; a < firstPathNumber + initialPath + 5; a++)
            {
                rightBGIndex = (rightBGIndex + 1 == environments[i].listRightBG.Count - 1) ? (0) : (rightBGIndex + 1);
                InstantiateObject(environments[i].listRightBG[rightBGIndex], ent.listRightBG, 1);
            }
            listEnvironment.Add(ent);
        }

        //Initial traps
        InstantiateObject(boxesPrefab, listBoxes, obstacleNumber);
        InstantiateObject(rollingRockPrefab, listRollingRock, obstacleNumber);
        InstantiateObject(pyramidPrefab, listPyramid, obstacleNumber);
        InstantiateObject(rotatingWallPrefab, listRotatingWall, obstacleNumber);
        InstantiateObject(spindlePrefab, listSpindle, obstacleNumber);
        InstantiateObject(hingePrefab, listHinge, obstacleNumber);
        InstantiateObject(bombPrefab, listBomb, obstacleNumber * 2);
        InstantiateObject(rollingBallPrefab, listRollingBall, obstacleNumber);

        //Initial barriers
        int barrierCounter = 1;
        for (int i = 0; i < obstacleNumber; i++)
        {
            barrierCounter++;
            if (barrierCounter % 2 == 0)
                InstantiateObject(leftBarrierPrefab, listLeftBarrier, 1);
            else
                InstantiateObject(rightBarrierPrefab, listRightBarrier, 1);
        }

        //Initial items
        InstantiateObject(coinPrefab, listCoin, coinNumber);
        InstantiateObject(laserPrefab, listLaser, otherItemNumber);
        InstantiateObject(magnetPrefab, listMagnet, otherItemNumber);

        //Initial particles
        InstantiateObject(hitObstacle, listHitObstacle, coinNumber);
        InstantiateObject(hitItem, listHitItem, coinNumber + otherItemNumber);
    }

    void InstantiateObject(GameObject prefabObject, List<GameObject> storedList, int objectNumber)
    {
        for (int i = 0; i < objectNumber; i++)
        {
            GameObject ob = Instantiate(prefabObject, Vector3.zero, Quaternion.identity);
            ob.transform.SetParent(transform);
            ob.SetActive(false);
            storedList.Add(ob);
        }
    }

    //Get the unactive object of the list
    GameObject GetObject(List<GameObject> listObject)
    {
        GameObject ob = listObject[Random.Range(0, listObject.Count)];
        while (ob.activeInHierarchy)
        {
            ob = listObject[Random.Range(0, listObject.Count)];
        }
        ob.transform.SetParent(null);
        ob.SetActive(true);
        return ob;
    }

    //Create the path
    public void CreatePath(bool createPathOnly)
    {
        GameObject thisPath = GetObject(listPathPack);
        thisPath.transform.position = currentPath.transform.position + Vector3.right * pathSize.x;
        CreateEnvironment(thisPath);
        GetObject(listBlackHole).transform.position = thisPath.transform.position + Vector3.down * 3f;

        if (isCreatingCoinPath && !createPathOnly) //Create coin path
        {
            GameObject pathChild = thisPath.transform.GetChild(pathChildIndex).gameObject;
            CreateItem(pathChild, listCoin, 1); //Create coin with frequency is 1
            for (int i = 0; i < thisPath.transform.childCount; i++)
            {
                if (thisPath.transform.GetChild(i).gameObject != pathChild)
                    thisPath.transform.GetChild(i).gameObject.SetActive(false);
            }
            coinPathNumber--;
            if (coinPathNumber == 0) //Reset
                isCreatingCoinPath = false;
        }
        else if (!createPathOnly)
        {
            //Create traps
            CreateTrap(thisPath);

            //Create items
            GameObject pathChild = thisPath.transform.GetChild(Random.Range(0, thisPath.transform.childCount)).gameObject;
            while (!pathChild.activeInHierarchy)
            {
                pathChild = thisPath.transform.GetChild(Random.Range(0, thisPath.transform.childCount)).gameObject;
            }

            if (Random.value <= coinFrequency) //Create coin
                CreateItem(pathChild, listCoin, 1);
            else
            {
                int whichItem = Random.Range(0, 3);
                if (whichItem == 0) //Create coin path
                {
                    if (Random.value <= coinPathFrequency)
                        CreateCoinPath();
                }
                else if (whichItem == 1) //Create magnet
                    CreateItem(pathChild, listMagnet, magnetFrequency);
                else if (whichItem == 2) //Create laser
                    CreateItem(pathChild, listLaser, laserFrequency);
            }
        }
        currentPath = thisPath;
    }

    void CreateEnvironment(GameObject pathPack)
    {
        if (ScoreManager.Instance.Score % changeEnvironmentScore == 0 && ScoreManager.Instance.Score != 0)
        {
            //if (environmentIndex == environments.Length - 1) //Reset index
            //    environmentIndex = 0;
            //else
                //environmentIndex++;
            playerController.IncreaseSpeed();
        }
        GetObject(listEnvironment[environmentIndex].listLeftBG).transform.position = pathPack.transform.position + Vector3.forward * 7.5f;
        GetObject(listEnvironment[environmentIndex].listRightBG).transform.position = pathPack.transform.position + Vector3.back * 7.5f;
    }


    void CreateTrap(GameObject pathPack)
    {
        int whichTrap = Random.Range(0, 11);
        if (whichTrap == 0)//Hide some ground of the path
            HideGround(pathPack);
        else if (whichTrap == 1)  //Create boxes
            CreateBoxes(pathPack);
        else if (whichTrap == 2) //Create rolling rock
            CreateRollingRock(pathPack);
        else if (whichTrap == 3) //Create barrier
            CreateBarrier(pathPack);
        else if (whichTrap == 4) //Create growing tree
            CreatePyramid(pathPack);
        else if (whichTrap == 5) //Create rotating wall
            CreateRotatingWall(pathPack);
        else if (whichTrap == 6) //Create spindle
            CreateSpindle(pathPack);
        else if (whichTrap == 7) //Create hinge
            CreateHinge(pathPack);
        else if (whichTrap == 8) //Create pyramid
            CreatePyramid(pathPack);
        else if (whichTrap == 9) //Create bombs
            CreateBombs(pathPack);
        else if (whichTrap == 10)//Create rolling ball
            CreateRollingBall(pathPack);
    }

    #region Traps

    void CreateBoxes(GameObject pathPack)
    {
        GetObject(listBoxes).transform.position = pathPack.transform.position;
    }

    void CreateRollingRock(GameObject pathPack)
    {
        GameObject pathchild = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).gameObject;
        GetObject(listRollingRock).transform.position = pathchild.transform.position + Vector3.up * rollingObstacleSize.y / 2f;
    }

    void CreatePyramid(GameObject pathPack)
    {
        GameObject pathChild = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).gameObject;
        GameObject pyramid = GetObject(listPyramid);
        pyramid.transform.position = pathChild.transform.position + Vector3.down * 0.1f;
        pyramid.transform.localScale = new Vector3(1, 0, 1);
    }

    void CreateRotatingWall(GameObject pathPack)
    {
        GameObject pathChild = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).gameObject;
        GetObject(listRotatingWall).transform.position = pathChild.transform.position;
    }

    void CreateSpindle(GameObject pathPack)
    {
        GameObject pathChild;
        do
        {
            pathChild = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).gameObject;
        } while (pathChild.name.Equals("LastLeft") || pathChild.name.Equals("LastRight"));
        GetObject(listSpindle).transform.position = pathChild.transform.position;
    }

    void CreateHinge(GameObject pathPack)
    {
        GameObject pathChild;
        do
        {
            pathChild = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).gameObject;
        } while (pathChild.name.Equals("LastLeft") || pathChild.name.Equals("LastRight"));
        GameObject hinge = GetObject(listHinge);
        hinge.transform.position = pathChild.transform.position;
        if (pathChild.name.Equals("MiddleLeft"))
            hinge.transform.position += Vector3.forward * pathSize.z / 2f;
        else
            hinge.transform.position += Vector3.back * pathSize.z / 2f;

    }

    void CreateBarrier(GameObject pathPack)
    {
        if (Random.value <= 0.5f) //Create left barrier
            GetObject(listLeftBarrier).transform.position = pathPack.transform.position + Vector3.forward * 7.5f;
        else //Create right barrier
            GetObject(listRightBarrier).transform.position = pathPack.transform.position + Vector3.back * 7.5f;
    }

    void HideGround(GameObject pathPack)//Hide some ground of the path
    {
        List<Transform> grounds = new List<Transform>();
        for (int i = 0; i < pathPack.transform.childCount; ++i)
        {
            grounds.Add(pathPack.transform.GetChild(i));
        }

        if (Random.value <= 0.5f) //Hide 1 ground of the path
            grounds[Random.Range(0, grounds.Count)].gameObject.SetActive(false);
        else //Hide 2 ground of the path
        {
            GameObject ground_1 = grounds[Random.Range(0, grounds.Count)].gameObject;
            GameObject ground_2 = grounds[Random.Range(0, grounds.Count)].gameObject;
            while (ground_2 == ground_1)
            {
                ground_2 = grounds[Random.Range(0, grounds.Count)].gameObject;
            }
            ground_1.SetActive(false);
            ground_2.SetActive(false);
        }
    }

    private void RemoveCurrentGround(List<Transform> grounds)
    {

    }

    void CreateBombs(GameObject pathPack)
    {
        int value = Random.Range(0, 3);
        if (value == 0) //Create 1 bomb
        {
            Vector3 pos = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            GetObject(listBomb).transform.position = new Vector3(pos.x, playerController.transform.position.y, pos.z);
        }
        else if (value == 1) //Create 2 bombs
        {
            Vector3 pos_1 = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            Vector3 pos_2 = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            while (pos_2 == pos_1)
            {
                pos_2 = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            }
            GetObject(listBomb).transform.position = new Vector3(pos_1.x, playerController.transform.position.y, pos_1.z);
            GetObject(listBomb).transform.position = new Vector3(pos_2.x, playerController.transform.position.y, pos_2.z);
        }
        else //Create 3 bombs
        {
            Vector3 pos_1 = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            Vector3 pos_2 = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            while (pos_2 == pos_1)
            {
                pos_2 = pathPack.transform.GetChild(Random.Range(0, pathPack.transform.childCount)).transform.position;
            }
            for (int i = 0; i < pathPack.transform.childCount; i++)
            {
                Vector3 pos = pathPack.transform.GetChild(i).transform.position;
                if (pos != pos_1 && pos != pos_2)
                {
                    GetObject(listBomb).transform.position = new Vector3(pos.x, playerController.transform.position.y, pos.z);
                }
            }
        }
    }

    void CreateRollingBall(GameObject pathPack)
    {
        Vector3 pos;
        if (Random.value <= 0.5f) //Create on left
            pos = pathPack.transform.Find("LastLeft").transform.position;
        else //Create on right
            pos = pathPack.transform.Find("LastRight").transform.position;
        GetObject(listRollingBall).transform.position = new Vector3(pos.x, playerController.transform.position.y, pos.z);
    }

    #endregion

    #region Coin path and items

    void CreateCoinPath()//Create the coin path
    {
        coinPathNumber = Random.Range(minCoinPathNumber, maxCoinPathNumber); //Random a coin path number
        pathChildIndex = Random.Range(0, pathPackPrefab.transform.childCount); //Get a random child of path pack
        isCreatingCoinPath = true;
    }

    void CreateItem(GameObject pathChild, List<GameObject> listItem, float frequency)
    {
        if (Random.value <= frequency)
            GetObject(listItem).transform.position = new Vector3(pathChild.transform.position.x,
                playerController.transform.position.y,
                pathChild.transform.position.z) + Vector3.right * pathSize.x / 2;
    }

    #endregion

    IEnumerator CheckAndLerpSkyboxColor()
    {
        while (true)
        {
            if (GameState == GameState.Playing)
            {
                StartCoroutine(LerpSkyboxColor());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator LerpSkyboxColor()
    {
        while (GameState == GameState.Playing)
        {
            float lerpTime = Random.Range(minSkyColorLerpTime, maxSkyColorLerpTime);
            Color skyStartColor;
            Color skyEndColor;
            if (currentSkyColorIndex == skyColors.Length - 1)
            {
                skyStartColor = skyColors[currentSkyColorIndex];
                skyEndColor = skyColors[0];
                currentSkyColorIndex = 0;
            }
            else
            {
                skyStartColor = skyColors[currentSkyColorIndex];
                skyEndColor = skyColors[currentSkyColorIndex + 1];
                currentSkyColorIndex++;
            }
            float t = 0;
            while (t < lerpTime)
            {
                t += Time.deltaTime;
                float factor = t / lerpTime;
                skyBox.SetColor("_Tint", Color.Lerp(skyStartColor, skyEndColor, factor));
                RenderSettings.fogColor = Color.Lerp(skyStartColor, skyEndColor, factor);
                yield return null;
            }
        }
    }

    public void ItemExplore(Vector3 position)
    {
        StartCoroutine(Exploding(listHitItem, position, true, false));
    }

    public void ObstacleExplore(Vector3 position)
    {
        StartCoroutine(Exploding(listHitObstacle, position, false, false));
    }

    public void PlayerExplodes(Vector3 position, float delay = 0)
    {
        StartCoroutine(Exploding(listHitObstacle, position, false, true, delay));
    }

    IEnumerator Exploding(List<GameObject> listParticle, Vector3 position, bool isItemExplodeParticle, bool isPlayerDie, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        ParticleSystem par = GetObject(listParticle).GetComponent<ParticleSystem>();
        par.transform.position = position;
        var main = par.main;
        if (isItemExplodeParticle)
        {
            main.simulationSpace = ParticleSystemSimulationSpace.Custom;
            main.customSimulationSpace = Camera.main.transform;
        }
        if (isPlayerDie)
            par.GetComponent<Renderer>().material = playerExplore;
        par.Play();
        yield return new WaitForSeconds(par.main.startLifetimeMultiplier);
        par.transform.SetParent(transform);
        par.gameObject.SetActive(false);
    }
}
