# README

## One-time setup

1. Set AppHost user secrets (run from the repo root):

```bash
dotnet user-secrets set "Parameters:postgres-password" "postgres" --project server/Stoq.AppHost
dotnet user-secrets set "Parameters:rabbitmq-password" "guest" --project server/Stoq.AppHost
```

These match the credentials already in your `appsettings.Development.json`, so no changes are needed there.

2. Start AppHost once to recreate the containers with the new persistent configuration:

```bash
dotnet run --project server/Stoq.AppHost
```

Wait for everything to go healthy in the Aspire dashboard, then stop it with `Ctrl+C`. The containers keep running in Docker after AppHost exits.

## Day-to-day VS debugging

1. Open the solution in Visual Studio.
2. Set `Stoq.ApiService` as the startup project.
3. Press `F5` — it reads `appsettings.Development.json`, connects to the already running containers, applies EF migrations, and starts.

No Aspire is needed for daily debugging. The containers also survive reboots, as Docker Desktop auto-restarts them on login.

If the containers happen to be stopped, for example after a Docker Desktop restart with auto-start off, run:

```bash
docker start stoq-postgres stoq-rabbitmq
```

## Using Aspire again

To use full Aspire orchestration again, including dashboard, health checks, and log streaming, just start AppHost. It detects the existing persistent containers and reuses them.
