using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    [SerializeField] GameObject slotPrefab;
    private GameObject[,] board = new GameObject[6, 6];
    private bool isInitialized = false;

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
                GameObject slot = Instantiate(slotPrefab, new Vector3(-2f + (row * 0.8f), -2.8f + (col * 0.8f), 0f), Quaternion.identity, transform.parent);
                board[col, row] = slot;
            }
        }
    }

    public void PlaceBlockInColumn(int col, GameObject block)
    {
        if (!isInitialized) return;

        int r = 0;
        bool pass = !board[r, col].GetComponent<SlotAvailability>().IsAvailable;
        while (r < 6 && pass)
        {
            r++;
        }

        board[r, col].GetComponent<SlotAvailability>().IsAvailable = false;
        StartCoroutine(SmoothAttachToParent(block, board[r, col].transform));
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

            if (block != null) // Safety check
            {
                block.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            yield return null;
        }

        if (block != null && targetParent != null) // Final safety check
        {
            block.transform.position = targetPosition;
            block.transform.parent = targetParent;
        }
    }

    public bool CanPlaceInColumn(int col)
    {
        if (!isInitialized) return false;

        if (col < 0 || col >= 6) return false;

        return board[5, col].GetComponent<SlotAvailability>().IsAvailable;
    }
}