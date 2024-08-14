using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Sprite parentSprite;
    public Sprite[] childSprites;
    public int rows = 3;
    public int columns = 3;
    public float spacing = 2f;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.red;
    public float overlapThreshold = 0.3f; // 30% overlap threshold

    private GameObject[,] gridCells;

    private void Start()
    {
        CreateGrid();
        ProcessGrid();
    }

    void CreateGrid()
    {
        gridCells = new GameObject[columns, rows];

        // Choose a random cell for the double-sized child
        int randomX = Random.Range(0, columns);
        int randomY = Random.Range(0, rows);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 position = new Vector2(x * spacing, y * spacing);
                gridCells[x, y] = CreateGridCell(position, x, y, x == randomX && y == randomY);
            }
        }
    }

    GameObject CreateGridCell(Vector2 position, int x, int y, bool isDoubleSizedCell)
    {
        GameObject cellObject = new GameObject($"Cell_{x}_{y}");
        cellObject.transform.position = position;
        cellObject.transform.SetParent(transform);

        GameObject parentObject = new GameObject("ParentSprite");
        parentObject.transform.SetParent(cellObject.transform);
        parentObject.transform.localPosition = Vector3.zero;
        SpriteRenderer parentRenderer = parentObject.AddComponent<SpriteRenderer>();
        parentRenderer.sprite = parentSprite;
        parentRenderer.color = defaultColor;
        parentRenderer.sortingOrder = 0;

        AddRandomChild(parentObject, isDoubleSizedCell);

        return cellObject;
    }

    void AddRandomChild(GameObject parent, bool isDoubleSized)
    {
        if (childSprites.Length == 0) return;

        int randomIndex = Random.Range(0, childSprites.Length);
        Sprite childSprite = childSprites[randomIndex];

        GameObject childObject = new GameObject("ChildSprite");
        childObject.transform.SetParent(parent.transform);
        childObject.transform.localPosition = Vector3.zero;
        SpriteRenderer childRenderer = childObject.AddComponent<SpriteRenderer>();
        childRenderer.sprite = childSprite;
        childRenderer.sortingOrder = 1;

        // Randomize child scale
        float parentScaleY = parent.transform.localScale.y;
        float randomScale = isDoubleSized ? parentScaleY * 2f : Random.Range(parentScaleY * 0.5f, parentScaleY);

        childObject.transform.localScale = Vector2.one * randomScale;
    }

    void ProcessGrid()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject currentCell = gridCells[x, y];
                GameObject parentObject = currentCell.transform.GetChild(0).gameObject;
                GameObject childObject = parentObject.transform.GetChild(0).gameObject;

                // Get Bounds (only considering y-axis for child)
                Bounds parentBounds = parentObject.GetComponent<Renderer>().bounds;
                Bounds childBounds = new Bounds(childObject.transform.position, new Vector3(parentBounds.size.x, childObject.GetComponent<Renderer>().bounds.size.y, childObject.GetComponent<Renderer>().bounds.size.z));

                // Check if child bounds extend beyond parent bounds (only on y-axis)
                if (childBounds.min.y < parentBounds.min.y ||
                    childBounds.max.y > parentBounds.max.y)
                {
                    HighlightCell(parentObject);
                    HighlightNearbyOverlappingParents(x, y, childObject, parentBounds.size.x);
                }
            }
        }
    }

    void HighlightNearbyOverlappingParents(int x, int y, GameObject overlappingChildObject, float parentWidth)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(x + 1, y),
            new Vector2Int(x - 1, y),
            new Vector2Int(x, y + 1),
            new Vector2Int(x, y - 1)
        };

        foreach (Vector2Int neighbor in neighbors)
        {
            if (IsValidCell(neighbor.x, neighbor.y))
            {
                GameObject neighborCell = gridCells[neighbor.x, neighbor.y];
                GameObject neighborParent = neighborCell.transform.GetChild(0).gameObject;
                GameObject neighborChild = neighborParent.transform.GetChild(0).gameObject;

                // Calculate overlap area (considering only y-axis and fixed parent width)
                Bounds modifiedChildBounds = new Bounds(overlappingChildObject.transform.position, new Vector3(parentWidth, overlappingChildObject.GetComponent<Renderer>().bounds.size.y, overlappingChildObject.GetComponent<Renderer>().bounds.size.z));
                float overlapArea = CalculateOverlapArea(modifiedChildBounds, neighborParent.GetComponent<Renderer>().bounds);

                // Calculate overlap percentage relative to neighbor parent's area
                float neighborParentArea = neighborParent.GetComponent<Renderer>().bounds.size.x * neighborParent.GetComponent<Renderer>().bounds.size.y;
                float overlapPercentage = overlapArea / neighborParentArea;

                // Check if overlap exceeds the threshold
                if (overlapPercentage >= overlapThreshold)
                {
                    HighlightCell(neighborParent);

                    // Remove the neighbor's child object
                    Destroy(neighborChild);
                }
            }
        }
    }

    float CalculateOverlapArea(Bounds rect1, Bounds rect2)
    {
        float xOverlap = Mathf.Max(0, Mathf.Min(rect1.max.x, rect2.max.x) - Mathf.Max(rect1.min.x, rect2.min.x));
        float yOverlap = Mathf.Max(0, Mathf.Min(rect1.max.y, rect2.max.y) - Mathf.Max(rect1.min.y, rect2.min.y));
        return xOverlap * yOverlap;
    }

    bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }

    void HighlightCell(GameObject parentObject)
    {
        SpriteRenderer parentRenderer = parentObject.GetComponent<SpriteRenderer>();
        if (parentRenderer != null)
        {
            parentRenderer.color = highlightColor;
        }
    }
}