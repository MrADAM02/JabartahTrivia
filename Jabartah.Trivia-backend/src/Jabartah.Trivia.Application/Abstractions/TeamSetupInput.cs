namespace Jabartah.Trivia.Application.Abstractions;

// Shared across all 4 Create*GameSessionCommand records -- unlike the Team entities
// themselves (deliberately duplicated per aggregate), this is a pure wire-format input
// row with no mode-specific meaning, so one shared type is appropriate here.
public record TeamSetupInput(string Name, string? Color, string? Icon);
