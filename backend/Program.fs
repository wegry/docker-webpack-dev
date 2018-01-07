module Backend.App

open System
open System.IO
open System.Threading.Tasks

open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe.HttpHandlers
open Giraffe.Middleware
open Newtonsoft.Json
open Types 

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=>  json { valueUsd = Decimal(0); at = DateTime.UtcNow }
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader() |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseCors(configureCors)
       .UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseGiraffe(webApp)

let client = new HttpClient()
let delay = TimeSpan.FromMinutes(1.1)

let requestWork() =
    printfn "[INFO] Callback fired"
    async {
        printfn "[INFO] Requesting data from coinmarketcap"
        let! response = client.GetAsync("https://api.coinmarketcap.com/v1/global/") |> Async.AwaitTask
        let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        printfn "[INFO] Received data from coinmarketcap"
        let coinMarketCapAllResponse = JsonConvert.DeserializeObject<AllResponse>(body)

        let writeEvent =
            Data.writeValue <| Decimal coinMarketCapAllResponse.total_market_cap_usd
            |> Async.RunSynchronously
        
        printfn "[INFO] Wrote data to db"

        do! Async.Sleep 20
    }

let periodicallyHitApi () =
    printfn "[INFO] Setting api client up"

    let awaitTaskVoid : (Task -> Async<unit>) = Async.AwaitIAsyncResult >> Async.Ignore

    let timer = 
        async {
            while true do
                do! requestWork()
                do! Task.Delay(delay) |> awaitTaskVoid
        }

    timer

let configureServices (services : IServiceCollection) =
    services.AddCors() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    let contentRoot = Directory.GetCurrentDirectory()

    let apiTimer =
        periodicallyHitApi()

    let server =
        async {
            WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRoot)
                .UseUrls("http://0.0.0.0:5000")
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
                .Build()
                .Run()
        }

    [apiTimer; server] |> Async.Parallel |> Async.RunSynchronously
    
    0