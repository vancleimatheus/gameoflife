using GameOfLifeAPI.Models;

namespace GameOfLifeAPI.Repository;

public interface IBoardRepository
{
    Task SaveAsync(IIdEntity entity);
    Task<IBoard?> GetByIdAsync(Guid id);
}