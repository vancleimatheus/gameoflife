using GameOfLifeAPI.Models;
using GameOfLifeAPI.Repository;
using System.Text;

namespace GameOfLifeAPI.Services;

public interface IBoardService
{
    Task<IBoard?> SaveBoard(string board);
    Task<IBoard?> ProcessNextGeneration(Guid id, int iterations, bool finalState);
    Task<IBoard?> GetBoard(Guid Id);
}

public class BoardService(IRepository repository, ILogger<BoardService> logger) : IBoardService
{
    public async Task<IBoard?> SaveBoard(string board)
    {
        if (!IsBoardValid(board))
            return null;

        var boardToSave = new Board { Content = board };
        await repository.SaveAsync(boardToSave);

        return boardToSave;
    }

    private static bool IsBoardValid(string board)
    {
        string[] rows = board.Split("|");

        //Board has to have at least 2 rows
        if (rows.Length < 2)
            return false;

        int firstRowLength = rows[0].Length;

        //All rows should have the same size
        return !rows.Any(r => r.Length != firstRowLength);
    }

    public async Task<IBoard?> GetBoard(Guid boardId)
    {
        return await repository.GetBoardByIdAsync(boardId);
    }

    public async Task<IBoard?> ProcessNextGeneration(Guid id, int iterations, bool finalState)
    {
        IBoard? board = await GetBoard(id);

        if (board == null)
            return null;

        string currentState = "";

        var memoryBoard = ConvertToMemoryList(board.Content);

        for (int i = 0; i < iterations; i++)
        {
            currentState = board.Content;

            GetNextGen(memoryBoard);
            board.Content = ConvertBoardToString(memoryBoard);

            if (finalState && currentState == board.Content)
                break;

        }

        //Didn't reach final state in the specified cycles, return null and doesn't update the state of the board
        if (finalState && currentState != board.Content)
            return null;


        //Saves new board's state to DB
        await repository.SaveAsync(board);

        return await Task.FromResult(board);

    }

    private void GetNextGen(List<List<Cell>> memoryBoard)
    {
        var cells = memoryBoard.SelectMany(row => row).ToList();

        cells.ForEach(cell => cell.NextState());
        cells.ForEach(cell => cell.CommitState());
    }

    private static string ConvertBoardToString(List<List<Cell>> memoryBoard)
    {
        var sbContent = new StringBuilder();

        foreach (var row in memoryBoard)
        {
            sbContent.Append(string.Join("", row.Select(cell => cell.State ? "*" : "-")));
            sbContent.Append('|');
        }

        // Remove the last redundant '|'
        if (sbContent.Length > 0)
            sbContent.Length--;

        return sbContent.ToString();
    }

    private List<List<Cell>> ConvertToMemoryList(string board)
    {
        List<List<Cell>> referencedBoard = [];

        string[] rows = board.Split("|");

        foreach (string row in rows)
        {
            referencedBoard.Add(CreateCellsFromString(row));
        }

        //link everything
        for (int i = 1; i < referencedBoard.Count; i++)
        {
            List<Cell> currentRow = referencedBoard[i];
            List<Cell> previousRow = referencedBoard[i - 1];

            for (int j = 0; j < currentRow.Count; j++)
            {
                linkCells(currentRow[j], previousRow[j]);

                if (j > 0)
                {
                    linkCells(currentRow[j], previousRow[j - 1]);
                }

                if (j < currentRow.Count - 1)
                {
                    linkCells(currentRow[j], previousRow[j + 1]);
                }
            }
        }

        return referencedBoard;
    }

    private void linkCells(Cell firstCell, Cell secondCell)
    {
        firstCell.Neighbours.Add(secondCell);
        secondCell.Neighbours.Add(firstCell);
    }

    private List<Cell> CreateCellsFromString(string row)
    {
        var cells = new List<Cell>();

        Cell? newCell = null;
        Cell? previousCell;

        for (int i = 0; i < row.Length; i++)
        {
            previousCell = i > 0 ? newCell : null;
            newCell = new(row[i] == '*');

            //If there is a previous cell, link it to the new cell
            if (previousCell != null)
            {
                newCell.Neighbours.Add(previousCell);
                previousCell.Neighbours.Add(newCell);
            }

            cells.Add(newCell);

        }

        return cells;
    }
}

internal class Cell(bool initialState)
{
    public bool State { get; private set; } = initialState;
    public bool OldState { get; set; } = initialState;
    public List<Cell> Neighbours { get; set; } = [];

    public void NextState()
    {
        var liveNeighbours = Neighbours.Count(n => n.OldState);

        //Exact 3 neighbours alive, it will always be alive
        if (liveNeighbours == 3)
            State = true;

        //Less than 2 or more than 3 neighbours alive, the cell will die
        if (liveNeighbours < 2 || liveNeighbours > 3)
            State = false;

        //If none of those previous conditions were met state remains unchanged
    }

    public void CommitState()
    {
        OldState = State;
    }
}