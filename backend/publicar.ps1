# Gera o pacote do backend pronto para subir numa hospedagem IIS (ex.: MonsterASP.NET).
#
# Uso (na pasta backend):   powershell -ExecutionPolicy Bypass -File .\publicar.ps1
# Saída: ..\deploy\api\ (pasta) e ..\deploy\cosmocash-api.zip
#
# O pacote NÃO leva segredos. As variáveis entram no web.config DENTRO DO SERVIDOR, depois do upload.

param(
  [string]$Saida = (Join-Path $PSScriptRoot '..\deploy\api')
)

$ErrorActionPreference = 'Stop'
$Saida = [System.IO.Path]::GetFullPath($Saida)
$Zip = Join-Path (Split-Path $Saida -Parent) 'cosmocash-api.zip'

if (Test-Path $Saida) { Remove-Item $Saida -Recurse -Force }
New-Item -ItemType Directory -Path $Saida | Out-Null

Write-Host "Publicando em $Saida ..."
# RollForward=LatestMajor: o app (net9.0) também roda se o servidor só tiver um runtime mais novo (ex.: .NET 10).
dotnet publish (Join-Path $PSScriptRoot 'CosmoCash.API\CosmoCash.API.csproj') `
  -c Release -o $Saida `
  -p:UseAppHost=false -p:DebugType=none -p:DebugSymbols=false -p:RollForward=LatestMajor
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish falhou.' }

# Arquivos que não devem ir para o servidor.
Remove-Item (Join-Path $Saida 'appsettings.Development.json') -ErrorAction SilentlyContinue
Get-ChildItem $Saida -Filter *.pdb -Recurse | Remove-Item -Force

# web.config com as variáveis de ambiente como MODELO (os valores reais são preenchidos no servidor).
$webConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <!-- stdoutLogEnabled="true" só enquanto estiver diagnosticando; depois volte para "false". -->
      <aspNetCore processPath="dotnet" arguments=".\CosmoCash.API.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="CONNECTION_STRING" value="PREENCHER" />
          <environmentVariable name="JWT_KEY" value="PREENCHER" />
          <environmentVariable name="CORS_ORIGINS" value="PREENCHER" />
          <environmentVariable name="ADMIN_NOME" value="Administrador" />
          <environmentVariable name="ADMIN_EMAIL" value="PREENCHER" />
          <environmentVariable name="ADMIN_SENHA" value="PREENCHER" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
'@
Set-Content -Path (Join-Path $Saida 'web.config') -Value $webConfig -Encoding UTF8

# Pasta de logs do stdout (o IIS não a cria sozinho).
New-Item -ItemType Directory -Path (Join-Path $Saida 'logs') -Force | Out-Null
Set-Content -Path (Join-Path $Saida 'logs\.keep') -Value '' -Encoding UTF8

if (Test-Path $Zip) { Remove-Item $Zip -Force }
Compress-Archive -Path (Join-Path $Saida '*') -DestinationPath $Zip

$tamanho = [math]::Round((Get-Item $Zip).Length / 1MB, 1)
Write-Host ''
Write-Host "Pronto: $Zip ($tamanho MB)"
Write-Host 'Lembrete: preencha os valores PREENCHER do web.config no servidor. Nunca versione esse arquivo preenchido.'
