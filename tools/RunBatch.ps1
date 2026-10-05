# Toplu koşu (batch): bir koşu listesini (CSV) FrameSap2000.exe /batch ile, en fazla -Parallel koşu aynı anda olmak üzere çalıştırır.
# Her koşu kendi klasöründe, modelin bir kopyasıyla çalışır (orijinal model değişmez; ErrorLog.txt koşu klasörüne yazılır).
# Sonunda <Out>\summary.csv: maliyet, ceza, final durumu, analiz sayısı, süre ve hibrit yığın satırları.
#
# Kullanım (proje kökünde, PowerShell):
#   powershell -ExecutionPolicy Bypass -File tools\RunBatch.ps1 -Runs tools\Asama7_runs.csv -Out D:\Kosular\Asama7 -Exe bin\Release\FrameSap2000.exe
#   -Parallel 3      aynı anda çalışacak koşu (ETABS) sayısı
#   -StartDelay 120  koşuların başlangıç aralığı [s]
#   -Resume          sonucu olmayan koşular yedekten (<ad>.result.backup.xml) devam eder
#                    (sonuçsuz biten koşu her durumda bir kez yedekten yeniden başlatılır)
#   -SummaryOnly     koşu yapmaz, yalnızca summary.csv yazar
#
# Runs CSV sütunları: Name, Model, Method, Mode, MaxAnalyses, MemorySize, Seed[, Transition, AbcLimit]
#   Method : HarmornySearch | ArtificialBeeColony | SocialSpider | (diğer OptMethod_ adları)
#   Mode   : Steel | Composite | Hybrid        Transition: PerStack | PerGroup (Hybrid)
#   Model  : göreli yol CSV dosyasının klasörüne göre
param(
    [Parameter(Mandatory = $true)][string]$Runs,
    [Parameter(Mandatory = $true)][string]$Out,
    [string]$Exe = "",
    [string]$Template = "",
    [int]$Parallel = 3,
    [int]$StartDelay = 120,
    [switch]$Resume,
    [switch]$SummaryOnly
)
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot
if (-not $Template) { $Template = Join-Path $PSScriptRoot 'batch_template.xml' }
if (-not $Exe) { $Exe = Join-Path $Root 'bin\Release\FrameSap2000.exe' }
$Exe = (Resolve-Path $Exe).Path
$RunsPath = (Resolve-Path $Runs).Path
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$Out = (Resolve-Path $Out).Path
$List = Import-Csv $RunsPath

function Set-Node($xml, $path, $value) {
    $n = $xml.SelectSingleNode($path)
    if ($null -eq $n) { throw "XML alanı yok: $path" }
    $n.InnerText = [string]$value
}

# ---- koşu klasörleri ve ayar dosyaları
$Jobs = @()
foreach ($r in $List) {
    $Dir = Join-Path $Out $r.Name
    New-Item -ItemType Directory -Force -Path $Dir | Out-Null
    $Src = $r.Model
    if (-not [IO.Path]::IsPathRooted($Src)) { $Src = Join-Path (Split-Path -Parent $RunsPath) $Src }
    $Model = Join-Path $Dir ([IO.Path]::GetFileName($Src))
    $Result = Join-Path $Dir ($r.Name + '.result.xml')
    $Settings = Join-Path $Dir ($r.Name + '.settings.xml')
    # program .NET Framework: 260 karakteri aşan yollar yazılamaz (yedek: <ad>.result.backup.xml.bak); 8.3 kısa adlar için pay
    if (-not $SummaryOnly -and -not (Test-Path $Result)) {
        $Longest = Join-Path $Dir ($r.Name + '.result.backup.xml.bak')
        if ($Longest.Length -gt 230) { throw "Yol çok uzun ($($Longest.Length) karakter, en çok 230): $Longest. -Out için daha kısa bir klasör seçin." }
        if (-not (Test-Path $Model)) { Copy-Item $Src $Model }
        [xml]$x = Get-Content -Raw -Encoding UTF8 $Template
        Set-Node $x '/FormInfo_/FileList/ETABSFile' $Model
        Set-Node $x '/FormInfo_/FileList/OutputFile' $Result
        Set-Node $x '/FormInfo_/OptInfo/OptimizationMethod' $r.Method
        Set-Node $x '/FormInfo_/OptInfo/MaxFuncEvaluation' $r.MaxAnalyses
        Set-Node $x '/FormInfo_/OptInfo/MemorySize' $r.MemorySize
        Set-Node $x '/FormInfo_/Seed' $r.Seed
        Set-Node $x '/FormInfo_/HideETABS' 'true'
        Set-Node $x '/FormInfo_/CompositeColumns' $(if ($r.Mode -eq 'Steel') { 'false' } else { 'true' })
        Set-Node $x '/FormInfo_/HybridColumns' $(if ($r.Mode -eq 'Hybrid') { 'true' } else { 'false' })
        if ($r.Transition) { Set-Node $x '/FormInfo_/TransitionMode' $r.Transition }
        if ($r.AbcLimit) {
            $oi = $x.SelectSingleNode('/FormInfo_/OptInfo')
            $ps = $oi.SelectSingleNode('Params'); if ($null -eq $ps) { $ps = $oi.AppendChild($x.CreateElement('Params')) }
            $p = $ps.AppendChild($x.CreateElement('MethodParam_'))
            foreach ($kv in @(@('Method', 'ArtificialBeeColony'), @('Key', 'Limit'), @('Value', $r.AbcLimit))) {
                $e = $p.AppendChild($x.CreateElement($kv[0])); $e.InnerText = $kv[1]
            }
        }
        $x.Save($Settings)
    }
    $Jobs += [pscustomobject]@{ Name = $r.Name; Dir = $Dir; Settings = $Settings; Result = $Result; Row = $r }
}

# ---- koşular: sistemde en fazla $Parallel FrameSap2000 süreci (başka bir betiğin başlattıkları da sayılır),
#      $StartDelay aralıkla. Başka bir süreçte çalışan koşu beklenir; sonuçsuz biten koşu yedeğinden bir kez yeniden başlatılır.
function Get-Batches {
    @(Get-CimInstance Win32_Process -Filter "Name='FrameSap2000.exe'" | ForEach-Object { [string]$_.CommandLine })
}
if (-not $SummaryOnly) {
    $Pending = [Collections.Generic.List[object]]::new()
    foreach ($j in $Jobs) { if (-not (Test-Path $j.Result)) { $Pending.Add($j) } }
    $Tries = @{}
    $LastStart = [datetime]::MinValue
    while ($Pending.Count -gt 0) {
        $Cmd = Get-Batches
        foreach ($j in @($Pending)) {
            $Busy = @($Cmd | Where-Object { $_ -like ('*' + $j.Settings + '*') }).Count -gt 0
            if (-not $Busy -and (Test-Path $j.Result)) { $Pending.Remove($j) | Out-Null; Write-Output ("{0:HH:mm:ss} bitti: {1}" -f (Get-Date), $j.Name) }
            elseif (-not $Busy -and $Tries[$j.Name] -ge 2) { $Pending.Remove($j) | Out-Null; Write-Output ("{0:HH:mm:ss} sonuçsuz bırakıldı: {1}" -f (Get-Date), $j.Name) }
        }
        $Free = $Parallel - $Cmd.Count
        if ($Free -gt 0 -and ((Get-Date) - $LastStart).TotalSeconds -ge $StartDelay) {
            $j = $Pending | Where-Object { $s = $_.Settings; @($Cmd | Where-Object { $_ -like ('*' + $s + '*') }).Count -eq 0 } | Select-Object -First 1
            if ($j) {
                $a = @('/batch', ('"' + $j.Settings + '"'))
                $Bak = [IO.Path]::ChangeExtension($j.Result, '.backup.xml')
                if (($Resume -or $Tries[$j.Name] -ge 1) -and (Test-Path $Bak)) { $a += '/resume' }
                $p = Start-Process -FilePath $Exe -ArgumentList $a -WorkingDirectory (Split-Path -Parent $Exe) -PassThru
                $Tries[$j.Name] = 1 + [int]$Tries[$j.Name]
                Write-Output ("{0:HH:mm:ss} başladı: {1} (pid {2}{3})" -f (Get-Date), $j.Name, $p.Id, $(if ($a -contains '/resume') { ', yedekten' } else { '' }))
                $LastStart = Get-Date
            }
        }
        Start-Sleep -Seconds 30
    }
    Write-Output ("{0:HH:mm:ss} bütün koşular bitti" -f (Get-Date))
}

# ---- özet
$Rows = foreach ($j in $Jobs) {
    $o = [ordered]@{ Name = $j.Name; Method = $j.Row.Method; Mode = $j.Row.Mode; Seed = $j.Row.Seed; MaxAnalyses = $j.Row.MaxAnalyses }
    if (Test-Path $j.Result) {
        [xml]$x = Get-Content -Raw -Encoding UTF8 $j.Result
        $c = $x.ClassFinal
        $o.Analyses = $c.Analyses
        $o.BestCost = $c.GlobalBest.CostValue
        $o.BestPenalty = $c.GlobalBest.Penalty
        $o.FinalCost = $c.FinalCheck.CostValue
        $o.FinalPenalty = $c.FinalCheck.Penalty
        $o.FinalFails = $c.FinalFails
        $o.Stacks = (@($c.GlobalBestPrint.string) | Where-Object { $_ -like 'Stack *' }) -join ' | '
    } else { $o.Analyses = ''; $o.BestCost = ''; $o.BestPenalty = ''; $o.FinalCost = ''; $o.FinalPenalty = ''; $o.FinalFails = 'no result'; $o.Stacks = '' }
    # süre: ErrorLog.txt'nin son "run time d.hh:mm:ss" satırı (optimizasyon; başlangıç hazırlığı hariç)
    $o.Hours = ''
    $El = Join-Path $j.Dir 'ErrorLog.txt'
    if (Test-Path $El) {
        $m = Select-String -Path $El -Pattern 'run time (\d+)\.(\d\d):(\d\d):(\d\d)' | Select-Object -Last 1
        if ($m) { $g = $m.Matches[0].Groups; $o.Hours = [math]::Round([int]$g[1].Value * 24 + [int]$g[2].Value + [int]$g[3].Value / 60 + [int]$g[4].Value / 3600, 2) }
    }
    [pscustomobject]$o
}
$Sum = Join-Path $Out 'summary.csv'
$Rows | Export-Csv -NoTypeInformation -Encoding UTF8 $Sum
$Rows | Select-Object Name, Analyses, BestCost, FinalCost, FinalPenalty, FinalFails, Hours | Format-Table -AutoSize | Out-String -Width 200
Write-Output "özet: $Sum"
