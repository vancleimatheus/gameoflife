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

    //Board is valid if all rows have the same length and has more than 1 rows
    private static bool IsBoardValid(string board)
    {
        var rows = board.Split('|');
        return rows.Length >= 2 && rows.All(row => row.Length == rows[0].Length);
    }

    public async Task<IBoard?> GetBoard(Guid boardId) => await repository.GetBoardByIdAsync(boardId);

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

            GetNextState(memoryBoard);
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

    private void GetNextState(List<List<Cell>> memoryBoard)
    {
        var cells = memoryBoard.SelectMany(row => row).ToList();

        cells.ForEach(cell => cell.NextState());
        cells.ForEach(cell => cell.CommitState());
    }

    private static string ConvertBoardToString(List<List<Cell>> memoryBoard) =>
        string.Join("|", memoryBoard.Select(row =>
            string.Concat(row.Select(cell => cell.State ? "*" : "-"))));

    private List<List<Cell>> ConvertToMemoryList(string board)
    {
        var rows = board.Split('|').Select(CreateCellsFromString).ToList();

        for (int i = 1; i < rows.Count; i++)
            LinkRows(rows[i - 1], rows[i]);

        return rows;
    }

    private static void LinkRows(List<Cell> previousRow, List<Cell> currentRow)
    {
        for (int j = 0; j < currentRow.Count; j++)
        {
            currentRow[j].Link(previousRow[j]);
            //LinkCells(currentRow[j], previousRow[j]);

            if (j > 0) currentRow[j].Link(previousRow[j - 1]);
            if (j < currentRow.Count - 1) currentRow[j].Link(previousRow[j + 1]);
        }
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

    public void Link(Cell neighbour)
    {
        Neighbours.Add(neighbour);
        neighbour.Neighbours.Add(this);
    }

    public void CommitState() => OldState = State;
    
}