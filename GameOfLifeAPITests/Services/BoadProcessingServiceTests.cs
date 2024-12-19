using GameOfLifeAPI.Models;

namespace GameOfLifeAPITests.Services;

public class BoardProcessingServiceTests
{
    private readonly BoardProcessingService _boardProcessingService;

    public BoardProcessingServiceTests()
    {
        _boardProcessingService = new BoardProcessingService();
    }

    [Fact]
    public void ConvertToMemoryBoard_ShouldReturnCorrectStructure()
    {
        // Arrange
        string boardContent = "*-*|--*";

        // Act
        var memoryBoard = _boardProcessingService.ConvertToMemoryBoard(boardContent);

        // Assert
        Assert.NotNull(memoryBoard);
        Assert.Equal(2, memoryBoard.Count); // Two rows
        Assert.Equal(3, memoryBoard[0].Count); // Three columns in the first row
        Assert.Equal(3, memoryBoard[1].Count); // Three columns in the second row
    }

    [Fact]
    public void ConvertToMemoryBoard_ShouldLinkCellsCorrectly()
    {
        // Arrange
        string boardContent = "*-*|--*";

        // Act
        var memoryBoard = _boardProcessingService.ConvertToMemoryBoard(boardContent);

        // Assert
        var firstCell = memoryBoard[0][0];
        Assert.NotEmpty(firstCell.Neighbours);
        Assert.Contains(memoryBoard[0][1], firstCell.Neighbours); // Neighbor in the same row
        Assert.Contains(memoryBoard[1][0], firstCell.Neighbours); // Neighbor in the next row
    }

    [Fact]
    public void ConvertToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var memoryBoard = new List<List<Cell>>
    {
        new List<Cell> { new Cell(true), new Cell(false), new Cell(true) },
        new List<Cell> { new Cell(false), new Cell(false), new Cell(true) }
    };

        // Act
        var result = _boardProcessingService.ConvertToString(memoryBoard);

        // Assert
        Assert.Equal("*-*|--*", result);
    }

}
