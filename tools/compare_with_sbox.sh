#!/bin/bash

# Script to compare our Sandbox.UI implementation with s&box's code
# This helps identify missing methods, properties, and logic differences

echo "==================================="
echo "Sandbox.UI vs S&box Comparison Tool"
echo "==================================="
echo ""

# Check if s&box repo path is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <path-to-sbox-public-repo>"
    echo ""
    echo "Example:"
    echo "  ./tools/compare_with_sbox.sh ~/repos/sbox-public"
    echo ""
    echo "First, clone s&box repo:"
    echo "  git clone https://github.com/Facepunch/sbox-public.git"
    exit 1
fi

SBOX_PATH="$1"
SBOX_UI_PATH="$SBOX_PATH/engine/Sandbox.Engine/Systems/UI"
OUR_UI_PATH="./src/Sandbox.UI"

if [ ! -d "$SBOX_UI_PATH" ]; then
    echo "Error: S&box UI path not found: $SBOX_UI_PATH"
    echo "Make sure you cloned the s&box-public repository correctly."
    exit 1
fi

echo "S&box UI path: $SBOX_UI_PATH"
echo "Our UI path: $OUR_UI_PATH"
echo ""

# Create output directory
OUTPUT_DIR="./tools/comparison_output"
mkdir -p "$OUTPUT_DIR"

echo "==================================="
echo "1. Finding missing files..."
echo "==================================="

# Get list of .cs files in s&box
find "$SBOX_UI_PATH" -name "*.cs" -type f | sed "s|$SBOX_UI_PATH/||" | sort > "$OUTPUT_DIR/sbox_files.txt"

# Get list of .cs files in our repo
find "$OUR_UI_PATH" -name "*.cs" -type f | sed "s|$OUR_UI_PATH/||" | sort > "$OUTPUT_DIR/our_files.txt"

# Find files in s&box but not in our repo
comm -23 "$OUTPUT_DIR/sbox_files.txt" "$OUTPUT_DIR/our_files.txt" > "$OUTPUT_DIR/missing_files.txt"

echo "Missing files (in s&box but not in ours):"
cat "$OUTPUT_DIR/missing_files.txt"
echo ""

echo "==================================="
echo "2. Comparing common files..."
echo "==================================="

# Find common files
comm -12 "$OUTPUT_DIR/sbox_files.txt" "$OUTPUT_DIR/our_files.txt" > "$OUTPUT_DIR/common_files.txt"

# Compare each common file
echo "Generating diffs for common files..."
while IFS= read -r file; do
    SBOX_FILE="$SBOX_UI_PATH/$file"
    OUR_FILE="$OUR_UI_PATH/$file"
    
    if [ -f "$SBOX_FILE" ] && [ -f "$OUR_FILE" ]; then
        # Generate diff
        diff -u "$SBOX_FILE" "$OUR_FILE" > "$OUTPUT_DIR/diff_$(echo $file | tr '/' '_').txt" 2>&1 || true
    fi
done < "$OUTPUT_DIR/common_files.txt"

echo "Diffs saved to: $OUTPUT_DIR/diff_*.txt"
echo ""

echo "==================================="
echo "3. Analyzing method signatures..."
echo "==================================="

# Extract public method signatures from s&box
echo "Extracting s&box method signatures..."
find "$SBOX_UI_PATH" -name "*.cs" -type f -exec grep -h "public.*(" {} \; | \
    grep -v "^//" | grep -v "^\s*\*" | sort -u > "$OUTPUT_DIR/sbox_methods.txt"

# Extract public method signatures from our code
echo "Extracting our method signatures..."
find "$OUR_UI_PATH" -name "*.cs" -type f -exec grep -h "public.*(" {} \; | \
    grep -v "^//" | grep -v "^\s*\*" | sort -u > "$OUTPUT_DIR/our_methods.txt"

# Find methods in s&box but not in ours
comm -23 "$OUTPUT_DIR/sbox_methods.txt" "$OUTPUT_DIR/our_methods.txt" > "$OUTPUT_DIR/missing_methods.txt"

echo "Potentially missing public methods:"
head -20 "$OUTPUT_DIR/missing_methods.txt"
echo "... (see $OUTPUT_DIR/missing_methods.txt for full list)"
echo ""

echo "==================================="
echo "4. Analyzing properties..."
echo "==================================="

# Extract public properties from s&box
echo "Extracting s&box properties..."
find "$SBOX_UI_PATH" -name "*.cs" -type f -exec grep -h "public.*{.*get.*}" {} \; | \
    grep -v "^//" | grep -v "^\s*\*" | sort -u > "$OUTPUT_DIR/sbox_properties.txt"

# Extract public properties from our code
echo "Extracting our properties..."
find "$OUR_UI_PATH" -name "*.cs" -type f -exec grep -h "public.*{.*get.*}" {} \; | \
    grep -v "^//" | grep -v "^\s*\*" | sort -u > "$OUTPUT_DIR/our_properties.txt"

# Find properties in s&box but not in ours
comm -23 "$OUTPUT_DIR/sbox_properties.txt" "$OUTPUT_DIR/our_properties.txt" > "$OUTPUT_DIR/missing_properties.txt"

echo "Potentially missing public properties:"
head -20 "$OUTPUT_DIR/missing_properties.txt"
echo "... (see $OUTPUT_DIR/missing_properties.txt for full list)"
echo ""

echo "==================================="
echo "5. Key files to review (border-radius related)..."
echo "==================================="

KEY_FILES=(
    "Styles/PanelStyle.cs"
    "Styles/Styles.cs"
    "Styles/Styles.Cached.cs"
    "Styles/Styles.From.cs"
    "Styles/StyleSystem.cs"
    "Panel/Panel.cs"
)

echo "Generating focused diffs for key style-related files..."
for file in "${KEY_FILES[@]}"; do
    SBOX_FILE="$SBOX_UI_PATH/$file"
    OUR_FILE="$OUR_UI_PATH/$file"
    
    if [ -f "$SBOX_FILE" ] && [ -f "$OUR_FILE" ]; then
        echo "  - $file"
        diff -u "$SBOX_FILE" "$OUR_FILE" > "$OUTPUT_DIR/IMPORTANT_diff_$(basename $file).txt" 2>&1 || true
    elif [ -f "$SBOX_FILE" ]; then
        echo "  - $file (MISSING IN OUR REPO!)"
    fi
done
echo ""

echo "==================================="
echo "Summary"
echo "==================================="
echo "All comparison results saved to: $OUTPUT_DIR/"
echo ""
echo "Next steps:"
echo "1. Review missing files: $OUTPUT_DIR/missing_files.txt"
echo "2. Review missing methods: $OUTPUT_DIR/missing_methods.txt"
echo "3. Review missing properties: $OUTPUT_DIR/missing_properties.txt"
echo "4. Check IMPORTANT_diff_*.txt files for critical differences"
echo ""
echo "For border-radius bug specifically, focus on:"
echo "  - IMPORTANT_diff_PanelStyle.cs.txt"
echo "  - IMPORTANT_diff_Styles.cs.txt"
echo "  - IMPORTANT_diff_Styles.Cached.cs.txt"
echo ""
