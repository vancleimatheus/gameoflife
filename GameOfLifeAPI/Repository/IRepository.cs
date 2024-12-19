using GameOfLifeAPI.Models;

namespace GameOfLifeAPI.Repository;

public interface IRepository
{
    Task SaveAsync(IIdEntity entity);
    Task<IBoard?> GetBoardByIdAsync(Guid id);
}