# Kişisel REA fork'u için Windows kurulum ve güncelleme betiği (PowerShell 5.1+ / 7+).
#
#   powershell -ExecutionPolicy Bypass -File kisisel\kur.ps1              # kur
#   powershell -ExecutionPolicy Bypass -File kisisel\kur.ps1 guncelle     # upstream'i birleştir + yeniden kur
#   powershell -ExecutionPolicy Bypass -File kisisel\kur.ps1 beceriler    # yalnızca Türkçe skill'leri kopyala
#
# Ek argümanlar `rea setup`a iletilir; örneğin: kisisel\kur.ps1 kur --client claude_code
# Betik Node.js, npm veya git kurmaz.
param(
  [string]$Komut = "kur",
  [Parameter(ValueFromRemainingArguments = $true)][string[]]$Ek = @()
)
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root
$Upstream = if ($env:REA_UPSTREAM_URL) { $env:REA_UPSTREAM_URL } else { "https://github.com/morluto/rea.git" }

function Bilgi($m) { Write-Host "==> $m" -ForegroundColor Cyan }
function Hata($m) { Write-Host "HATA: $m" -ForegroundColor Red; exit 1 }
function Calistir($exe, [string[]]$argv) {
  & $exe @argv
  if ($LASTEXITCODE -ne 0) { Hata "$exe $($argv -join ' ') başarısız (çıkış kodu $LASTEXITCODE)." }
}

function OnKosullar {
  foreach ($c in "git", "node", "npm") {
    if (-not (Get-Command $c -ErrorAction SilentlyContinue)) { Hata "$c bulunamadı." }
  }
  $kontrol = 'const [a,b,c]=process.versions.node.split(".").map(Number);const ge=(x,y,z)=>a>x||(a===x&&(b>y||(b===y&&c>=z)));const ok=(a===22&&ge(22,23,4))||(a===24&&ge(24,22,1))||ge(26,12,1);process.exit(ok?0:1)'
  & node -e $kontrol
  if ($LASTEXITCODE -ne 0) { Hata "Node $(node -v) desteklenmiyor; gereken: ^22.23.4 || ^24.22.1 || >=26.12.1" }
}

function Derle {
  Bilgi "Bağımlılıklar kuruluyor (npm ci)"
  Calistir npm @("ci", "--no-audit", "--no-fund")
  Bilgi "Derleniyor (npm run build:cached)"
  Calistir npm @("run", "build:cached")
  Bilgi "Derlenen sürüm: $(node scripts/rea.mjs --version)"
}

function AjanKurulumu {
  Bilgi "Ajan kurulumu başlıyor. Plan gösterilecek ve onayınız istenecek."
  Bilgi "MCP kaydı bu klasördeki derlemeyi gösterecek: $Root\scripts\rea.mjs"
  Calistir node (@("scripts/rea.mjs", "setup") + $Ek)
  Bilgi "Bitti. Ajanınızı yeniden başlatın."
}

function BecerilerKur {
  $hedef = if ($env:CLAUDE_SKILLS_DIR) { $env:CLAUDE_SKILLS_DIR } else { Join-Path $HOME ".claude\skills" }
  New-Item -ItemType Directory -Force -Path $hedef | Out-Null
  $isaret = ".rea-kisisel"
  $zaman = Get-Date -Format "yyyyMMddHHmmss"
  Bilgi "Türkçe skill'ler kopyalanıyor: $hedef"
  Get-ChildItem (Join-Path $Root "kisisel\beceriler") -Directory | ForEach-Object {
    $yer = Join-Path $hedef $_.Name
    if ((Test-Path $yer) -and -not (Test-Path (Join-Path $yer $isaret))) {
      Bilgi "  $($_.Name) zaten var; yedekleniyor: $yer.yedek-$zaman"
      Move-Item $yer "$yer.yedek-$zaman"
    }
    if (Test-Path $yer) { Remove-Item -Recurse -Force $yer }
    Copy-Item -Recurse $_.FullName $yer
    New-Item -ItemType File -Path (Join-Path $yer $isaret) | Out-Null
    Write-Host "    /$($_.Name)"
  }
  Bilgi "Windows'ta kopya kullanılır; güncellemeden sonra bu adım yeniden çalışır."
}

function UpstreamBirlestir {
  if (git status --porcelain) { Hata "Kaydedilmemiş değişiklikler var; önce commit veya stash yapın." }
  if (@(git remote) -notcontains "upstream") { Bilgi "upstream ekleniyor: $Upstream"; Calistir git @("remote", "add", "upstream", $Upstream) }
  if ((git rev-parse --is-shallow-repository) -eq "true") { Calistir git @("fetch", "--unshallow", "origin") }
  Calistir git @("fetch", "upstream", "main")
  Bilgi "upstream/main birleştiriliyor"
  & git merge --no-edit upstream/main
  if ($LASTEXITCODE -ne 0) { Hata "Birleştirme çakışması; çözüp 'git commit' yapın ve tekrar çalıştırın." }
}

switch ($Komut) {
  "kur" { OnKosullar; Derle; AjanKurulumu; BecerilerKur }
  "guncelle" { OnKosullar; UpstreamBirlestir; Derle; AjanKurulumu; BecerilerKur }
  "beceriler" { BecerilerKur }
  "dogrula" { OnKosullar; & node scripts/rea.mjs doctor }
  default { Hata "Bilinmeyen komut: $Komut (kur | guncelle | beceriler | dogrula)" }
}
