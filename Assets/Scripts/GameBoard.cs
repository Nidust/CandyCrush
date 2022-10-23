using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private int Rows;
    [SerializeField] private int Columns;
    [SerializeField] private Vector2 Margin;
    [SerializeField] private Vector2 Padding;
    [SerializeField] private GameObject BoardTemplate;
    [SerializeField] private GameObject TileTemplate;
    [SerializeField] private Sprite[] FruitsList;
    [SerializeField] private Vector2Int[] DisablePositions;

    [SerializeField] private int StartingMove;
    [SerializeField] private TextMeshProUGUI MovesText;
    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private GameObject GameOverMenu;

    public static GameBoard Instance { get; private set; }
    private Tile[,] Grid;
    private Bounds BoardBounds;
    private Bounds FruitsBounds;

    private int _score;
    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
            ScoreText.text = _score.ToString();
        }
    }

    private int _numMoves;
    public int NumMoves
    {
        get { return _numMoves; }
        set
        {
            _numMoves = value;
            MovesText.text = _numMoves.ToString();
        }
    }

    private void Awake()
    {
        Instance = this;

        BoardBounds = BoardTemplate.GetComponent<SpriteRenderer>().bounds;
        FruitsBounds = TileTemplate.GetComponentInChildren<SpriteRenderer>().bounds;

        Score = 0;
        NumMoves = StartingMove;
    }

    private void Start()
    {
        Grid = new Tile[Columns, Rows];
        GameOverMenu.SetActive(false);

        InitGrid();
    }

    #region Create

    void InitGrid()
    {
        Vector2 startPosition = BoardBounds.min + new Vector3(Padding.x, Padding.y, 0f);
        Vector2 position = startPosition;

        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Vector2Int coordinate = new Vector2Int(column, row);
                if (DisablePositions.Contains(coordinate))
                {
                    Grid[column, row] = null;
                }
                else
                {
                    GameObject newTile = Instantiate(TileTemplate, position, Quaternion.identity, transform);

                    SpriteRenderer renderer = newTile.GetComponentInChildren<SpriteRenderer>();
                    renderer.sprite = FruitsList[Random.Range(0, FruitsList.Length)];

                    Tile tile = newTile.GetComponent<Tile>();
                    tile.Position = new Vector2Int(column, row);

                    Grid[column, row] = tile;
                }

                position.x += FruitsBounds.extents.x + Margin.x;
            }

            position.x = startPosition.x;
            position.y += FruitsBounds.extents.y + Margin.y;
        }
    }
    
    #endregion

    #region Game Logic

    public void SwapTile(Vector2Int lhs, Vector2Int rhs)
    {
        Tile tile1 = Grid[lhs.x, lhs.y];
        Tile tile2 = Grid[rhs.x, rhs.y];
        
        Sprite temp = tile1.Renderer.sprite;
        tile1.Renderer.sprite = tile2.Renderer.sprite;
        tile2.Renderer.sprite = temp;

        bool isMatches = CheckMatches();
        if (isMatches == false)
        {
            // Rollback
            temp = tile1.Renderer.sprite;
            tile1.Renderer.sprite = tile2.Renderer.sprite;
            tile2.Renderer.sprite = temp;
        }
        else
        {
            --NumMoves;

            do
            {
                FillHoles();
            } while (CheckMatches());

            if (NumMoves <= 0)
            {
                NumMoves = 0;
                GameOver();
            }
        }
    }

    public bool CheckMatches()
    {
        HashSet<Tile> matchedTiles = new HashSet<Tile>();

        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Tile currentTile = Grid[column, row];
                if (currentTile == null)
                {
                    continue;
                }

                List<Tile> horizontalMatches = FindColumnMatchForTile(row, column, currentTile);
                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(currentTile);
                }

                List<Tile> verticalMatches = FindRowMatchForTile(row, column, currentTile);
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(currentTile);
                }
            }
        }

        foreach (Tile tile in matchedTiles)
        {
            tile.Renderer.sprite = null;
        }

        Score += matchedTiles.Count;

        return matchedTiles.Count > 0;
    }

    void FillHoles()
    {
        for (int column = 0; column < Rows; column++)
        {
            for (int row = 0; row < Columns; row++)
            {
                if (Grid[column, row] == null)
                {
                    continue;
                }

                while (GetSpriteRendererAt(column, row).sprite == null)
                {
                    SpriteRenderer current = GetSpriteRendererAt(column, row);
                    SpriteRenderer next = current;

                    for (int filler = row; filler < Rows - 1; filler++)
                    {
                        if (Grid[column, filler + 1] == null)
                        {
                            continue;
                        }

                        next = GetSpriteRendererAt(column, filler + 1);
                        current.sprite = next.sprite;
                        current = next;
                    }

                    next.sprite = FruitsList[Random.Range(0, FruitsList.Length)];
                }
            }
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        PlayerPrefs.SetInt("score", Score);
        GameOverMenu.SetActive(true);
    }

    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= Columns || row < 0 || row >= Rows)
            return null;

        Tile tile = Grid[column, row];
        if (tile == null)
            return null;

        return tile.Renderer;
    }

    public List<Tile> FindColumnMatchForTile(int row, int column, Tile currentTile)
    {
        List<Tile> matchTiles = new List<Tile>();

        for (int i = column + 1; i < Columns; i++)
        {
            Tile nextTile = Grid[i, row];
            if (nextTile == null)
            {
                continue;
            }

            if (nextTile.Renderer.sprite != currentTile.Renderer.sprite)
            {
                break;
            }

            matchTiles.Add(nextTile);
        }

        return matchTiles;
    }

    public List<Tile> FindRowMatchForTile(int row, int column, Tile currentTile)
    {
        List<Tile> matchTiles = new List<Tile>();

        for (int i = row + 1; i < Rows; i++)
        {
            Tile nextTile = Grid[column, i];
            if (nextTile == null)
            {
                continue;
            }

            if (nextTile.Renderer.sprite != currentTile.Renderer.sprite)
            {
                break;
            }

            matchTiles.Add(nextTile);
        }

        return matchTiles;
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

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
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
