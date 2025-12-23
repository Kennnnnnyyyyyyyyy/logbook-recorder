namespace Backend.Core.DTOs;

public record ExportDraftDto(
    Guid DraftId,
    string ExportPath
);
