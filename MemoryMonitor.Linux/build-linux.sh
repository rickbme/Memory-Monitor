#!/bin/bash

# Build script for Memory Monitor on Linux
# This script builds the application for Linux x64

set -e  # Exit on error

echo "======================================"
echo "Memory Monitor - Linux Build Script"
echo "======================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
OUTPUT_DIR="$SCRIPT_DIR/bin/Release/net8.0/linux-x64/publish"

echo -e "${YELLOW}Cleaning previous build...${NC}"
rm -rf "$PROJECT_DIR/bin" "$PROJECT_DIR/obj"

echo -e "${YELLOW}Restoring NuGet packages...${NC}"
dotnet restore "$PROJECT_DIR/MemoryMonitor.Linux.csproj"

echo -e "${YELLOW}Building project...${NC}"
dotnet build "$PROJECT_DIR/MemoryMonitor.Linux.csproj" -c Release

echo -e "${YELLOW}Publishing self-contained Linux binary...${NC}"
dotnet publish "$PROJECT_DIR/MemoryMonitor.Linux.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishReadyToRun=true

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}======================================"
    echo -e "Build completed successfully!"
    echo -e "======================================${NC}"
    echo ""
    echo "Output directory: $OUTPUT_DIR"
    echo ""
    echo "To run the application:"
    echo "  cd \"$OUTPUT_DIR\""
    echo "  ./Memory\ Monitor"
    echo ""
else
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi
