using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InputHandler : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDraggable = true;
    private Vector3 targetPosition;
    private float smoothSpeed = 15f; // Adjust this value to change movement speed

    [SerializeField] private float[] columnPositionsX = { -2f, -1.2f, -0.4f, 0.4f, 1.2f, 2f };
    [SerializeField] private float defaultY = 2f;
    [SerializeField] private BoardManager boardManager;

    public UnityEvent onDragStart;
    public UnityEvent onDragEnd;

    private void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position;

        // If not assigned in inspector, try to find BoardManager
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
    }

    private void OnMouseDown()
    {
        if (!isDraggable) return;

        onDragStart?.Invoke();
        StartCoroutine(DragRoutine());
    }

    private IEnumerator DragRoutine()
    {
        while (Input.GetMouseButton(0))
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            targetPosition = GetSnappedPosition(mousePosition);

            // Smooth movement
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );

            yield return null;
        }

        // When released, get the column index and place block
        int columnIndex = GetCurrentColumnIndex();
        boardManager.PlaceBlockInColumn(columnIndex, gameObject);
        isDraggable = false;
        onDragEnd?.Invoke();
    }

    private Vector3 GetSnappedPosition(Vector3 mousePosition)
    {
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        // Using squared distance to avoid square root calculations
        for (int i = 0; i < columnPositionsX.Length; i++)
        {
            float distSquared = (mousePosition.x - columnPositionsX[i]) * (mousePosition.x - columnPositionsX[i]);
            if (distSquared < closestDistance)
            {
                closestDistance = distSquared;
                closestIndex = i;
            }
        }
        //Debug.Log(closestIndex);
        return new Vector3(columnPositionsX[closestIndex], defaultY, 0f);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    private int GetCurrentColumnIndex()
    {
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < columnPositionsX.Length; i++)
        {
            float distSquared = (transform.position.x - columnPositionsX[i]) *
                                (transform.position.x - columnPositionsX[i]);
            if (distSquared < closestDistance)
            {
                closestDistance = distSquared;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

}