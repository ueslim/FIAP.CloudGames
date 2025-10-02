using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

public class PaymentsConfirmationProcessor
{
    private readonly ILogger _logger;
    public PaymentsConfirmationProcessor(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PaymentsConfirmationProcessor>();
    }

    [Function("PaymentsConfirmationProcessor")]
    public async Task Run([QueueTrigger("payments-events", Connection = "AzureWebJobsStorage")] string message)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
            if (evt == null) return;
            if (evt.TryGetValue("Type", out var type) && type?.ToString() == "GamePurchased")
            {
                var paymentId = Guid.Parse(evt["PaymentId"]!.ToString()!);
                var connStr = Environment.GetEnvironmentVariable("PaymentsDb") ?? string.Empty;
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                // mark as succeeded
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Payments SET Status = 'Succeeded' WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", paymentId);
                await cmd.ExecuteNonQueryAsync();
                // append PaymentSucceeded event
                var insert = conn.CreateCommand();
                insert.CommandText = "INSERT INTO PaymentEvents (Id, Type, Payload, CreatedAt) VALUES (@id, @type, @payload, SYSUTCDATETIME())";
                insert.Parameters.AddWithValue("@id", Guid.NewGuid());
                insert.Parameters.AddWithValue("@type", "PaymentSucceeded");
                insert.Parameters.AddWithValue("@payload", message);
                await insert.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment confirmation");
            throw;
        }
    }
}
