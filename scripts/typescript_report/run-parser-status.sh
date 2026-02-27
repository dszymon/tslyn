#!/bin/bash
set -e

# Define paths
REPO_ROOT=$(git rev-parse --show-toplevel)
PARSER_STATUS_PROJ="$REPO_ROOT/src/Compilers/TypeScript/Tools/ParserStatus/Microsoft.CodeAnalysis.TypeScript.ParserStatus.csproj"
TEMP_DIR="$REPO_ROOT/temp/TypeScript_Clone"
TYPESCRIPT_REPO="https://github.com/microsoft/TypeScript.git"
TARGET_DIR="$TEMP_DIR/src/compiler"

# Create temp directory if not exists
mkdir -p "$TEMP_DIR"

# Clone TypeScript repo if not already cloned
if [ ! -d "$TEMP_DIR/.git" ]; then
    echo "Cloning TypeScript repository..."
    git clone --depth 1 "$TYPESCRIPT_REPO" "$TEMP_DIR"
else
    echo "TypeScript repository already exists. Pulling latest changes..."
    cd "$TEMP_DIR"
    git pull
    cd "$REPO_ROOT"
fi

# Build the Parser Status tool
echo "Building Parser Status tool..."
dotnet build "$PARSER_STATUS_PROJ" -c Release

# Run the tool
echo "Running Parser Status tool against $TARGET_DIR..."
dotnet run --project "$PARSER_STATUS_PROJ" -c Release -- "$TARGET_DIR"

# Cleanup (optional, comment out if you want to keep the repo for debugging)
# rm -rf "$TEMP_DIR"

echo "Done."
