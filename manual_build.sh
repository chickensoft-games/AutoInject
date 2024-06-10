#!/bin/bash

# Copy source files from Chickensoft.AutoInject.Tests/src/**/*.cs
# to Chickensoft.AutoInject/src/**/*.cs
#
# Because source-only packages are hard to develop and test, we
# actually keep the source that goes in the source-only package inside
# the test project to make it easier to develop and test.
#
# we can always copy it right before publishing the package.

mkdir -p Chickensoft.AutoInject/src
cp -v -r Chickensoft.AutoInject.Tests/src/* Chickensoft.AutoInject/src/
# Define the multiline prefix and suffix
PREFIX="#pragma warning disable
#nullable enable
"
SUFFIX="
#nullable restore
#pragma warning restore"

# Function to add prefix and suffix to a file
add_prefix_suffix() {
    local file="$1"
    # Create a temporary file
    tmp_file=$(mktemp)

    # Add prefix, content of the file, and suffix to the temporary file
    {
        echo "$PREFIX"
        cat "$file"
        echo "$SUFFIX"
    } > "$tmp_file"

    # Move the temporary file to the original file
    mv "$tmp_file" "$file"
}

# Export the function and variables so they can be used by find
export -f add_prefix_suffix
export PREFIX
export SUFFIX

# Find all files and apply the function
find Chickensoft.AutoInject/src -type f -name "*.cs" -exec bash -c 'add_prefix_suffix "$0"' {} \;

cd Chickensoft.AutoInject
dotnet build -c Release

# Delete everything copied into Chickensoft.AutoInject/src
rm -r src

# Recreate folder and .gitkeep file
mkdir src
touch src/.gitkeep
