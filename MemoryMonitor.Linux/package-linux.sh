#!/bin/bash
#
# Builds Memory Monitor for Linux and packages it for release:
#   bin/dist/memory-monitor-<version>-linux-x64.tar.gz
#
# The tarball contains the self-contained app (no .NET install needed), icons,
# install.sh / uninstall.sh and the README. Users unpack it and run ./install.sh.

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_FILE="$PROJECT_DIR/MemoryMonitor.Linux.csproj"
PUBLISH_DIR="$PROJECT_DIR/bin/Release/net8.0/linux-x64/publish"
DIST_DIR="$PROJECT_DIR/bin/dist"

VERSION=$(grep -oP '(?<=<Version>)[^<]+' "$PROJECT_FILE")
if [ -z "$VERSION" ]; then
    echo -e "${RED}Could not read <Version> from $PROJECT_FILE${NC}"
    exit 1
fi
PACKAGE_NAME="memory-monitor-$VERSION-linux-x64"
STAGING_DIR="$DIST_DIR/$PACKAGE_NAME"

echo "======================================"
echo "Memory Monitor $VERSION - Linux Package"
echo "======================================"
echo ""

echo -e "${YELLOW}Cleaning previous build...${NC}"
rm -rf "$PROJECT_DIR/bin/Release" "$PROJECT_DIR/obj" "$STAGING_DIR" "$DIST_DIR/$PACKAGE_NAME.tar.gz"

echo -e "${YELLOW}Publishing self-contained Linux build...${NC}"
dotnet publish "$PROJECT_FILE" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishReadyToRun=true

echo -e "${YELLOW}Assembling package...${NC}"
mkdir -p "$STAGING_DIR"
cp -r "$PUBLISH_DIR" "$STAGING_DIR/app"
cp -r "$PROJECT_DIR/Packaging/icons" "$STAGING_DIR/icons"
cp "$PROJECT_DIR/Packaging/install.sh" "$PROJECT_DIR/Packaging/uninstall.sh" "$STAGING_DIR/"
cp "$PROJECT_DIR/README.md" "$STAGING_DIR/"
chmod +x "$STAGING_DIR/install.sh" "$STAGING_DIR/uninstall.sh" "$STAGING_DIR/app/Memory Monitor"

echo -e "${YELLOW}Creating tarball...${NC}"
tar -czf "$DIST_DIR/$PACKAGE_NAME.tar.gz" -C "$DIST_DIR" "$PACKAGE_NAME"

echo ""
echo -e "${GREEN}======================================"
echo -e "Package created successfully!"
echo -e "======================================${NC}"
echo ""
echo "  $DIST_DIR/$PACKAGE_NAME.tar.gz ($(du -h "$DIST_DIR/$PACKAGE_NAME.tar.gz" | cut -f1))"
echo ""
echo "To install:"
echo "  tar -xzf $PACKAGE_NAME.tar.gz"
echo "  cd $PACKAGE_NAME"
echo "  ./install.sh"
echo ""
