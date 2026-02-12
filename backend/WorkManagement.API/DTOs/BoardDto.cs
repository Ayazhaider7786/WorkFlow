namespace WorkManagement.API.DTOs;

public record BoardDto(
    int Id,
    string Name,
    int ProjectId,
    int? OwnerId,
    bool IsDefault,
    IEnumerable<BoardColumnDto> Columns
);

public record BoardColumnDto(
    int Id,
    int BoardId,
    int StatusId,
    string StatusName,
    int Order
);

public record CreateBoardDto(
    string Name,
    bool IsDefault
);

public record AddColumnDto(
    int StatusId
);
