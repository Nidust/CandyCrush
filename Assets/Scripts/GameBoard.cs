using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BoardType
{
    None,
    Apple,
    Banana,
    Blueberry,
    Grapes,
    Orange,
    Pear,
    Strawberry
}

[System.Serializable]
public class GameBoardElement
{
    public BoardType Type;
    public GameObject Prefab;
}

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Vector2 Size;
    [SerializeField] private Vector2 Margin;
    [SerializeField] private Vector2 Padding;
    [SerializeField] private GameObject BoardTemplate;
    [SerializeField] private GameObject FruitsTemplate;
    [SerializeField] private GameBoardElement[] ElementsEachType;

    private List<List<BoardType>> Elements;
    private Bounds BoardBounds;
    private Bounds FruitsBounds;

    // Start is called before the first frame update
    void Start()
    {
        BoardBounds = BoardTemplate.GetComponent<SpriteRenderer>().bounds;
        FruitsBounds = FruitsTemplate.GetComponentInChildren<SpriteRenderer>().bounds;

        CreateBoard();
        CreateBoardElements();
    }

    void CreateBoard()
    {
        Elements = new List<List<BoardType>>();

        for (int y = 0; y < Size.y; y++)
        {
            List<BoardType> rows = new List<BoardType>();

            for (int x = 0; x < Size.x; x++)
            {
                rows.Add(BoardType.Apple);
            }

            Elements.Add(rows);
        }
    }

    void CreateBoardElements()
    {
        Vector2 startPosition = BoardBounds.min + new Vector3(Padding.x, Padding.y, 0f);
        Vector2 position = startPosition;

        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                BoardType type = Elements[y][x];
                if (type == BoardType.None)
                {
                    continue;
                }

                GameBoardElement element = ElementsEachType.SingleOrDefault(x => x.Type == type);
                if (element == null)
                {
                    continue;
                }

                Instantiate(
                    original: element.Prefab,
                    position: position,
                    rotation: Quaternion.identity,
                    parent: transform
                );

                position.x += FruitsBounds.extents.x + Margin.x;
            }

            position.x = startPosition.x;
            position.y += FruitsBounds.extents.y + Margin.y;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawWireCube(transform.position, BoardBounds.size);
        Gizmos.DrawWireSphere(BoardBounds.min, 0.5f);

        // Draw Elements Cube
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);

        Vector2 startPosition = BoardBounds.min + new Vector3(Padding.x, Padding.y, 0f);
        Vector2 position = startPosition;

        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                BoardType type = Elements[y][x];
                if (type == BoardType.None)
                {
                    continue;
                }

                GameBoardElement element = ElementsEachType.SingleOrDefault(x => x.Type == type);
                if (element == null)
                {
                    continue;
                }

                Gizmos.DrawWireCube(position, FruitsBounds.size);

                position.x += FruitsBounds.extents.x + Margin.x;
            }

            position.x = startPosition.x;
            position.y += FruitsBounds.extents.y + Margin.y;
        }
    }
}
