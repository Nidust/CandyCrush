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
    [SerializeField] private Vector2Int[] DisablePositions;

    public static GameBoard Instance;
    private List<List<Tile>> Rows;
    private Bounds BoardBounds;
    private Bounds FruitsBounds;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

        BoardBounds = BoardTemplate.GetComponent<SpriteRenderer>().bounds;
        FruitsBounds = FruitsTemplate.GetComponentInChildren<SpriteRenderer>().bounds;

        CreateBoard();
    }

    #region Create

    void CreateBoard()
    {
        Vector2 startPosition = BoardBounds.min + new Vector3(Padding.x, Padding.y, 0f);
        Vector2 position = startPosition;

        System.Array enumArr = System.Enum.GetValues(typeof(BoardType));

        Rows = new List<List<Tile>>();

        for (int y = 0; y < Size.y; y++)
        {
            List<Tile> cols = new List<Tile>();
            Rows.Add(cols);

            for (int x = 0; x < Size.x; x++)
            {
                Vector2Int coordinate = new Vector2Int(x, y);
                if (DisablePositions.Contains(coordinate))
                {
                    cols.Add(null);
                }
                else
                {
                    CreateRandomTile(cols, enumArr, position, coordinate);
                }

                position.x += FruitsBounds.extents.x + Margin.x;
            }

            position.x = startPosition.x;
            position.y += FruitsBounds.extents.y + Margin.y;
        }
    }

    void CreateRandomTile(List<Tile> columns, System.Array enumArr, Vector2 position, Vector2Int coordinate)
    {
        BoardType type = (BoardType)enumArr.GetValue(Random.Range(1, enumArr.Length));
        if (type == BoardType.None)
        {
            return;
        }

        GameBoardElement element = ElementsEachType.SingleOrDefault(x => x.Type == type);
        if (element == null)
        {
            return;
        }

        Tile newTile = Instantiate(element.Prefab, position, Quaternion.identity, transform).GetComponent<Tile>();
        newTile.TileType = type;
        newTile.Position = coordinate;
        newTile.MovePosition = position;
        columns.Add(newTile);
    }

    #endregion

    #region Public

    public void SwapTile(Vector2Int lhs, Vector2Int rhs)
    {
        Tile tile1 = Rows[lhs.y][lhs.x];
        Tile tile2 = Rows[rhs.y][rhs.x];

        if (tile1.InProgress || tile2.InProgress)
        {
            return;
        }

        Vector2 temp = tile1.transform.position;
        Vector2Int positionTemp = tile1.Position;

        tile1.Position = tile2.Position;
        tile1.MovePosition = tile2.transform.position;

        tile2.Position = positionTemp;
        tile2.MovePosition = temp;

        Rows[lhs.y][lhs.x] = tile2;
        Rows[rhs.y][rhs.x] = tile1;
    }
    #endregion

    #region Debug
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
                Gizmos.DrawWireCube(position, FruitsBounds.size);

                position.x += FruitsBounds.extents.x + Margin.x;
            }

            position.x = startPosition.x;
            position.y += FruitsBounds.extents.y + Margin.y;
        }
    }
    #endregion
}
