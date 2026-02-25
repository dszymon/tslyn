import os

target_dir = "src/Tools/Source/CompilerGeneratorTools/Source/TypeScriptSyntaxGenerator"

replacements = {
    "CSharpSyntaxGenerator": "TypeScriptSyntaxGenerator",
    "Microsoft.CodeAnalysis.CSharp": "Microsoft.CodeAnalysis.TypeScript",
    "CSharpSyntaxNode": "TypeScriptSyntaxNode",
    "CSharpSyntaxVisitor": "TypeScriptSyntaxVisitor",
    "CSharpSyntaxRewriter": "TypeScriptSyntaxRewriter",
    "CSharp.Generated.g4": "TypeScript.Generated.g4",
    "CSharpResources": "TypeScriptResources",
    "CSharp.Syntax": "TypeScript.Syntax",
    "CSharpSyntaxTree": "TypeScriptSyntaxTree",
    "CSharpSyntaxNodeCache": "TypeScriptSyntaxNodeCache",
}

for root, dirs, files in os.walk(target_dir):
    for file in files:
        if file.endswith(".cs") or file.endswith(".csproj"):
            filepath = os.path.join(root, file)
            with open(filepath, "r") as f:
                content = f.read()

            new_content = content
            for search, replace in replacements.items():
                new_content = new_content.replace(search, replace)

            if new_content != content:
                print(f"Modifying {filepath}")
                with open(filepath, "w") as f:
                    f.write(new_content)
