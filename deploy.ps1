Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AppName = "discobot"
$RemoteUser = "deploy"
$RemoteHost = "127.0.0.1"
$RemotePath = "/home/deploy/apps/$AppName"

Write-Host "Building DiscoBot"


dotnet publish `
  -c Release `
  -f net10.0 `
  --no-self-contained `
  -o .\publish


Write-Host "Stopping service..."
ssh $RemoteUser@$RemoteHost "sudo rc-service $AppName stop"

Write-Host "Uploading files..."
scp -r .\publish\* "${RemoteUser}@${RemoteHost}:${RemotePath}"

Write-Host "Setting permissions..."
ssh $RemoteUser@$RemoteHost "sudo chown -R deploy:deploy $RemotePath"

Write-Host "Starting service..."
ssh $RemoteUser@$RemoteHost "sudo rc-service $AppName start"

Write-Host "Done."
