using UnityEngine;
using System.Collections;
using static UnityEngine.Rendering.DebugUI.Table;

public class BoardManager : MonoBehaviour
{
    [SerializeField] GameObject slotPrefab;
    
    private GameObject[,] board = new GameObject[6, 6];
    [HideInInspector]
    public bool isInitialized = false;

    private SlotAvailability[,] slotAvailabilities = new SlotAvailability[6, 6];
    private BlockLogic[,] blockLogics = new BlockLogic[6, 6];

    private void Start()
    {
        // Delay initialization by one frame to ensure all components are ready
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return null; // Wait one frame
        InitializeBoard();
        isInitialized = true;
    }

    void InitializeBoard()
    {
        for (int col = 0; col < 6; col++)
        {
            for (int row = 0; row < 6; row++)
            {
                GameObject slot = Instantiate(slotPrefab,
                    new Vector3(-2f + (row * 0.8f), 
                    -2.8f + (col * 0.8f), 0f), 
                    Quaternion.identity, 
                    transform.parent);
                board[col, row] = slot;

                // Cache the SlotAvailability component
                slotAvailabilities[col, row] = slot.GetComponent<SlotAvailability>();
            }
        }
    }

    public void PlaceBlockInColumn(int col, GameObject block)
    {
        if (!isInitialized)
        {
            Debug.Log("Board not initialized!");
            return;
        }

        int row = 0;
        
        while (row < 6 && !CanPlaceInSlot(row, col))
        {
            row++;
        }

        //Debug.Log($"Placing block at row {row}, column {col}");
        if (row >= 6) return;

        slotAvailabilities[row, col].IsAvailable = false;
        blockLogics[row, col] = block.GetComponent<BlockLogic>();
        StartCoroutine(SmoothAttachToParent(block, board[row, col].transform));
    }

    private IEnumerator SmoothAttachToParent(GameObject block, Transform targetParent)
    {
        if (block == null || targetParent == null) yield break;

        float duration = 0.8f;
        float elapsedTime = 0;
        Vector3 startPosition = block.transform.position;
        Vector3 targetPosition = targetParent.position;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            block.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        } 
        block.transform.position = targetPosition;
        block.transform.parent = targetParent;
        CheckNeighborSlots();
    }

    public bool CanPlaceInSlot(int row, int col)
    {
        if (!isInitialized) return false;

        if (row < 0 || row >= 6 || col < 0 || col >= 6) return false;

        return board[row, col].GetComponent<SlotAvailability>().IsAvailable;
    }

    void CheckNeighborSlots()
    {
        // Direction arrays for checking neighbors (left, right, down, up)
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        string[] directions = { "left", "right", "bot", "bot" };

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                if (slotAvailabilities[row, col].IsAvailable) continue;

                var currentBlock = blockLogics[row, col];
                if (currentBlock == null) continue;  // Skip if no block is present

                // Check all four directions
                for (int dir = 0; dir < 4; dir++)
                {
                    int newRow = row + dy[dir];
                    int newCol = col + dx[dir];

                    // Skip if out of bounds
                    if (newRow < 0 || newRow >= 6 || newCol < 0 || newCol >= 6) continue;

                    // Skip if neighbor slot is empty or has no block
                    if (slotAvailabilities[newRow, newCol].IsAvailable) continue;
                    var neighborBlock = blockLogics[newRow, newCol];
                    if (neighborBlock == null) continue;

                    // Check color matches based on direction
                    switch (dir)
                    {
                        case 0: // Left
                            CheckColorMatch(currentBlock.LeftTopColor, neighborBlock.RightTopColor, "lt", directions[dir]);
                            CheckColorMatch(currentBlock.LeftBotColor, neighborBlock.RightBotColor, "lb", directions[dir]);
                            break;

                        case 1: // Right
                            CheckColorMatch(currentBlock.RightTopColor, neighborBlock.LeftTopColor, "rt", directions[dir]);
                            CheckColorMatch(currentBlock.RightBotColor, neighborBlock.LeftBotColor, "rb", directions[dir]);
                            break;

                        case 2: // Down
                            CheckColorMatch(currentBlock.LeftBotColor, neighborBlock.LeftTopColor, "lb", directions[dir]);
                            CheckColorMatch(currentBlock.RightBotColor, neighborBlock.RightTopColor, "rb", directions[dir]);
                            break;

                        case 3: // Up
                            CheckColorMatch(currentBlock.LeftTopColor, neighborBlock.LeftBotColor, "lb", directions[dir]);
                            CheckColorMatch(currentBlock.RightTopColor, neighborBlock.RightBotColor, "rb", directions[dir]);
                            break;
                    }
                }
            }
        }
    }

    private void CheckColorMatch(BlockLogic.BlockColor color1, BlockLogic.BlockColor color2, string position, string direction)
    {
        if (color1 == color2)
        {
            Debug.Log($"{position}-{direction}");
        }
    }

    //void CheckLeftSlot(int row, int col) { }
    //void CheckRightSlot(int row, int col) { }
    //void CheckTopSlot(int row, int col) { }
    //void CheckDownSlot(int row, int col) { }
}