using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.3f;

    private float stepTime;
    private float lockTime;

    private bool activeInput;
    private Vector3 mousePosition;
    private int frame = 0;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;

        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0f;

        if (this.cells == null)
        {
            this.cells = new Vector3Int[this.data.cells.Length];
        }

        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)this.data.cells[i];
        }
    }

    private void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        RuntimePlatform platform = Application.platform;

        if (platform == RuntimePlatform.WindowsPlayer)
        {
            WindowsKeyboardListening();
        }

        if (Time.time >= this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }

    private void WindowsKeyboardListening()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down);
        }

        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            frame++;
            if (frame == 20)
            {
                frame = 0;
                Move(Vector2Int.down);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            this.board.GameOver();
        }
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        Move(Vector2Int.down);

        if (this.lockTime >= this.lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i ++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationDirection * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    public void OnButtonClick(string position)
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if (position == "Up")
        {
            Rotate(1);
        }

        else if (position == "Left")
        {
            Move(Vector2Int.left);
        }

        else if (position == "Right")
        {
            Move(Vector2Int.right);
        }

        else if (position == "Down")
        {
            Move(Vector2Int.down);
        }

        else if (position == "Restart")
        {
            this.board.GameOver();
        }


        if (Time.time >= this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }
}
