using GameOfLifeAPI.Models;
using GameOfLifeAPI.Repository;
using GameOfLifeAPI.Services;
using Moq;

namespace GameOfLifeAPITests.Services;

public class BoardServiceTests
{
    private readonly Mock<IBoardRepository> _repositoryMock;
    private readonly BoardService _boardService;

    public BoardServiceTests()
    {
        _repositoryMock = new Mock<IBoardRepository>();
        _boardService = new BoardService(new BoardProcessingService(), _repositoryMock.Object);
    }

    // Helper method to create a sample board
    private Board CreateSampleBoard(string content) => new Board
    {
        Id = Guid.NewGuid(),
        Content = content
    };

    [Fact]
    public async Task SaveBoardAsync_ShouldReturnNull_WhenBoardIsInvalid()
    {
        // Arrange
        string invalidBoard = "*|**|";

        // Act
        var result = await _boardService.SaveBoardAsync(invalidBoard);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<IBoard>()), Times.Never);
    }

    [Fact]
    public async Task SaveBoardAsync_ShouldSaveBoard_WhenBoardIsValid()
    {
        // Arrange
        string validBoard = "*-*|--*";
        var board = CreateSampleBoard(validBoard);

        // Act
        var result = await _boardService.SaveBoardAsync(validBoard);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validBoard, result?.Content);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.Is<IBoard>(b => b.Content == validBoard)), Times.Once);
    }

    [Fact]
    public async Task GetBoardAsync_ShouldReturnNull_WhenBoardDoesNotExist()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync((IBoard?)null);

        // Act
        var result = await _boardService.GetBoardAsync(boardId);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task GetBoardAsync_ShouldReturnBoard_WhenBoardExists()
    {
        // Arrange
        var board = CreateSampleBoard("*-*|--*");
        var boardId = board.Id;
        _repositoryMock.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(board);

        // Act
        var result = await _boardService.GetBoardAsync(boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(boardId, result?.Id);
        Assert.Equal(board.Content, result?.Content);
        _repositoryMock.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
    }

    [Fact]
    public async Task ProcessNextGenerationAsync_ShouldReturnNull_WhenBoardDoesNotExist()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync((IBoard?)null);

        // Act
        var result = await _boardService.ProcessNextGenerationAsync(boardId, 3, false);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<IBoard>()), Times.Never);
    }

    [Fact]
    public async Task ProcessNextGenerationAsync_ShouldProcessBoard_WhenIterationsAreGiven()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var initialBoard = "-----|-***-|-----";
        var nextBoard = "--*--|--*--|--*--";
        var board = CreateSampleBoard(initialBoard);

        _repositoryMock.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(board);

        // Act
        var result = await _boardService.ProcessNextGenerationAsync(boardId, 1, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(nextBoard, result?.Content);
        _repositoryMock.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.Is<IBoard>(b => b.Content == nextBoard)), Times.Once);
    }

    [Fact]
    public async Task ProcessNextGenerationAsync_ShouldReturnNull_WhenFinalStateNotReached()
    {
        // Arrange
        var initialBoard = "------|--***-|------";
        var board = CreateSampleBoard(initialBoard);
        var boardId = board.Id;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(board);

        // Act
        var result = await _boardService.ProcessNextGenerationAsync(boardId, 10, true);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(repo => repo.GetByIdAsync(boardId), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveAsync(It.IsAny<IBoard>()), Times.Never);
    }

}