#!/bin/bash
#
# Removes Memory Monitor installed by install.sh.
#
#   ./uninstall.sh               Ask whether to also delete your settings and logs
#   ./uninstall.sh --yes         Also delete settings and logs
#   ./uninstall.sh --no          Keep settings and logs
#
# MangoHud's config and game logs are never touched; they belong to MangoHud.

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

DATA_HOME="${XDG_DATA_HOME:-$HOME/.local/share}"
CONFIG_HOME="${XDG_CONFIG_HOME:-$HOME/.config}"
STATE_HOME="${XDG_STATE_HOME:-$HOME/.local/state}"
INSTALL_DIR="$DATA_HOME/memory-monitor"
DESKTOP_FILE_NAME="memory-monitor.desktop"
DESKTOP_DIR="$(xdg-user-dir DESKTOP 2>/dev/null || echo "$HOME/Desktop")"

ANSWER_ALL=""
case "$1" in
    --yes) ANSWER_ALL="y" ;;
    --no) ANSWER_ALL="n" ;;
    "") ;;
    *) echo "Usage: $0 [--yes|--no]"; exit 1 ;;
esac

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
echo "Memory Monitor - Uninstall"
echo "======================================"
echo ""

if pgrep -f "$INSTALL_DIR/app/Memory Monitor" >/dev/null 2>&1; then
    echo -e "${YELLOW}Memory Monitor is running. Please exit it (right-click > Exit) and run uninstall.sh again.${NC}"
    exit 1
fi

rm -f "$HOME/.local/bin/memory-monitor"
rm -f "$DATA_HOME/applications/$DESKTOP_FILE_NAME"
rm -f "$CONFIG_HOME/autostart/$DESKTOP_FILE_NAME"
rm -f "$DESKTOP_DIR/$DESKTOP_FILE_NAME"
for size in 16 24 32 48 64 128 256; do
    rm -f "$DATA_HOME/icons/hicolor/${size}x${size}/apps/memory-monitor.png"
done
rm -f "$DATA_HOME/icons/hicolor/scalable/apps/memory-monitor.svg"
rm -rf "$INSTALL_DIR"

if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q "$DATA_HOME/applications" 2>/dev/null || true
fi
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -f -t -q "$DATA_HOME/icons/hicolor" 2>/dev/null || true
fi

echo -e "${GREEN}Removed the app, launcher, menu entry, icons, startup entry and desktop shortcut.${NC}"
echo ""

if ask "Also delete your Memory Monitor settings, logs and crash dumps?"; then
    rm -rf "$CONFIG_HOME/memory-monitor" "$STATE_HOME/memory-monitor"
    echo -e "${GREEN}Deleted settings and logs.${NC}"
else
    echo "Kept your settings ($CONFIG_HOME/memory-monitor) and logs ($STATE_HOME/memory-monitor)."
fi

echo ""
echo "Memory Monitor has been uninstalled."
