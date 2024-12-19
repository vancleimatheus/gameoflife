using MongoDB.Driver;
using AutoMapper;
using GameOfLifeAPI.Models;

namespace GameOfLifeAPI.Repository;

public class BoardRepository : IBoardRepository
{
    private readonly IMongoCollection<MongoBoard> _boardCollection;
    private readonly IMapper mapper;

    public BoardRepository(IMapper mapper)
    {
        var connectionString = "mongodb://host.docker.internal:27017";
        var databaseName = "BoardDatabase";
        var collectionName = "Boards";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _boardCollection = database.GetCollection<MongoBoard>(collectionName);
        this.mapper = mapper;
    }

    public async Task SaveAsync(IIdEntity entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid(); // Generate a new GUID if not provided
        }

        var mongoBoard = mapper.Map<MongoBoard>(entity);

        //Upsert in MongoDB
        await _boardCollection.ReplaceOneAsync(
                doc => doc.Id == mongoBoard.Id,
                mongoBoard,
                new ReplaceOptions { IsUpsert = true });
    }

    public async Task<IBoard?> GetByIdAsync(Guid id)
    {
        var filter = Builders<MongoBoard>.Filter.Eq(b => b.Id, id);
        return await _boardCollection.Find(filter).FirstOrDefaultAsync();
    }
}