$container = "fleet-sql"
$image     = "mcr.microsoft.com/mssql/server:2022-latest"
$saPwd     = "StrongPassword123"
$appPwd    = "AppPassword123"
$sqlScript = "Database\group8_p2.sql"

if (docker ps -aq -f "name=^${container}$") {
    docker rm -f $container | Out-Null
}

docker run `
    -e "ACCEPT_EULA=Y" `
    -e "SA_PASSWORD=$saPwd" `
    -e "MSSQL_PID=Developer" `
    -p 1433:1433 `
    --name $container `
    -d $image | Out-Null

Write-Host "Waiting for SQL Server to accept connections..."
Start-Sleep -Seconds 20

function Invoke-Sql([string]$query) {
    docker exec -i $container /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -P $saPwd -C -Q $query
}

Invoke-Sql "IF DB_ID('FleetDB') IS NULL CREATE DATABASE FleetDB;"
Invoke-Sql "IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name='app_user')
            CREATE LOGIN app_user WITH PASSWORD='$appPwd', CHECK_POLICY=OFF;"
Invoke-Sql "USE FleetDB;
            IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name='app_user')
                CREATE USER app_user FOR LOGIN app_user;
            EXEC sp_addrolemember 'db_owner','app_user';
            ALTER LOGIN app_user WITH DEFAULT_DATABASE = FleetDB;"

docker cp $sqlScript "${container}:/tmp/group56_p2.sql" | Out-Null
docker exec -i $container /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P $saPwd -C -i /tmp/group56_p2.sql

# Re-create the db user because group56_p2.sql drops and rebuilds the DB.
Invoke-Sql "USE FleetDB;
            IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name='app_user')
                CREATE USER app_user FOR LOGIN app_user;
            EXEC sp_addrolemember 'db_owner','app_user';"

Write-Host "FleetDB is ready on localhost:1433"
Write-Host "  SA login: sa / $saPwd"
Write-Host "  App login: app_user / $appPwd"

