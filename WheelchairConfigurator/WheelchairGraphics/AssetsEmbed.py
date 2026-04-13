import os
import sys

def sanitize_name(path):
    return path.replace("\\", "_").replace("/", "_").replace(".", "_").replace("-", "_")

def collect_files(input_dir):
    files = []
    for root, _, filenames in os.walk(input_dir):
        for f in filenames:
            full_path = os.path.join(root, f)
            rel_path = os.path.relpath(full_path, input_dir)
            files.append((full_path, rel_path))
    return files

def write_header(header_path):
    with open(header_path, "w") as h:
        h.write("#pragma once\n")
        h.write("#include <cstdint>\n")
        h.write("#include <string>\n")
        h.write("#include <unordered_map>\n\n")

        h.write("struct EmbeddedAsset {\n")
        h.write("    const uint8_t* data;\n")
        h.write("    size_t size;\n")
        h.write("};\n\n")

        h.write("const EmbeddedAsset* getEmbeddedAsset(const std::string& name);\n")

def write_cpp(cpp_path, header_name, files):
    with open(cpp_path, "w") as cpp:
        cpp.write(f'#include "{header_name}"\n\n')

        asset_entries = []

        for full_path, rel_path in files:
            var_name = sanitize_name(rel_path)

            with open(full_path, "rb") as f:
                data = f.read()

            cpp.write(f"static const uint8_t {var_name}[] = {{\n")

            for i, b in enumerate(data):
                cpp.write(f"0x{b:02x},")
                if i % 16 == 15:
                    cpp.write("\n")

            cpp.write("\n};\n")
            cpp.write(f"static const size_t {var_name}_size = {len(data)};\n\n")

            asset_entries.append((rel_path.replace("\\", "/"), var_name))

        # Map
        cpp.write("static const std::unordered_map<std::string, EmbeddedAsset> assetMap = {\n")
        for path, var_name in asset_entries:
            cpp.write(f'    {{"{path}", {{ {var_name}, {var_name}_size }} }},\n')
        cpp.write("};\n\n")

        cpp.write("const EmbeddedAsset* getEmbeddedAsset(const std::string& name) {\n")
        cpp.write("    auto it = assetMap.find(name);\n")
        cpp.write("    if (it != assetMap.end()) return &it->second;\n")
        cpp.write("    return nullptr;\n")
        cpp.write("}\n")

def main():
    if len(sys.argv) < 3:
        print("Usage: python embed_assets.py <input_folder> <output_name>")
        return

    input_dir = sys.argv[1]
    output_name = sys.argv[2]

    header_path = output_name + ".h"
    cpp_path = output_name + ".cpp"

    files = collect_files(input_dir)

    print(f"Embedding {len(files)} files...")

    write_header(header_path)
    write_cpp(cpp_path, os.path.basename(header_path), files)

    print(f"Generated {header_path} and {cpp_path}")

if __name__ == "__main__":
    main()