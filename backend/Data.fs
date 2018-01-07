module Backend.Data

open System
open Npgsql
open NpgsqlTypes
open Types

// So secure
let connString = "Host=postgres;Username=postgres;Password=postgres;Database=market_cap";

let readValues () =
    async {
        use conn = new NpgsqlConnection(connString)
        conn.Open()

        // Retrieve all rows
        use cmd = new NpgsqlCommand("SELECT value_usd, time_checked FROM market_cap", conn)
        cmd.Prepare()
        use! reader = cmd.ExecuteReaderAsync() |> Async.AwaitTask

        return seq {
            while (reader.Read())
                do yield  {
                    MarketCap.valueUsd = reader.GetDecimal(0);
                    at = reader.GetDateTime(1)
                }
        }
    }

let writeValue (marketCapUsd: Decimal) =
    async {
        use conn = new NpgsqlConnection(connString)
        conn.OpenAsync() |> Async.AwaitTask |> Async.RunSynchronously
        use cmd = new NpgsqlCommand()

        cmd.Connection <- conn
        cmd.CommandText <- "INSERT INTO market_cap (value_usd) VALUES (@value_usd)";
        cmd.Parameters.AddWithValue("value_usd", NpgsqlDbType.Money,  marketCapUsd)
        |> ignore

        return cmd.ExecuteNonQuery()
    }