using UnityEngine;

public class Tile : MonoBehaviour
{
    private static Tile Selected;
    private SpriteRenderer Renderer;

    public BoardType TileType;
    public Vector2Int Position;
    public Vector2 MovePosition;
    public bool InProgress = false;

    // Start is called before the first frame update
    void Start()
    {
        Renderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, MovePosition) >= 0.0001f)
        {
            InProgress = true;
            transform.position = Vector2.MoveTowards(transform.position, MovePosition, 10f * Time.deltaTime);
        }
        else
        {
            InProgress = false;
            transform.position = MovePosition;
        }
    }

    void Select()
    {
        Renderer.color = Color.grey;
    }

    void Unselect()
    {
        Renderer.color = Color.white;
    }

    private void OnMouseDown()
    {
        if (Selected != null)
        {
            if (Selected == this)
                return;

            Selected.Unselect();

            if (Vector2Int.Distance(Selected.Position, Position) == 1)
            {
                GameBoard.Instance.SwapTile(Position, Selected.Position);
                Selected = null;
            }
            else
            {
                Select();
                Selected = this;
            }
        }
        else
        {
            Select();
            Selected = this;
        }
    }
}
