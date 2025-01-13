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
            Debug.LogError("Board not initialized!");
            return;
        }

        int row = 0;
        while (row < 6 && !CanPlaceInSlot(row, col))
        {
            row++;
        }

        if (row >= 6)
        {
            Debug.LogWarning("Column full!");
            return;
        }

        Debug.Log($"Placing block at position [{row}, {col}]");
        slotAvailabilities[row, col].IsAvailable = false;
        blockLogics[row, col] = block.GetComponent<BlockLogic>();
        if (blockLogics[row, col] == null)
        {
            Debug.LogError("Failed to get BlockLogic component!");
        }
        StartCoroutine(SmoothAttachToParent(block, board[row, col].transform));
    }

    private IEnumerator SmoothAttachToParent(GameObject block, Transform targetParent)
    {
        Debug.Log("Starting SmoothAttachToParent");
        if (block == null || targetParent == null)
        {
            Debug.LogError("Block or targetParent is null!");
            yield break;
        }

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

        Debug.Log("Block movement complete, waiting before check");
        yield return new WaitForSeconds(0.1f);

        Debug.Log("Calling CheckNeighborSlots");
        CheckNeighborSlots(); // This line might not be executing
    }

    public bool CanPlaceInSlot(int row, int col)
    {
        if (!isInitialized) return false;

        if (row < 0 || row >= 6 || col < 0 || col >= 6) return false;

        return board[row, col].GetComponent<SlotAvailability>().IsAvailable;
    }

    void CheckNeighborSlots()
    {
        Debug.Log("=== Starting CheckNeighborSlots ===");

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                if (blockLogics[row, col] == null) continue;

                Debug.Log($"Checking block at [{row}, {col}]");
                BlockLogic currentBlock = blockLogics[row, col];
                currentBlock.LogColors();

                // Check Left
                if (col > 0 && blockLogics[row, col - 1] != null)
                {
                    BlockLogic leftNeighbor = blockLogics[row, col - 1];
                    Debug.Log($"Checking left neighbor - Current LT:{currentBlock.LeftTopColor} vs Neighbor RT:{leftNeighbor.RightTopColor}");

                    if (currentBlock.LeftTopColor == leftNeighbor.RightTopColor)
                    {
                        Debug.Log("Match found on Left Top!");
                        StartCoroutine(HandleColorMatch(currentBlock, leftNeighbor, "lt", "left"));
                    }
                    if (currentBlock.LeftBotColor == leftNeighbor.RightBotColor)
                    {
                        Debug.Log("Match found on Left Bottom!");
                        StartCoroutine(HandleColorMatch(currentBlock, leftNeighbor, "lb", "left"));
                    }
                }

                // Check Right
                if (col < 5 && blockLogics[row, col + 1] != null)
                {
                    BlockLogic rightNeighbor = blockLogics[row, col + 1];
                    Debug.Log($"Checking right neighbor - Current RT:{currentBlock.RightTopColor} vs Neighbor LT:{rightNeighbor.LeftTopColor}");

                    if (currentBlock.RightTopColor == rightNeighbor.LeftTopColor)
                    {
                        Debug.Log("Match found on Right Top!");
                        StartCoroutine(HandleColorMatch(currentBlock, rightNeighbor, "rt", "right"));
                    }
                    if (currentBlock.RightBotColor == rightNeighbor.LeftBotColor)
                    {
                        Debug.Log("Match found on Right Bottom!");
                        StartCoroutine(HandleColorMatch(currentBlock, rightNeighbor, "rb", "right"));
                    }
                }

                // Check Bottom
                if (row > 0 && blockLogics[row - 1, col] != null)
                {
                    BlockLogic bottomNeighbor = blockLogics[row - 1, col];
                    Debug.Log($"Checking bottom neighbor - Current LB:{currentBlock.LeftBotColor} vs Neighbor LT:{bottomNeighbor.LeftTopColor}");

                    if (currentBlock.LeftBotColor == bottomNeighbor.LeftTopColor)
                    {
                        Debug.Log("Match found on Left Bottom!");
                        StartCoroutine(HandleColorMatch(currentBlock, bottomNeighbor, "lb", "bot"));
                    }
                    if (currentBlock.RightBotColor == bottomNeighbor.RightTopColor)
                    {
                        Debug.Log("Match found on Right Bottom!");
                        StartCoroutine(HandleColorMatch(currentBlock, bottomNeighbor, "rb", "bot"));
                    }
                }

                // Check Top
                if (row < 5 && blockLogics[row + 1, col] != null)
                {
                    BlockLogic topNeighbor = blockLogics[row + 1, col];
                    Debug.Log($"Checking top neighbor - Current LT:{currentBlock.LeftTopColor} vs Neighbor LB:{topNeighbor.LeftBotColor}");

                    if (currentBlock.LeftTopColor == topNeighbor.LeftBotColor)
                    {
                        Debug.Log("Match found on Left Top!");
                        StartCoroutine(HandleColorMatch(currentBlock, topNeighbor, "lt", "top"));
                    }
                    if (currentBlock.RightTopColor == topNeighbor.RightBotColor)
                    {
                        Debug.Log("Match found on Right Top!");
                        StartCoroutine(HandleColorMatch(currentBlock, topNeighbor, "rt", "top"));
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

    private SpriteRenderer GetMatchingSprite(BlockLogic block, string position)
    {
        SpriteRenderer[] sprites = block.GetComponentsInChildren<SpriteRenderer>();
        switch (position)
        {
            case "lt": return sprites[2];  // SingleColor128_0 (2)
            case "rt": return sprites[3];  // SingleColor128_0 (3)
            case "lb": return sprites[0];  // SingleColor128_0
            case "rb": return sprites[1];  // SingleColor128_0 (1)
            default:
                Debug.LogError($"Invalid position: {position}");
                return null;
        }
    }

    private SpriteRenderer GetLocalNeighborSprite(BlockLogic block, string position)
    {
        SpriteRenderer[] sprites = block.GetComponentsInChildren<SpriteRenderer>();
        switch (position)
        {
            case "lt": return sprites[3];  // For left-top, get right-top
            case "rt": return sprites[2];  // For right-top, get left-top
            case "lb": return sprites[1];  // For left-bottom, get right-bottom
            case "rb": return sprites[0];  // For right-bottom, get left-bottom
            default:
                Debug.LogError($"Invalid position: {position}");
                return null;
        }
    }

    private string GetOppositePosition(string position, string direction)
    {
        if (direction == "left") return position[0] == 'l' ? "rt" : "rb";
        if (direction == "right") return position[0] == 'r' ? "lt" : "lb";
        if (direction == "bot") return position[1] == 't' ? "lb" : "lt";
        return position;
    }

    private IEnumerator HandleColorMatch(BlockLogic currentBlock, BlockLogic neighborBlock,
    string position, string direction)
    {
        Debug.Log($"HandleColorMatch started - position: {position}, direction: {direction}");

        // Get the matching sprites
        SpriteRenderer sprite1 = GetMatchingSprite(currentBlock, position);
        SpriteRenderer sprite2 = GetMatchingSprite(neighborBlock, GetOppositePosition(position, direction));

        if (sprite1 == null || sprite2 == null)
        {
            Debug.LogError($"Sprites null - sprite1: {sprite1}, sprite2: {sprite2}");
            yield break;
        }

        Color originalColor1 = sprite1.color;
        Color originalColor2 = sprite2.color;
        Debug.Log($"Original colors - sprite1: {originalColor1}, sprite2: {originalColor2}");

        // Turn white for 0.6 seconds
        sprite1.color = Color.white;
        sprite2.color = Color.white;
        yield return new WaitForSeconds(0.6f);

        // Get the local neighbor sprites' colors
        SpriteRenderer localNeighbor1 = GetLocalNeighborSprite(currentBlock, position);
        SpriteRenderer localNeighbor2 = GetLocalNeighborSprite(neighborBlock, GetOppositePosition(position, direction));

        if (localNeighbor1 == null || localNeighbor2 == null)
        {
            Debug.LogError($"Local neighbors null - neighbor1: {localNeighbor1}, neighbor2: {localNeighbor2}");
            sprite1.color = originalColor1;
            sprite2.color = originalColor2;
            yield break;
        }

        Color newColor1 = localNeighbor1.color;
        Color newColor2 = localNeighbor2.color;
        Debug.Log($"New colors - from localNeighbor1: {newColor1}, from localNeighbor2: {newColor2}");

        // Apply the new colors
        sprite1.color = newColor1;
        sprite2.color = newColor2;

        // Update the BlockLogic colors
        UpdateBlockColors(currentBlock, position, GetColorFromUnityColor(newColor1));
        UpdateBlockColors(neighborBlock, GetOppositePosition(position, direction), GetColorFromUnityColor(newColor2));
    }

    // Add this helper method to convert Unity Color back to BlockColor
    private BlockLogic.BlockColor GetColorFromUnityColor(Color color)
    {
        if (color == Color.red) return BlockLogic.BlockColor.Red;
        if (color == Color.green) return BlockLogic.BlockColor.Green;
        if (color == Color.blue) return BlockLogic.BlockColor.Blue;
        if (color == Color.yellow) return BlockLogic.BlockColor.Yellow;
        return BlockLogic.BlockColor.Red; // Default fallback
    }

    // Add this helper method to update the block's logical colors
    private void UpdateBlockColors(BlockLogic block, string position, BlockLogic.BlockColor newColor)
    {
        switch (position)
        {
            case "lt": block.SetColors(2, newColor); break;
            case "lb": block.SetColors(0, newColor); break;
            case "rt": block.SetColors(3, newColor); break;
            case "rb": block.SetColors(1, newColor); break;
        }
    }
}