﻿using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using PaymentGateway.Common.Constants;
using PaymentGateway.Common.Model;
using System.Data;


namespace PaymentGateway.Common.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ILogger<PaymentRepository> _logger;
        private readonly string _connectionString;
        
        public PaymentRepository(ILogger<PaymentRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("Postgres");
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

        public async Task UpdateEntityAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            const string sql = @"
            UPDATE payments
            SET amount = @Amount,
                requested_at = @RequestedAt,
                status = @Status,
                processed_at = @ProcessedAt,
                processed = @Processed,
                processor_used = @ProcessorUsed,
                total_attempts = @TotalAttempts
            WHERE correlation_id = @CorrelationId;";
            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, payment);
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

            string sql = @"
            SELECT processor_used AS Processor,
                   COUNT(*) AS TotalRequests,
                   SUM(amount) AS TotalAmount
            FROM payments
            WHERE " + "{where}" + @"
            GROUP BY processor_used;";

            string sqlComplete = sql.Replace("{where}", whereClause);

            using var connection = CreateConnection();
            var result = await connection.QueryAsync<(string Processor, int TotalRequests, decimal TotalAmount)>(
                sqlComplete,
                new
                {
                    From = from?.ToUniversalTime(),
                    To = to?.ToUniversalTime(),
                    Approved = StatusPayment.Approved.ToString("D")
                });

            

            if (result is null || !result.Any())
            {
                return new PaymentsSummaryAggregate();
            }

            _logger.LogInformation("Query resultado {Count} linhas", result.Count());

            var summary = new PaymentsSummaryAggregate
            {
                Default = new Default(),
                Fallback = new Fallback()
            };

            foreach (var row in result)
            {
                if (row.Processor == ProcessorCodes.Default)
                {
                    summary.Default.TotalRequests = row.TotalRequests;
                    summary.Default.TotalAmount = row.TotalAmount;
                }
                else if (row.Processor == ProcessorCodes.Fallback)
                {
                    summary.Fallback.TotalRequests = row.TotalRequests;
                    summary.Fallback.TotalAmount = row.TotalAmount;
                }
            }

            return summary;
        }

        public async Task<Payment?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT 
                    correlation_id AS CorrelationId,
                    amount AS Amount,
                    created_at AS CreatedAt,
                    requested_at AS RequestedAt,
                    processor_used AS ProcessorUsed,
                    status AS Status,
                    processed_at AS ProcessedAt,
                    processed AS Processed,
                    total_attempts AS TotalAttempts
                FROM payments
                WHERE correlation_id = @CorrelationId
                LIMIT 1;";

            using var connection = CreateConnection();
            var payment = await connection.QueryFirstOrDefaultAsync<Payment>(
                new CommandDefinition(sql, new { CorrelationId = correlationId }, cancellationToken: cancellationToken)
            );
            return payment;
        }

        public async Task<IEnumerable<Payment>?> GetPendingRetriesAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
            SELECT 
            correlation_id AS CorrelationId,
            amount, 
            requested_at AS RequestedAt,
            processed_at AS ProcessedAt,
            status AS Status,
            processor_used AS ProcessorUsed,
            processed, 
            total_attempts AS TotalAttempts
            FROM payments
            WHERE status = @Status
                AND total_attempts <= 5;
            ";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Payment>(sql, new
            {
                Status = StatusPayment.Failed.ToString("D")
            });
        }

    }
}
