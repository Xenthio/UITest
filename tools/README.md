# Sandbox.UI Comparison Tools

This directory contains tools to help compare our Sandbox.UI implementation with s&box's official code.

## compare_with_sbox.sh

A comprehensive comparison script that identifies differences between our code and s&box's implementation.

### Usage

1. **Clone the s&box repository:**
   ```bash
   git clone https://github.com/Facepunch/sbox-public.git ~/sbox-public
   ```

2. **Run the comparison script:**
   ```bash
   ./tools/compare_with_sbox.sh ~/sbox-public
   ```

### What It Does

The script performs the following analyses:

1. **Missing Files** - Identifies `.cs` files in s&box that don't exist in our repo
2. **File Diffs** - Generates unified diffs for all common files
3. **Missing Methods** - Finds public methods in s&box that we're missing
4. **Missing Properties** - Finds public properties in s&box that we're missing
5. **Key File Focus** - Highlights critical style-related files for the border-radius bug

### Output

All results are saved to `./tools/comparison_output/`:

- `missing_files.txt` - Files that exist in s&box but not in our repo
- `missing_methods.txt` - Public methods we're missing
- `missing_properties.txt` - Public properties we're missing
- `diff_*.txt` - Detailed diffs for each common file
- `IMPORTANT_diff_*.txt` - Focused diffs for critical style files

### Debugging Border-Radius Bug

For the specific border-radius state bug, review these files first:

1. `IMPORTANT_diff_PanelStyle.cs.txt` - Style resolution and selector matching
2. `IMPORTANT_diff_Styles.cs.txt` - Main style class logic
3. `IMPORTANT_diff_Styles.Cached.cs.txt` - Cached style management

Look for differences in:
- How `BuildCached()` is called
- How `From()` and `Add()` are integrated
- Pseudo-class selector matching logic
- Style finalization in `BuildFinal()`

### Tips

- The script uses `diff -u` format (unified diff) which is easy to read
- Grep for specific methods/properties in the output files
- Focus on logic differences, not just formatting
- Check if methods are called in different orders

### Example Workflow

```bash
# Run comparison
./tools/compare_with_sbox.sh ~/sbox-public

# Check for missing files
cat tools/comparison_output/missing_files.txt

# Search for specific method
grep -r "BuildCached" tools/comparison_output/IMPORTANT_*.txt

# View critical diff
less tools/comparison_output/IMPORTANT_diff_Styles.cs.txt
```

## Future Tools

Additional comparison utilities can be added here:
- AST-based code comparison (beyond text diffing)
- Method call graph comparison
- Property usage analysis
- Test coverage comparison
