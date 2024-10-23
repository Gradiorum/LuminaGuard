import os

# Define the root directory (current directory where the script is placed)
ROOT_DIR = os.path.dirname(os.path.abspath(__file__))

# Define the output file name
OUTPUT_FILE = os.path.join(ROOT_DIR, "LuminaGuard_Code_Compilation.txt")

# Define file extensions to include
INCLUDE_EXTENSIONS = {'.cs', '.xaml', '.csproj'}

# Define directories to exclude
EXCLUDE_DIRS = {'bin', 'obj', '.git', 'packages'}

def compile_code(root_dir, output_file):
    with open(output_file, 'w', encoding='utf-8') as outfile:
        for subdir, dirs, files in os.walk(root_dir):
            # Modify dirs in-place to skip excluded directories
            dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
            
            for file in files:
                file_ext = os.path.splitext(file)[1].lower()
                if file_ext in INCLUDE_EXTENSIONS:
                    file_path = os.path.join(subdir, file)
                    # Get relative path for better readability
                    relative_path = os.path.relpath(file_path, root_dir)
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f:
                            content = f.read()
                        # Write a header for each file
                        outfile.write(f"\n{'='*80}\n")
                        outfile.write(f"File: {relative_path}\n")
                        outfile.write(f"{'='*80}\n\n")
                        # Write the file content
                        outfile.write(content)
                        outfile.write("\n\n")
                    except Exception as e:
                        print(f"Failed to read {file_path}: {e}")
    
    print(f"Code compilation complete. Output file created at: {output_file}")

if __name__ == "__main__":
    compile_code(ROOT_DIR, OUTPUT_FILE)
