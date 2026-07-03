using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.DTOs.AI.ValidationReport.AIResult;

public class KeyFindingsDto
{
    [JsonPropertyName("precedent_analysis")]
    public string PrecedentAnalysis { get; set; }

    // هنا حلينا المشكلة بإننا استقبلنا الـ ResourceAssessment كـ JsonElement 
    // عشان لو رجع String أو Array فاضية [] الكود ميعملش Crash ويقبل الاتنين
    [JsonPropertyName("resource_assessment")]
    public JsonElement ResourceAssessment { get; set; }

    [JsonPropertyName("market_trends")]
    public string MarketTrends { get; set; }
}
