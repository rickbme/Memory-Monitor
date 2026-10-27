#!/bin/bash
#
# Installs Memory Monitor for the current user (no sudo needed).
# Run it from the unpacked release folder. Running it again upgrades an existing install.
#
#   ./install.sh                 Ask about each optional step
#   ./install.sh --yes           Accept every optional step (autostart, desktop shortcut, MangoHud)
#   ./install.sh --no            Skip every optional step
#
# Installs:
#   ~/.local/share/memory-monitor/             App files, README, uninstall.sh
#   ~/.local/bin/memory-monitor                Launcher (logs, crash dumps)
#   ~/.local/share/applications/               App menu entry
#   ~/.local/share/icons/hicolor/              Icons
# Optional:
#   ~/.config/autostart/                       Start at login
#   ~/Desktop/                                 Desktop shortcut
#   MangoHud.conf                              FPS logging for Memory Monitor

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

SOURCE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

DATA_HOME="${XDG_DATA_HOME:-$HOME/.local/share}"
CONFIG_HOME="${XDG_CONFIG_HOME:-$HOME/.config}"
INSTALL_DIR="$DATA_HOME/memory-monitor"
BIN_DIR="$HOME/.local/bin"
LAUNCHER="$BIN_DIR/memory-monitor"
APPLICATIONS_DIR="$DATA_HOME/applications"
ICON_DIR="$DATA_HOME/icons/hicolor"
DESKTOP_FILE_NAME="memory-monitor.desktop"
AUTOSTART_FILE="$CONFIG_HOME/autostart/$DESKTOP_FILE_NAME"

ANSWER_ALL=""
case "$1" in
    --yes) ANSWER_ALL="y" ;;
    --no) ANSWER_ALL="n" ;;
    "") ;;
    *) echo "Usage: $0 [--yes|--no]"; exit 1 ;;
esac

# ask "Question" -> returns 0 for yes. Defaults to no (and to no when not run from a terminal).
ask() {
    if [ -n "$ANSWER_ALL" ]; then
        [ "$ANSWER_ALL" = "y" ]
        return
    fi
    if [ ! -t 0 ]; then
        return 1
    fi
    local reply
    read -r -p "$1 [y/N] " reply
    [[ "$reply" =~ ^[Yy] ]]
}

echo "======================================"
echo "Memory Monitor - Linux Install"
echo "======================================"
echo ""

if [ ! -x "$SOURCE_DIR/app/Memory Monitor" ]; then
    echo -e "${RED}Can't find app/Memory Monitor next to this script.${NC}"
    echo "Run install.sh from the unpacked memory-monitor release folder."
    exit 1
fi

if [ "$(uname -m)" != "x86_64" ]; then
    echo -e "${RED}This build is for 64-bit x86 (x86_64) Linux; this machine is $(uname -m).${NC}"
    exit 1
fi

if pgrep -f "$INSTALL_DIR/app/Memory Monitor" >/dev/null 2>&1; then
    echo -e "${YELLOW}Memory Monitor is running. Please exit it (right-click > Exit) and run install.sh again.${NC}"
    exit 1
fi

# --- App files --------------------------------------------------------------
if [ -d "$INSTALL_DIR/app" ]; then
    echo -e "${YELLOW}Upgrading existing install in $INSTALL_DIR${NC}"
    rm -rf "$INSTALL_DIR/app"
else
    echo -e "${YELLOW}Installing to $INSTALL_DIR${NC}"
fi
mkdir -p "$INSTALL_DIR"
cp -r "$SOURCE_DIR/app" "$INSTALL_DIR/app"
cp "$SOURCE_DIR/README.md" "$SOURCE_DIR/uninstall.sh" "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/uninstall.sh" "$INSTALL_DIR/app/Memory Monitor"

# --- Launcher ---------------------------------------------------------------
# Logs output and saves a crash dump if the app crashes, so problems can be diagnosed
mkdir -p "$BIN_DIR"
cat > "$LAUNCHER" <<EOF
#!/bin/sh
# Memory Monitor launcher (installed by install.sh)
STATE_DIR="\${XDG_STATE_HOME:-\$HOME/.local/state}/memory-monitor"
DUMP_DIR="\$STATE_DIR/crash-dumps"
LOG_FILE="\$STATE_DIR/memory-monitor.log"
mkdir -p "\$DUMP_DIR"

# Keep the log under about 1 MB and only the 5 newest crash dumps
if [ -f "\$LOG_FILE" ] && [ "\$(stat -c %s "\$LOG_FILE")" -gt 1048576 ]; then
    mv -f "\$LOG_FILE" "\$LOG_FILE.1"
fi
ls -1t "\$DUMP_DIR"/core.* 2>/dev/null | tail -n +6 | xargs -r rm -f --

# Write a small crash dump (thread stacks) if the app crashes
export DOTNET_DbgEnableMiniDump=1
export DOTNET_DbgMiniDumpType=1
export DOTNET_DbgMiniDumpName="\$DUMP_DIR/core.%p"

echo "=== \$(date '+%Y-%m-%d %H:%M:%S') starting Memory Monitor ===" >> "\$LOG_FILE"
exec "$INSTALL_DIR/app/Memory Monitor" "\$@" >> "\$LOG_FILE" 2>&1
EOF
chmod +x "$LAUNCHER"

# --- Icons ------------------------------------------------------------------
for size in 16 24 32 48 64 128 256; do
    mkdir -p "$ICON_DIR/${size}x${size}/apps"
    cp "$SOURCE_DIR/icons/memory-monitor-$size.png" "$ICON_DIR/${size}x${size}/apps/memory-monitor.png"
done
mkdir -p "$ICON_DIR/scalable/apps"
cp "$SOURCE_DIR/icons/memory-monitor.svg" "$ICON_DIR/scalable/apps/memory-monitor.svg"
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -f -t -q "$ICON_DIR" 2>/dev/null || true
fi

# --- App menu entry ---------------------------------------------------------
mkdir -p "$APPLICATIONS_DIR"
cat > "$APPLICATIONS_DIR/$DESKTOP_FILE_NAME" <<EOF
[Desktop Entry]
Type=Application
Name=Memory Monitor
GenericName=System Monitor
Comment=CPU, GPU, memory, disk and network gauges for 1920x480 mini displays
Exec=$LAUNCHER
Icon=memory-monitor
Terminal=false
Categories=System;Monitor;
Keywords=cpu;gpu;ram;memory;fps;monitor;
StartupWMClass=Memory Monitor
StartupNotify=false
EOF
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q "$APPLICATIONS_DIR" 2>/dev/null || true
fi

echo -e "${GREEN}Installed the app, the 'memory-monitor' command and the app menu entry.${NC}"
echo ""

# --- Optional: start at login -----------------------------------------------
if ask "Start Memory Monitor automatically when you log in?"; then
    mkdir -p "$(dirname "$AUTOSTART_FILE")"
    cp "$APPLICATIONS_DIR/$DESKTOP_FILE_NAME" "$AUTOSTART_FILE"
    echo "X-GNOME-Autostart-enabled=true" >> "$AUTOSTART_FILE"
    echo -e "${GREEN}Added to startup applications.${NC}"
elif [ -f "$AUTOSTART_FILE" ]; then
    rm -f "$AUTOSTART_FILE"
    echo "Removed from startup applications."
fi

# --- Optional: desktop shortcut ---------------------------------------------
DESKTOP_DIR="$(xdg-user-dir DESKTOP 2>/dev/null || echo "$HOME/Desktop")"
if [ -d "$DESKTOP_DIR" ] && ask "Add a desktop shortcut?"; then
    cp "$APPLICATIONS_DIR/$DESKTOP_FILE_NAME" "$DESKTOP_DIR/$DESKTOP_FILE_NAME"
    chmod +x "$DESKTOP_DIR/$DESKTOP_FILE_NAME"
    # Let GNOME's desktop icons launch it without the "untrusted" prompt
    gio set "$DESKTOP_DIR/$DESKTOP_FILE_NAME" metadata::trusted true 2>/dev/null || true
    echo -e "${GREEN}Added a desktop shortcut.${NC}"
fi

# --- Optional: MangoHud FPS logging -----------------------------------------
# The Steam snap can only write inside its own home folder, so it gets its own config and log folder
if [ -d "$HOME/snap/steam/common" ]; then
    MANGOHUD_CONFIG="$HOME/snap/steam/common/.config/MangoHud/MangoHud.conf"
    MANGOHUD_LOGS="$HOME/snap/steam/common/mangohud-logs"
else
    MANGOHUD_CONFIG="$CONFIG_HOME/MangoHud/MangoHud.conf"
    MANGOHUD_LOGS="$HOME/mangohud-logs"
fi

echo ""
if [ -f "$MANGOHUD_CONFIG" ]; then
    if grep -q "^output_folder=$MANGOHUD_LOGS" "$MANGOHUD_CONFIG"; then
        echo "MangoHud is already set up for game FPS."
    else
        echo -e "${YELLOW}A MangoHud config already exists, so it was left alone:${NC} $MANGOHUD_CONFIG"
        echo "For game FPS in Memory Monitor it needs these lines (see README.md):"
        echo "  output_folder=$MANGOHUD_LOGS"
        echo "  autostart_log=1"
        echo "  log_interval=500"
    fi
elif ask "Set up MangoHud so Memory Monitor can show game FPS?"; then
    mkdir -p "$(dirname "$MANGOHUD_CONFIG")" "$MANGOHUD_LOGS"
    cat > "$MANGOHUD_CONFIG" <<EOF
# Log FPS automatically so Memory Monitor can show it (games launched with: mangohud %command%)
output_folder=$MANGOHUD_LOGS
autostart_log=1
log_interval=500
EOF
    echo -e "${GREEN}Created $MANGOHUD_CONFIG${NC}"
    echo "Add 'mangohud %command%' to a game's Steam launch options to see its FPS."
fi

echo ""
echo -e "${GREEN}======================================"
echo -e "Memory Monitor is installed!"
echo -e "======================================${NC}"
echo ""
echo "Start it from the app menu, or run: memory-monitor"
echo "To uninstall: $INSTALL_DIR/uninstall.sh"
if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then
    echo ""
    echo -e "${YELLOW}Note:${NC} $BIN_DIR isn't on your PATH yet. Log out and back in, or use the app menu."
fi
echo ""
