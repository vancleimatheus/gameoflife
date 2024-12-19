using GameOfLifeAPI.Models;

public interface IBoardProcessingService
{
    List<List<Cell>> ConvertToMemoryBoard(string boardContent);
    List<List<Cell>> ComputeNextGeneration(List<List<Cell>> memoryBoard);
    string ConvertToString(List<List<Cell>> memoryBoard);
}

public class BoardProcessingService : IBoardProcessingService
{
    public List<List<Cell>> ConvertToMemoryBoard(string boardContent)
    {
        var rows = boardContent.Split('|').Select(CreateCellsFromString).ToList();
        for (int i = 1; i < rows.Count; i++) LinkRows(rows[i - 1], rows[i]);
        return rows;
    }

    public List<List<Cell>> ComputeNextGeneration(List<List<Cell>> memoryBoard)
    {
        var cells = memoryBoard.SelectMany(row => row).ToList();
        cells.ForEach(cell => cell.NextState());
        cells.ForEach(cell => cell.CommitState());
        return memoryBoard;
    }

    public string ConvertToString(List<List<Cell>> memoryBoard)
    {
        return string.Join("|", memoryBoard.Select(row =>
            string.Concat(row.Select(cell => cell.State ? "*" : "-"))));
    }

    //Reads a string and translates it to a list of cells
    private static List<Cell> CreateCellsFromString(string row)
    {
        var cells = new List<Cell>();
        Cell? previous = null;

        foreach (var charState in row)
        {
            var cell = new Cell(charState == '*');

            //links the current cell to the previous one in this row
            previous?.Link(cell);

            cells.Add(cell);
            previous = cell;
        }

        return cells;
    }

    private static void LinkRows(List<Cell> previousRow, List<Cell> currentRow)
    {
        for (int j = 0; j < currentRow.Count; j++)
        {

            currentRow[j].Link(previousRow[j]);
            if (j > 0) currentRow[j].Link(previousRow[j - 1]);
            if (j < currentRow.Count - 1) currentRow[j].Link(previousRow[j + 1]);
        }
    }
}
