#!/usr/bin/env bash
# Kişisel REA fork'u için kurulum, güncelleme ve doğrulama betiği.
#
#   kisisel/kur.sh                 # bağımlılıkları kur, derle, ajan kurulumunu başlat
#   kisisel/kur.sh guncelle        # upstream'i (morluto/rea) birleştir, yeniden derle, kurulumu yenile
#   kisisel/kur.sh guncelle --push # birleştirdikten sonra fork'a da gönder
#   kisisel/kur.sh dogrula         # doctor + küçük bir duman testi
#   kisisel/kur.sh beceriler       # yalnızca Türkçe Claude Code skill'lerini bağla
#
# "kur" ve "guncelle" sonunda `rea setup` çalışır; bu komut değişiklik planını
# gösterir ve onay ister. Ek argümanlar setup'a iletilir, örneğin:
#   kisisel/kur.sh kur --client claude_code --client cursor
#
# Betik Node.js, npm veya git kurmaz; sadece varlıklarını kontrol eder.
set -euo pipefail

UPSTREAM_URL="${REA_UPSTREAM_URL:-https://github.com/morluto/rea.git}"
UPSTREAM_BRANCH="${REA_UPSTREAM_BRANCH:-main}"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

bilgi() { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
hata() { printf '\033[1;31mHATA:\033[0m %s\n' "$*" >&2; exit 1; }

on_kosullar() {
  command -v git >/dev/null || hata "git bulunamadı."
  command -v node >/dev/null || hata "Node.js bulunamadı. Node 24.22.1+ (önerilen) veya 22.23.4+ kurun."
  command -v npm >/dev/null || hata "npm bulunamadı."
  # package.json#engines: ^22.23.4 || ^24.22.1 || >=26.12.1
  node -e '
    const [a, b, c] = process.versions.node.split(".").map(Number);
    const ge = (x, y, z) => a > x || (a === x && (b > y || (b === y && c >= z)));
    const ok = (a === 22 && ge(22, 23, 4)) || (a === 24 && ge(24, 22, 1)) || ge(26, 12, 1);
    if (!ok) { console.error("Node " + process.versions.node + " desteklenmiyor; gereken: ^22.23.4 || ^24.22.1 || >=26.12.1"); process.exit(1); }
  ' || hata "Uygun bir Node.js sürümüne geçin (ör. nvm use)."
  if [[ "$(uname -s)" == "Linux" ]] && ! ps -o pid= -p $$ >/dev/null 2>&1; then
    hata "Linux'ta procps uyumlu 'ps' gerekli (Debian/Ubuntu: sudo apt install procps)."
  fi
}

derle() {
  bilgi "Bağımlılıklar kuruluyor (npm ci)"
  npm ci --no-audit --no-fund
  bilgi "Derleniyor (npm run build:cached)"
  npm run build:cached
  bilgi "Derlenen sürüm: $(node scripts/rea.mjs --version)"
}

ajan_kurulumu() {
  bilgi "Ajan kurulumu başlıyor. Plan gösterilecek ve onayınız istenecek."
  bilgi "MCP kaydı bu klasördeki derlemeyi gösterecek: $ROOT/scripts/rea.mjs"
  node scripts/rea.mjs setup "$@"
  bilgi "Bitti. Ajanınızı (Claude Code, Cursor vb.) yeniden başlatın."
}

upstream_birlestir() {
  local push="$1"
  [[ -z "$(git status --porcelain)" ]] || hata "Çalışma ağacında kaydedilmemiş değişiklikler var; önce commit veya stash yapın."
  if ! git remote get-url upstream >/dev/null 2>&1; then
    bilgi "upstream uzak deposu ekleniyor: $UPSTREAM_URL"
    git remote add upstream "$UPSTREAM_URL"
  fi
  if [[ "$(git rev-parse --is-shallow-repository)" == "true" ]]; then
    bilgi "Sığ klon tespit edildi; birleştirme için geçmiş çekiliyor"
    git fetch --unshallow origin || git fetch --depth=5000 origin
  fi
  bilgi "upstream/$UPSTREAM_BRANCH çekiliyor"
  git fetch upstream "$UPSTREAM_BRANCH"
  local branch
  branch="$(git rev-parse --abbrev-ref HEAD)"
  bilgi "upstream/$UPSTREAM_BRANCH, '$branch' dalına birleştiriliyor"
  git merge --no-edit "upstream/$UPSTREAM_BRANCH" || hata "Birleştirme çakışması. Çakışmaları çözüp 'git commit' yapın, sonra betiği tekrar çalıştırın."
  if [[ "$push" == "1" ]]; then
    bilgi "origin/$branch dalına gönderiliyor"
    git push -u origin "$branch"
  fi
}

beceriler_kur() {
  local hedef="${CLAUDE_SKILLS_DIR:-$HOME/.claude/skills}"
  local zaman
  zaman="$(date +%Y%m%d%H%M%S)"
  mkdir -p "$hedef"
  bilgi "Türkçe skill'ler bağlanıyor: $hedef"
  for kaynak in "$ROOT"/kisisel/beceriler/*/; do
    local ad
    ad="$(basename "$kaynak")"
    local yer="$hedef/$ad"
    if [[ -e "$yer" && ! -L "$yer" ]]; then
      bilgi "  $ad zaten var (bağlantı değil); yedekleniyor: $yer.yedek-$zaman"
      mv "$yer" "$yer.yedek-$zaman"
    fi
    ln -sfn "${kaynak%/}" "$yer"
    printf '    /%s\n' "$ad"
  done
  bilgi "Skill'ler bu klasöre bağlı; 'guncelle' sonrası otomatik güncel kalır."
}

dogrula() {
  bilgi "rea doctor"
  node scripts/rea.mjs doctor || true
  bilgi "Duman testi: Node.js çalıştırılabilir dosyasının envanteri"
  node scripts/rea.mjs inspect-artifact "$(command -v node)" --filter-output normalized_result.subject
}

komut="${1:-kur}"
[[ $# -gt 0 ]] && shift

case "$komut" in
  kur)
    on_kosullar
    derle
    ajan_kurulumu "$@"
    beceriler_kur
    ;;
  guncelle)
    push=0
    rest=()
    for a in "$@"; do
      if [[ "$a" == "--push" ]]; then push=1; else rest+=("$a"); fi
    done
    on_kosullar
    upstream_birlestir "$push"
    derle
    ajan_kurulumu "${rest[@]+"${rest[@]}"}"
    beceriler_kur
    ;;
  beceriler)
    beceriler_kur
    ;;
  dogrula)
    on_kosullar
    dogrula
    ;;
  -h|--help|yardim)
    sed -n '2,14p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
    ;;
  *)
    hata "Bilinmeyen komut: $komut (kur | guncelle | dogrula | beceriler | yardim)"
    ;;
esac
