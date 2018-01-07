CREATE TABLE IF NOT EXISTS market_cap (
  time_checked timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
  value_usd money NOT NULL
)