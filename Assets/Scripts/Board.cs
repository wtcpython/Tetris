using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {  get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominos ;
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public Vector2Int boardSize = new Vector2Int(10, 20);

    public TextMeshProUGUI scoreUI;
    public TextMeshProUGUI bestScoreUI;
    private int score = 0;
    private int bestScore = 0;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        for (int i = 0; i < tetrominos.Length; i++)
        {
            this.tetrominos[i].Initialize();
        }
    }

    private void Start()
    {
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);

        SpawnPiece();
        bestScore = PlayerPrefs.GetInt("BestScore", 0);

        SetScore();
    }

    private void OnApplicationQuit()
    {
        SaveBestScore();
    }

    private void SaveBestScore()
    {
        if (score > bestScore)
        {
            bestScore = score;
        }

        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.Save();
    }

    private void SetScore()
    {
        scoreUI.text = $"Score: {score}";

        SaveBestScore();
        if (bestScore > 0)
        {
            bestScoreUI.text = $"Best: {bestScore}";
        }

        float minDelay = 0.3f;

        float stepDelay = this.activePiece.stepDelay;

        if (score < 50)
        {
            stepDelay = 1 - score / 100.0f;
        }
        else
        {
            stepDelay = minDelay + (0.5f - minDelay) * 50.0f / score;
        }

        this.activePiece.stepDelay = stepDelay;
    }

    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominos.Length);
        TetrominoData data = this.tetrominos[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        this.tilemap.ClearAllTiles();
        if (score > bestScore)
        {
            bestScore = score;
        }
        score = 0;
        SetScore();

        // ...
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length;i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                score++;
                SetScore();
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }
}
