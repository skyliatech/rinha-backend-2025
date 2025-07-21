CREATE TABLE IF NOT EXISTS payments (
    correlation_id VARCHAR(255) PRIMARY KEY,
    amount DECIMAL(18,2) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    requested_at TIMESTAMP NOT NULL,
    processor_used VARCHAR(100), -- Nullable enum
    status VARCHAR(50) NOT NULL, -- Enum obrigatória
    processed_at TIMESTAMP NULL,
    processed BOOLEAN NOT NULL DEFAULT FALSE,
    total_attempts INT NOT NULL DEFAULT 0
);

-- Índice por status (busca por pagamentos com determinado status)
CREATE INDEX IF NOT EXISTS idx_status ON payments (status);

-- Índice por processed (para reprocessamento, por exemplo)
CREATE INDEX IF NOT EXISTS idx_processed ON payments (processed);
