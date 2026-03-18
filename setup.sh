#!/bin/bash

# =============================================================
#  AniLog — Setup del entorno de desarrollo
#  CachyOS / Arch Linux
# =============================================================

set -e  # Detener si algun comando falla

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

ok()   { echo -e "${GREEN}[OK]${NC} $1"; }
info() { echo -e "${BLUE}[INFO]${NC} $1"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
err()  { echo -e "${RED}[ERROR]${NC} $1"; }

echo ""
echo "============================================="
echo "   AniLog — Setup del entorno de desarrollo  "
echo "   CachyOS / Arch Linux                      "
echo "============================================="
echo ""

# =============================================================
# PASO 1: Verificar herramientas existentes
# =============================================================
info "Verificando herramientas instaladas..."

HAS_DOTNET=false
HAS_PSQL=false
HAS_EFTOOLS=false
HAS_NODE=false
HAS_GIT=false

command -v dotnet &>/dev/null && HAS_DOTNET=true
command -v psql   &>/dev/null && HAS_PSQL=true
command -v node   &>/dev/null && HAS_NODE=true
command -v git    &>/dev/null && HAS_GIT=true

if $HAS_DOTNET; then
    dotnet tool list --global 2>/dev/null | grep -q "dotnet-ef" && HAS_EFTOOLS=true
fi

echo ""
echo "  .NET SDK   : $(if $HAS_DOTNET; then dotnet --version; else echo 'NO instalado'; fi)"
echo "  EF Tools   : $(if $HAS_EFTOOLS; then echo 'instalado'; else echo 'NO instalado'; fi)"
echo "  PostgreSQL : $(if $HAS_PSQL; then psql --version; else echo 'NO instalado'; fi)"
echo "  Node.js    : $(if $HAS_NODE; then node --version; else echo 'NO instalado'; fi)"
echo "  git        : $(if $HAS_GIT; then git --version; else echo 'NO instalado'; fi)"
echo ""

# =============================================================
# PASO 2: Instalar .NET 8 SDK
# =============================================================
if $HAS_DOTNET; then
    DOTNET_VERSION=$(dotnet --version)
    if [[ "$DOTNET_VERSION" == 8* ]]; then
        ok ".NET 8 ya esta instalado ($DOTNET_VERSION)"
    else
        warn ".NET instalado pero version $DOTNET_VERSION — instalando .NET 8 tambien..."
        sudo pacman -S --noconfirm --needed dotnet-sdk-8.0
        ok ".NET 8 instalado"
    fi
else
    info "Instalando .NET 8 SDK y ASP.NET Core runtime..."
    sudo pacman -S --noconfirm --needed dotnet-sdk-8.0 aspnet-runtime-8.0
    if dotnet --version 2>/dev/null | grep -q "^8"; then
        ok ".NET SDK instalado: $(dotnet --version)"
    else
        err "Fallo la instalacion de .NET SDK"
        exit 1
    fi
fi

# Instalar aspnet-runtime-8.0 si falta (puede estar ausente aunque dotnet-sdk-8.0 este instalado)
if ! dotnet --list-runtimes 2>/dev/null | grep -q "Microsoft.AspNetCore.App 8"; then
    info "Instalando ASP.NET Core runtime 8.0..."
    sudo pacman -S --noconfirm --needed aspnet-runtime-8.0
    ok "ASP.NET Core runtime 8.0 instalado"
else
    ok "ASP.NET Core runtime 8.0 ya esta instalado"
fi

# =============================================================
# PASO 3: Instalar EF Core tools
# =============================================================
if $HAS_EFTOOLS; then
    ok "EF Core tools ya esta instalado"
else
    info "Instalando EF Core tools..."
    dotnet tool install --global dotnet-ef

    DOTNET_TOOLS_PATH="$HOME/.dotnet/tools"

    # Agregar al PATH segun el shell en uso
    CURRENT_SHELL=$(basename "$SHELL")
    if [[ "$CURRENT_SHELL" == "fish" ]]; then
        FISH_CONFIG="$HOME/.config/fish/config.fish"
        mkdir -p "$(dirname "$FISH_CONFIG")"
        if ! grep -q "dotnet/tools" "$FISH_CONFIG" 2>/dev/null; then
            warn "Agregando ~/.dotnet/tools al PATH en $FISH_CONFIG"
            echo '' >> "$FISH_CONFIG"
            echo '# .NET tools' >> "$FISH_CONFIG"
            echo 'fish_add_path $HOME/.dotnet/tools' >> "$FISH_CONFIG"
        fi
    else
        # bash/zsh
        SHELL_RC="$HOME/.bashrc"
        [[ "$CURRENT_SHELL" == "zsh" ]] && SHELL_RC="$HOME/.zshrc"
        if ! grep -q "dotnet/tools" "$SHELL_RC" 2>/dev/null; then
            warn "Agregando ~/.dotnet/tools al PATH en $SHELL_RC"
            echo '' >> "$SHELL_RC"
            echo '# .NET tools' >> "$SHELL_RC"
            echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> "$SHELL_RC"
        fi
    fi

    export PATH="$PATH:$DOTNET_TOOLS_PATH"
    ok "EF Core tools instalado"
fi

# =============================================================
# PASO 4: Instalar Node.js LTS y npm
# =============================================================
if $HAS_NODE; then
    NODE_VERSION=$(node --version)
    NODE_MAJOR=$(echo "$NODE_VERSION" | sed 's/v\([0-9]*\).*/\1/')
    if [[ "$NODE_MAJOR" -ge 18 ]]; then
        ok "Node.js ya esta instalado ($NODE_VERSION)"
    else
        warn "Node.js instalado pero version $NODE_VERSION < 18 — actualizando..."
        sudo pacman -S --noconfirm --needed nodejs-lts-jod npm
        ok "Node.js actualizado: $(node --version)"
    fi
else
    info "Instalando Node.js LTS (Jod/22) y npm..."
    sudo pacman -S --noconfirm --needed nodejs-lts-jod npm
    ok "Node.js instalado: $(node --version)"
fi

# =============================================================
# PASO 5: Instalar PostgreSQL
# =============================================================
if $HAS_PSQL; then
    ok "PostgreSQL ya esta instalado: $(psql --version)"
else
    info "Instalando PostgreSQL..."
    sudo pacman -S --noconfirm --needed postgresql
    ok "PostgreSQL instalado: $(psql --version)"
fi

# Inicializar cluster si es la primera vez (Arch requiere esto antes de iniciar el servicio)
PG_DATA="/var/lib/postgres/data"
if [[ ! -f "$PG_DATA/PG_VERSION" ]]; then
    info "Inicializando cluster de PostgreSQL (primera vez)..."
    sudo -u postgres initdb -D "$PG_DATA"
    ok "Cluster inicializado"
else
    ok "Cluster de PostgreSQL ya inicializado"
fi

# Iniciar y habilitar el servicio
if ! systemctl is-active --quiet postgresql; then
    info "Iniciando servicio PostgreSQL..."
    sudo systemctl start postgresql
    sudo systemctl enable postgresql
fi
ok "Servicio PostgreSQL activo"

# =============================================================
# PASO 6: Crear base de datos para AniLog
# =============================================================
info "Configurando base de datos AniLog..."

DB_EXISTS=$(sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='anilog_db'" 2>/dev/null || echo "")

if [[ "$DB_EXISTS" == "1" ]]; then
    ok "Base de datos 'anilog_db' ya existe"
else
    sudo -u postgres psql -c "CREATE DATABASE anilog_db;"
    ok "Base de datos 'anilog_db' creada"
fi

# Configurar password del usuario postgres
echo ""
echo -e "${YELLOW}Configurando password para el usuario 'postgres' de PostgreSQL.${NC}"
echo -e "${YELLOW}Este password se usara en la connection string del proyecto.${NC}"
echo ""
read -s -p "Ingresa el password para postgres (o Enter para usar 'postgres'): " PG_PASSWORD
echo ""

if [[ -z "$PG_PASSWORD" ]]; then
    PG_PASSWORD="postgres"
    warn "Usando password por defecto: 'postgres'"
fi

sudo -u postgres psql -c "ALTER USER postgres PASSWORD '$PG_PASSWORD';"
ok "Password de postgres configurado"

# Configurar pg_hba.conf para autenticacion con password desde localhost
PG_HBA=$(sudo -u postgres psql -tAc "SHOW hba_file;" 2>/dev/null)
if [[ -n "$PG_HBA" ]]; then
    if ! sudo grep -qE "^host[[:space:]]+all[[:space:]]+all[[:space:]]+127\.0\.0\.1" "$PG_HBA" 2>/dev/null; then
        info "Configurando autenticacion en pg_hba.conf..."
        sudo bash -c "echo 'host    all             all             127.0.0.1/32            scram-sha-256' >> $PG_HBA"
        sudo systemctl reload postgresql
        ok "pg_hba.conf actualizado"
    fi
fi

# Actualizar connection string en appsettings.json
APPSETTINGS="$(dirname "$0")/AniLog.API/appsettings.json"
if [[ -f "$APPSETTINGS" ]]; then
    info "Actualizando connection string en appsettings.json..."
    sed -i "s|Host=localhost;Database=anilog_db;Username=postgres;Password=.*\"|Host=localhost;Database=anilog_db;Username=postgres;Password=$PG_PASSWORD\"|" "$APPSETTINGS"
    ok "appsettings.json actualizado"
fi

# =============================================================
# PASO 7: Verificacion final
# =============================================================
echo ""
echo "============================================="
echo "   Verificacion final                        "
echo "============================================="

pass=true

echo -n "  .NET 8 SDK    : "
if dotnet --version 2>/dev/null | grep -q "^8"; then
    ok "$(dotnet --version)"
else
    err "NO OK"; pass=false
fi

echo -n "  EF Core tools : "
if dotnet tool list --global 2>/dev/null | grep -q "dotnet-ef"; then
    ok "$(dotnet tool list --global | grep dotnet-ef | awk '{print $2}')"
else
    err "NO OK"; pass=false
fi

echo -n "  PostgreSQL    : "
if command -v psql &>/dev/null; then
    ok "$(psql --version)"
else
    err "NO OK"; pass=false
fi

echo -n "  Servicio PG   : "
if systemctl is-active --quiet postgresql; then
    ok "activo"
else
    err "inactivo"; pass=false
fi

echo -n "  Base de datos : "
DB_OK=$(sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='anilog_db'" 2>/dev/null || echo "")
if [[ "$DB_OK" == "1" ]]; then
    ok "anilog_db existe"
else
    err "anilog_db no encontrada"; pass=false
fi

echo -n "  Node.js       : "
if command -v node &>/dev/null; then
    ok "$(node --version)"
else
    err "NO OK"; pass=false
fi

echo -n "  git           : "
if command -v git &>/dev/null; then
    ok "$(git --version)"
else
    err "NO OK"; pass=false
fi

echo ""

if $pass; then
    echo -e "${GREEN}=============================================${NC}"
    echo -e "${GREEN}   Entorno listo. Podes empezar a codear!   ${NC}"
    echo -e "${GREEN}=============================================${NC}"
    echo ""
    echo "  Connection string configurada:"
    echo -e "  ${YELLOW}Host=localhost;Database=anilog_db;Username=postgres;Password=$PG_PASSWORD${NC}"
    echo ""
    echo "  Proximos pasos:"
    echo "    cd AniLog.API"
    echo "    dotnet restore"
    echo "    dotnet ef database update"
    echo "    dotnet run"
    echo ""
    echo -e "  ${YELLOW}IMPORTANTE: Abre una nueva terminal (o ejecuta 'exec fish')${NC}"
    echo -e "  ${YELLOW}para que dotnet-ef quede disponible en el PATH.${NC}"
else
    echo -e "${RED}Algo no quedo bien instalado. Revisa los errores arriba.${NC}"
    exit 1
fi
