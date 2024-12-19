using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using AutoMapper;

namespace GameOfLifeAPI.Models;

public interface IBoard : IIdEntity
{
    public string Content { get; set; }
}

public class BoardDTO : IBoard, IIdEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = "";
}

public class Board : IBoard, IIdEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = "";
}

public class MongoBoard : IBoard, IIdEntity
{
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public string Content { get; set; } = "";
}

public class BoardProfile : Profile
{
    public BoardProfile()
    {
        CreateMap<IBoard, BoardDTO>();
        CreateMap<Board, MongoBoard>();
    }
}