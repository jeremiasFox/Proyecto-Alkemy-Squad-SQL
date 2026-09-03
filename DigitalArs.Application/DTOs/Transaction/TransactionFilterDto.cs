using System.Text.Json.Serialization;
using DigitalArs.Domain.Entities;

namespace DigitalArs.Application.DTOs.Transaction;

public class TransactionFilterDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionType? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
