using AutoMapper;
using GameOfLifeAPI.Models;
using GameOfLifeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLifeAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class BoardController(IBoardService boardService, ILogger<BoardController> logger, IMapper mapper) : ControllerBase
{

    /// <summary>
    /// Retrieves a previously saved board
    /// </summary>
    /// <param name="id">id of the board</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get(Guid id)
    {
        if (!ValidId(id))
            return BadRequest("Please enter a valid id");

        IBoard? board = await boardService.GetBoardAsync(id);

        return board != null
            ? Ok(mapper.Map<BoardDTO>(board))
            : BadRequest("Board is invalid, please use | (pipe) to split rows and make all the rows the same size.");

    }

    /// <summary>
    /// Process the next states of a game of life board
    /// </summary>
    /// <param name="id">id of the board</param>
    /// <param name="iterations">How many states should be processed, should be higher than 0. Default is 1</param>
    /// <param name="finalState">Retrieves the board final state, if the final state is not reached after the number of iterations provided, returns a UnprocessableEntity error</param>
    /// <returns></returns>
    [HttpPut(Name = nameof(ProcessNextGeneration))]
    public async Task<IActionResult> ProcessNextGeneration(Guid id, int iterations = 1, bool finalState = false)
    {
        if (!ValidId(id))
            return BadRequest("Please enter a valid id");

        if (iterations < 0)
            return BadRequest();

        var processedBoard = await boardService.ProcessNextGenerationAsync(id, iterations, finalState);

        if (processedBoard == null)
            return new UnprocessableEntityObjectResult($"The board didn't reach a final state in {iterations} iterations and was not updated.");

        return Ok(mapper.Map<BoardDTO>(processedBoard));
    }

    /// <summary>
    /// Saves a new board 
    /// 
    /// The board rows should be separated by  the | (pipe) character and have all the same size
    /// Alive cells should be represented by the * (star) character, dead cells can be anything for the first input
    ///
    /// Example:
    /// --*--|--*--|--*--
    ///
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post(string board)
    {
        if (string.IsNullOrWhiteSpace(board))
            return BadRequest("No board was provided");

        IBoard? savedBoard = await boardService.SaveBoardAsync(board);

        return savedBoard != null
            ? Ok(mapper.Map<BoardDTO>(savedBoard))
            : BadRequest("Board is invalid, please use | (pipe) to split rows and make all the rows the same size.");
    }

    private static bool ValidId(Guid id) => id != Guid.Empty;
}
