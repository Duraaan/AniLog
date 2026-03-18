#!/bin/bash

# =============================================================
#  AniLog — Setup del entorno de desarrollo
#  Ubuntu 24.04 x86_64
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
        HAS_DOTNET=false
    fi
fi

if ! $HAS_DOTNET; then
    info "Instalando .NET 8 SDK..."

    # Registrar repositorio de Microsoft
    wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
        -O /tmp/packages-microsoft-prod.deb
    sudo dpkg -i /tmp/packages-microsoft-prod.deb
    rm /tmp/packages-microsoft-prod.deb

    sudo apt-get update -q
    sudo apt-get install -y dotnet-sdk-8.0

    # Verificar
    if command -v dotnet &>/dev/null; then
        ok ".NET SDK instalado: $(dotnet --version)"
    else
        err "Fallo la instalacion de .NET SDK"
        exit 1
    fi
fi

# =============================================================
# PASO 3: Instalar EF Core tools
# =============================================================
if $HAS_EFTOOLS; then
    ok "EF Core tools ya esta instalado"
else
    info "Instalando EF Core tools..."
    dotnet tool install --global dotnet-ef

    # Agregar ~/.dotnet/tools al PATH si no esta
    DOTNET_TOOLS_PATH="$HOME/.dotnet/tools"
    if [[ ":$PATH:" != *":$DOTNET_TOOLS_PATH:"* ]]; then
        warn "Agregando ~/.dotnet/tools al PATH en ~/.bashrc"
        echo '' >> ~/.bashrc
        echo '# .NET tools' >> ~/.bashrc
        echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
        export PATH="$PATH:$DOTNET_TOOLS_PATH"
    fi

    ok "EF Core tools instalado"
fi

# =============================================================
# PASO 4: Instalar PostgreSQL
# =============================================================
if $HAS_PSQL; then
    ok "PostgreSQL ya esta instalado: $(psql --version)"
else
    info "Instalando PostgreSQL..."
    sudo apt-get install -y postgresql postgresql-contrib
    ok "PostgreSQL instalado: $(psql --version)"
fi

# Asegurar que el servicio este corriendo
if ! systemctl is-active --quiet postgresql; then
    info "Iniciando servicio PostgreSQL..."
    sudo systemctl start postgresql
    sudo systemctl enable postgresql
fi
ok "Servicio PostgreSQL activo"

# =============================================================
# PASO 5: Crear base de datos y usuario para AniLog
# =============================================================
info "Configurando base de datos AniLog..."

# Verificar si la DB ya existe
DB_EXISTS=$(sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='anilog_db'" 2>/dev/null || echo "")

if [[ "$DB_EXISTS" == "1" ]]; then
    ok "Base de datos 'anilog_db' ya existe"
else
    sudo -u postgres psql -c "CREATE DATABASE anilog_db;" 2>/dev/null
    ok "Base de datos 'anilog_db' creada"
fi

# Configurar password del usuario postgres (para la connection string)
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

sudo -u postgres psql -c "ALTER USER postgres PASSWORD '$PG_PASSWORD';" 2>/dev/null
ok "Password de postgres configurado"

# Configurar pg_hba.conf para permitir login con password
PG_HBA=$(sudo -u postgres psql -tAc "SHOW hba_file;" 2>/dev/null)
if [[ -n "$PG_HBA" ]]; then
    # Verificar si ya tiene configuracion md5 o scram para local
    if ! sudo grep -q "^host.*all.*all.*127.0.0.1" "$PG_HBA" 2>/dev/null; then
        info "Configurando autenticacion en pg_hba.conf..."
        sudo bash -c "echo 'host    all             all             127.0.0.1/32            scram-sha-256' >> $PG_HBA"
        sudo systemctl reload postgresql
    fi
fi

# =============================================================
# PASO 6: Inicializar git
# =============================================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if [[ -d ".git" ]]; then
    ok "Repositorio git ya inicializado"
else
    info "Inicializando repositorio git..."
    git init
    ok "Repositorio git inicializado"
fi

# Crear .gitignore si no existe
if [[ ! -f ".gitignore" ]]; then
    info "Creando .gitignore..."
    cat > .gitignore << 'EOF'
# .NET
bin/
obj/
*.user
*.suo
.vs/
.vscode/
*.DotSettings.user
appsettings.Development.json

# Secretos - NUNCA commitear
appsettings.local.json

# Node / React
node_modules/
dist/
.env
.env.local

# Sistema
.DS_Store
Thumbs.db
EOF
    ok ".gitignore creado"
else
    ok ".gitignore ya existe"
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
    echo "  Connection string para appsettings.json:"
    echo -e "  ${YELLOW}Host=localhost;Database=anilog_db;Username=postgres;Password=$PG_PASSWORD${NC}"
    echo ""
    echo "  Proximos pasos:"
    echo "    1. dotnet new webapi -n AniLog.API"
    echo "    2. cd AniLog.API"
    echo "    3. dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL"
    echo "    4. dotnet add package Microsoft.EntityFrameworkCore.Design"
    echo ""
    echo -e "  ${YELLOW}IMPORTANTE: Ejecuta 'source ~/.bashrc' o abre una nueva terminal${NC}"
    echo -e "  ${YELLOW}para que EF tools quede disponible en el PATH.${NC}"
else
    echo -e "${RED}Algo no quedo bien instalado. Revisa los errores arriba.${NC}"
    exit 1
fi
