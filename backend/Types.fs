module Backend.Types

open System

type MarketCap = {
  valueUsd : Decimal;
  at: DateTime
}

type AllResponse = {
  total_market_cap_usd: uint64;
  total_24h_volume_usd: uint64;
  bitcoin_percentage_of_market_cap: float;
  active_currencies: uint32;
  active_assets: uint32;
  active_markets: uint32;
  last_updated: uint64;
}