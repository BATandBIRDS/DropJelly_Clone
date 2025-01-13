using UnityEngine;
using System.Collections.Generic;

public class BlockLogic : MonoBehaviour
{// Attached to Prefabs/Block
    public enum BlockColor { Red, Green, Blue, Yellow }

    private readonly BlockColor[] validColors = { BlockColor.Red, BlockColor.Green, BlockColor.Blue, BlockColor.Yellow };
    private readonly System.Random rnd = new System.Random();
    private BlockColor[,] block = new BlockColor[2, 2];

    // Add references to sprite renderers
    private SpriteRenderer[] spriteRenderers;

    [SerializeField] Vector3 spawnPosition = new Vector3(0, 2.5f, 0);

    void Awake()
    {
        // Get all child sprite renderers
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Validate we have exactly 4 sprites
        if (spriteRenderers.Length != 4)
        {
            Debug.LogError("Block must have exactly 4 sprite children!");
            return;
        }
    }

    void Start()
    {
        // Set Initial Position Position
        transform.position = spawnPosition;
        SetRandomBlockColors(); // Initialize colors on start
    }

    public void SpawnNewBlock()
    {
        // Create the new block and store the reference
        GameObject newBlock = Instantiate(gameObject, spawnPosition, Quaternion.identity);

        // Get the BlockLogic component of the new instance and set its colors
        BlockLogic newBlockLogic = newBlock.GetComponent<BlockLogic>();
        newBlockLogic.SetRandomBlockColors();
    }

    void SetRandomBlockColors()
    {
        bool validConfiguration;
        var colorCounts = new Dictionary<BlockColor, int>();

        do
        {
            colorCounts.Clear();
            validConfiguration = true;

            // Generate random colors
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    BlockColor color = validColors[rnd.Next(validColors.Length)];
                    block[i, j] = color;

                    colorCounts.TryGetValue(color, out int count);
                    colorCounts[color] = count + 1;

                    int spriteIndex = i * 2 + j;
                    spriteRenderers[spriteIndex].color = GetUnityColor(color);
                }
            }

            // Check diagonal matches
            if (block[0, 0] == block[1, 1] || block[0, 1] == block[1, 0])
            {
                validConfiguration = false;
                continue;
            }

            // Check for three of a kind
            foreach (var kvp in colorCounts)
            {
                if (kvp.Value == 3)
                {
                    FillBlockWithColor(kvp.Key);
                    return; // Exit immediately after filling
                }
            }

        } while (!validConfiguration);
        LogColors();
    }

    public void LogColors()
    {
        string colorInfo = $"Block at {transform.position}:\n" +
            $"LeftTop: {LeftTopColor}, RightTop: {RightTopColor}\n" +
            $"LeftBot: {LeftBotColor}, RightBot: {RightBotColor}";
        Debug.Log(colorInfo);
    }

    private void FillBlockWithColor(BlockColor color)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                block[i, j] = color;
                int spriteIndex = i * 2 + j;
                spriteRenderers[spriteIndex].color = GetUnityColor(color);
            }
        }
    }

    private Color GetUnityColor(BlockColor blockColor)
    {
        return blockColor switch
        {
            BlockColor.Red => Color.red,
            BlockColor.Green => Color.green,
            BlockColor.Blue => Color.blue,
            BlockColor.Yellow => Color.yellow,
            _ => Color.white
        };
    }

    public BlockColor LeftTopColor { get { return block[1, 0]; } }
    public BlockColor LeftBotColor { get { return block[0, 0]; } }
    public BlockColor RightTopColor { get { return block[1, 1]; } }
    public BlockColor RightBotColor { get { return block[0, 1]; } }

    public void SetColors(int index, BlockColor color)
    {
        int row = index / 2;
        int col = index % 2;
        block[row, col] = color;

        // Also update the sprite color
        if (spriteRenderers != null && spriteRenderers.Length > index)
        {
            spriteRenderers[index].color = GetUnityColor(color);
        }
    }



}