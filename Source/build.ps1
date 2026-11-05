param([string]$GamePath = 'C:\Steam\steamapps\common\Probably Stolen')
$ErrorActionPreference = 'Stop'
$managed = Join-Path $GamePath 'Probably Stolen_Data\Managed'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$destination = Join-Path $PSScriptRoot '..\Mods\FreeStart\FreeStart.dll'
$references = @('mscorlib', 'System', 'System.Core', 'netstandard', 'Assembly-CSharp', '0Harmony', 'UnityEngine.CoreModule', 'UnityEngine.UI', 'Unity.TextMeshPro')
$compilerArgs = @('/nologo', '/noconfig', '/target:library', '/optimize+', '/nostdlib+', ('/out:' + $destination))
foreach ($name in $references) { $compilerArgs += '/reference:' + (Join-Path $managed ($name + '.dll')) }
$compilerArgs += Join-Path $PSScriptRoot 'FreeStart.cs'
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw "Build failed: $LASTEXITCODE" }
Write-Host "Built: $destination"
