using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FinanceTracking.API.DTOs;
public class MlCategoriesResponseDto
{
    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();
}
public record PredictionRequest([property: JsonPropertyName("texts")] List<string> Texts);

public record PredictionResponse([property: JsonPropertyName("results")] List<PredictionResultItem> Results);

public record PredictionResultItem(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("category")] string Category
);