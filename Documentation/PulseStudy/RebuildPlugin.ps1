param([string]$UnityDirectory = 'C:\Program Files\Unity\Hub\Editor\6000.2.12f1')
$ErrorActionPreference = 'Stop'
$pulseRepository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$pulseBuild = Join-Path $pulseRepository 'Temp\PulsePluginBuild'
$pulseJava = Join-Path $UnityDirectory 'Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin'
$pulseAndroidJar = Join-Path $UnityDirectory 'Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platforms\android-35\android.jar'
$pulseClassDir = Join-Path $pulseBuild 'classes'
$pulseAarDir = Join-Path $pulseBuild 'aar'
New-Item -ItemType Directory -Force -Path $pulseClassDir,$pulseAarDir | Out-Null
& (Join-Path $pulseJava 'javac.exe') -source 8 -target 8 -encoding UTF-8 -classpath $pulseAndroidJar -d $pulseClassDir (Join-Path $PSScriptRoot 'PulseCamera.java')
if ($LASTEXITCODE -ne 0) { throw 'Java compilation failed' }
& (Join-Path $pulseJava 'jar.exe') cf (Join-Path $pulseAarDir 'classes.jar') -C $pulseClassDir .
if ($LASTEXITCODE -ne 0) { throw 'JAR creation failed' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'AndroidManifest.xml'),(Join-Path $PSScriptRoot 'proguard.txt') -Destination $pulseAarDir
& (Join-Path $pulseJava 'jar.exe') cf (Join-Path $pulseRepository 'Assets\Plugins\Android\NeuroMazePulse.aar') -C $pulseAarDir .
if ($LASTEXITCODE -ne 0) { throw 'AAR creation failed' }
Write-Output 'NeuroMazePulse.aar rebuilt successfully.'
