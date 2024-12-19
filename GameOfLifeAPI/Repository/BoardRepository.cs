using MongoDB.Driver;
using AutoMapper;
using GameOfLifeAPI.Models;
using SharpCompress.Common;

namespace GameOfLifeAPI.Repository;

public interface IBoardRepository
{
    Task SaveAsync(IIdEntity entity);
    Task<IBoard?> GetByIdAsync(Guid id);
}

public class BoardRepository : IBoardRepository
{
    private readonly IMongoCollection<MongoBoard> _boardCollection;
    private readonly ILogger<BoardRepository> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public BoardRepository(ILogger<BoardRepository> logger, IMapper mapper, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GOLDatabase");  
        var databaseName = configuration["DatabaseSettings:DatabaseName"];
        var collectionName = "Boards";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _boardCollection = database.GetCollection<MongoBoard>(collectionName);
        this._logger = logger;
        this._mapper = mapper;
        this._configuration = configuration;
    }

    public async Task SaveAsync(IIdEntity entity)
    {
        _logger.LogInformation("Saving board", entity);

        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid(); // Generate a new GUID if not provided
        }

        var mongoBoard = _mapper.Map<MongoBoard>(entity);

        //Upsert in MongoDB
        var result = await _boardCollection.ReplaceOneAsync(
                doc => doc.Id == mongoBoard.Id,
                mongoBoard,
                new ReplaceOptions { IsUpsert = true });

        _logger.LogInformation("Board saved to the dabase", mongoBoard, result);
    }

    public async Task<IBoard?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting board from the database", id);

        var filter = Builders<MongoBoard>.Filter.Eq(b => b.Id, id);
        var result = await _boardCollection.Find(filter).FirstOrDefaultAsync();

        _logger.LogInformation("Board read from the database", result);

        return result;
    }
}