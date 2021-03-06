using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [Header("Input Settings : ")] 
    [SerializeField]
    private LayerMask boxesLayerMask;

    [Header("Mark Sprites : ")] 
    [SerializeField]
    private Sprite spriteX;
    [SerializeField] 
    private Sprite spriteO;

    public UnityAction<BoxState> OnWinAction;
    public BoxState[] boxStates;
    public List<Box> boxList;
    private Camera cam;
    private BoxState currentMark;
    private BoxState playerMark;
    private BoxState enemyMark;
    private bool canPlay;
    private LineRenderer lineRenderer;
    private int marksCount = 0;

    public GameObject networkedClient;
    public GameObject gameManager;
    public Text turnTip;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (var go in allObjects)
        {
            if (go.name == "GameManager")
                gameManager = go;
        }
        
        cam = Camera.main ;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        currentMark = BoxState.X;
        boxStates = new BoxState[9];

        canPlay = gameManager.GetComponent<GameManager>().getCanPlay();
        SetGame(canPlay);
        //Debug.Log("step is empty? " + gameManager.GetComponent<GameManager>().IsStepListEmpty());
    }

    // Update is called once per frame
    void Update()
    {
        if (canPlay && Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(clickPosition, boxesLayerMask);

            if (hit)
            {
                int index = hit.GetComponent<Box>().index;
                DrawMark(index,true);
                //MarkBox(hit.GetComponent<Box>());
            }
        }
    }
    
    /*
    private void MarkBox(Box box)
    {
        if (box.isEmpty)
        {
            boxStates[box.index] = currentMark;
            if (currentMark == BoxState.X)
            {
                box.MarkBox(spriteX,BoxState.X);
            }
            else if (currentMark == BoxState.O)
            {
                box.MarkBox(spriteO, BoxState.O);
            }

            marksCount++;

            bool won = CheckWin();
            if (won)
            {
                if (OnWinAction != null)
                {
                    OnWinAction.Invoke(currentMark);
                    Debug.Log(currentMark+" win");
                }

                canPlay = false;
                return;
            }

            if (marksCount == 9)
            {
                if (OnWinAction != null)
                {
                    OnWinAction.Invoke(BoxState.Empty);
                    Debug.Log("Nobody wins");
                }

                canPlay = false;
                return;
            }

            SwitchPlayer();
        }
    }
    */

    public void DrawMark(int location, bool isPlayer)
    {
        if (boxStates[location] == BoxState.Empty)
        {
            if (isPlayer)
            {
                currentMark = playerMark;
            }
            else
            {
                currentMark = enemyMark;
            }

            boxStates[location] = currentMark;
            //Debug.Log("Xia Qi" + currentMark);
            if (currentMark == BoxState.X)
            {
                boxList[location].MarkBox(spriteX,BoxState.X);
            }
            else if (currentMark == BoxState.O)
            {
                boxList[location].MarkBox(spriteO, BoxState.O);
            }
            
            if(isPlayer)
                gameManager.GetComponent<GameManager>().MarkDrawed(location, playerMark);
            marksCount++;

            bool won = CheckWin();
            if (won)
            {
                if (OnWinAction != null)
                {
                    OnWinAction.Invoke(currentMark);
                    Debug.Log(currentMark+" win");
                    gameManager.GetComponent<GameManager>().GameOver();
                }

                canPlay = false;
                return;
            }

            if (marksCount == 9)
            {
                if (OnWinAction != null)
                {
                    OnWinAction.Invoke(BoxState.Empty);
                    Debug.Log("Nobody wins");
                    gameManager.GetComponent<GameManager>().GameOver();
                }

                canPlay = false;
                return;
            }
            
            canPlay = false;
            turnTip.text = "Opponent Turn";
        }
    }

    public void SetBoxMarked(int location, BoxState state)
    {
        if (state == BoxState.X)
        {
            boxList[location].MarkBox(spriteX,BoxState.X);
        }
        else if (state == BoxState.O)
        {
            boxList[location].MarkBox(spriteO, BoxState.O);
        }
        
        marksCount++;
    }

    private bool CheckWin()
    {
        bool isWin = CheckLine(0, 1, 2) || CheckLine(3, 4, 5) || CheckLine(6, 7, 8) || CheckLine(0, 3, 6) ||
                     CheckLine(1, 4, 7) || CheckLine(2, 5, 8) || CheckLine(0, 4, 8) || CheckLine(2, 4, 6);
        return isWin;
    }

    public bool CheckGameOver()
    {
        return CheckWin() || marksCount >= 9;
    }

    private bool CheckLine(int a, int b, int c)
    {
        bool result = boxStates[a] != BoxState.Empty && boxStates[a] == boxStates[b] && boxStates[a] == boxStates[c] &&
                      boxStates[b] == boxStates[c];
        if (result)
        {
            DrawLine(a,c);
        }

        //Debug.Log(boxStates[a]+" "+boxStates[b]+" "+boxStates[c]);
        return result;
    }

    private void DrawLine(int start, int end)
    {
        lineRenderer.SetPosition(0, transform.GetChild(start).position);
        lineRenderer.SetPosition(1, transform.GetChild(end).position);
        lineRenderer.enabled = true;
    }

    private void SwitchPlayer()
    {
        if (currentMark == BoxState.X)
        {
            currentMark = BoxState.O;
        }
        else
        {
            currentMark = BoxState.X;
        }
    }

    public void EnterPlayerTurn()
    {
        canPlay = true;
        turnTip.text = "Your Turn";
    }

    public void LoadStep()
    {
        if (!gameManager.GetComponent<GameManager>().IsStepListEmpty())
        {
            //Debug.Log("Kai Shi Hui Su");
            foreach (GameManager.Step step in gameManager.GetComponent<GameManager>().GetStepList())
            {
                SetBoxMarked(step.Location, step.State);
            }
            gameManager.GetComponent<GameManager>().ClearStepList();
        }
        else
        {
            //Debug.Log("mei you jia zai");
        }
    }

    public void StartReplay()
    {
        lineRenderer.enabled = false;
        foreach (Box box in boxList)
        {
            box.clearBox();
        }

        for (int i = 0; i < 9; i++)
        {
            boxStates[i] = BoxState.Empty;
        }

        StartCoroutine(Replay());
    }

    private IEnumerator Replay()
    {
        foreach (GameManager.Step step in gameManager.GetComponent<GameManager>().GetStepList())
        {
            SetBoxMarked(step.Location,step.State);
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void SetGame(bool isFirstPlayer)
    {
        if (isFirstPlayer)
        {
            playerMark = BoxState.X;
            enemyMark = BoxState.O;
            turnTip.text = "Your Turn";
        }
        else
        {
            playerMark = BoxState.O;
            enemyMark = BoxState.X;
            turnTip.text = "Opponent Turn";
        }
    }
}
