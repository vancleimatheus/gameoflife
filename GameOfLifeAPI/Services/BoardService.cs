using GameOfLifeAPI.Models;
using GameOfLifeAPI.Repository;

namespace GameOfLifeAPI.Services;

public interface IBoardService
{
    Task<IBoard?> SaveBoardAsync(string boardContent);
    Task<IBoard?> GetBoardAsync(Guid boardId);
    Task<IBoard?> ProcessNextGenerationAsync(Guid boardId, int iterations, bool stopAtStableState);
}

public class BoardService(
        IBoardProcessingService processingService,
        IBoardRepository repository) : IBoardService
{

    public async Task<IBoard?> SaveBoardAsync(string boardContent)
    {
        if (!IsValid(boardContent))
            return null;

        var board = new Board { Content = boardContent };
        await repository.SaveAsync(board);
        return board;
    }

    public async Task<IBoard?> GetBoardAsync(Guid boardId)
    {
        return await repository.GetByIdAsync(boardId);
    }

    public async Task<IBoard?> ProcessNextGenerationAsync(Guid boardId, int iterations, bool stopAtStableState)
    {
        var board = await GetBoardAsync(boardId);
        if (board == null) return null;

        var memoryBoard = processingService.ConvertToMemoryBoard(board.Content);

        var previousState = "";

        for (int i = 0; i < iterations; i++)
        {
            previousState = board.Content;
            memoryBoard = processingService.ComputeNextGeneration(memoryBoard);
            board.Content = processingService.ConvertToString(memoryBoard);

            if (stopAtStableState && previousState == board.Content)
                break;
        }

        //Didn't reach final state in the specified cycles, return null and doesn't update the state of the board
        if (stopAtStableState && previousState != board.Content)
            return null;

        await repository.SaveAsync(board);
        return board;
    }

    //Boards should have all rows with the same size and at least 2
    private static bool IsValid(string boardContent)
    {
        var rows = boardContent.Split('|');
        if (rows.Length < 2) return false;

        var rowLength = rows[0].Length;
        return rows.All(row => row.Length == rowLength);
    }
}

