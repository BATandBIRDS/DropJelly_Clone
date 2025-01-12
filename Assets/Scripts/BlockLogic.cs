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
        SetRandomBlockColors(); //for testing
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
}