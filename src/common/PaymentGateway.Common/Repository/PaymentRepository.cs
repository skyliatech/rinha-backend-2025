using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PaymentGateway.Common.Model;
using System.Data;

namespace PaymentGateway.Common.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task InsertAsync(Payment payment)
        {
            const string sql = @"
            INSERT INTO payments (
                correlation_id, amount, created_at, requested_at,
                processor_used, status, processed_at, processed
            )
            VALUES (
                @CorrelationId, @Amount, @CreatedAt, @RequestedAt,
                @ProcessorUsed, @Status, @ProcessedAt, @Processed
            );";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, payment);
        }

        public async Task UpdateAfterProcessingAsync(string correlationId, string processorUsed, StatusPayment status)
        {
            const string sql = @"
            UPDATE payments
            SET processor_used = @ProcessorUsed,
                status = @Status,
                processed = true,
                processed_at = @ProcessedAt
            WHERE correlation_id = @CorrelationId;";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                CorrelationId = correlationId,
                ProcessorUsed = processorUsed,
                Status = (int)status,
                ProcessedAt = DateTime.UtcNow
            });
        }

        public async Task<PaymentsSummaryAggregate> GetSummaryAsync(DateTime? from, DateTime? to)
        {
            var filters = new List<string>
            {
                "status = @Approved"
            };

            if (from.HasValue) 
                filters.Add("processed_at >= @From");
            
            if (to.HasValue) 
                filters.Add("processed_at <= @To");

            var whereClause = string.Join(" AND ", filters);

            const string sql = @"
            SELECT processor_used AS Processor,
                   COUNT(*) AS TotalRequests,
                   SUM(amount) AS TotalAmount
            FROM payments
            WHERE " + "{where}" + @"
            GROUP BY processor_used;";

            using var connection = CreateConnection();
            var result = await connection.QueryAsync<(string Processor, int TotalRequests, decimal TotalAmount)>(
                sql.Replace("{where}", whereClause),
                new
                {
                    From = from,
                    To = to,
                    Approved = (int)StatusPayment.Approved
                });

            var summary = new PaymentsSummaryAggregate();

            foreach (var row in result)
            {
                if (row.Processor == "default")
                {
                    summary.Default.TotalRequests = row.TotalRequests;
                    summary.Default.TotalAmount = row.TotalAmount;
                }
                else if (row.Processor == "fallback")
                {
                    summary.Fallback.TotalRequests = row.TotalRequests;
                    summary.Fallback.TotalAmount = row.TotalAmount;
                }
            }

            return summary;
        }
    }
}
