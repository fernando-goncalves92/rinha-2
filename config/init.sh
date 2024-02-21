#!/bin/bash
wait_time=10s

echo Database will be created in $wait_time seconds
sleep $wait_time
echo Creating datanase...

/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "Rinha@123" -i /tmp/setup.sql