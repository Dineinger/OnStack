using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnStackGenerator;

[Generator]
public class OnStackGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationIncrementalValue = context.CompilationProvider;

        context.RegisterSourceOutput(
            compilationIncrementalValue,
            static (context, compilation) =>
            {
                var classesWithAttribute = GetClassesWithAttribute(compilation);

                StringBuilder sb = new StringBuilder();
                CreateHeader(sb, "OnStackTests");

                foreach (var a in classesWithAttribute)
                {
                    if (a is null)
                    {
                        continue;
                    }

                    var fields = GetFields(a);


                    CreateClassAndStruct(sb, a.Identifier.Text);

                    CreateFields(sb, fields);

                    CloseClassAndStruct(sb);
                }


                context.AddSource("OnStack.g.cs", sb.ToString());
            });

    }

    private static void CreateFields(StringBuilder sb, IEnumerable<FieldDeclarationSyntax> fields)
    {
        foreach (var field in fields)
        {
            var type = GetFieldType(field);
            var name = GetFieldName(field);

            sb.Append("        public ");
            sb.Append(type);
            sb.Append(" ");
            sb.Append(name);
            sb.AppendLine(";");
            sb.AppendLine();
        }
    }

    private static string GetFieldType(FieldDeclarationSyntax field)
    {
        return field.DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .Select(variable => variable
                .DescendantNodes()
                .OfType<PredefinedTypeSyntax>()
                .Select(type => type
                    .DescendantTokens()
                    .Select(token => token.Text)
                    .First()
                )
                .First()
            )
            .First();
    }

    private static string GetFieldName(FieldDeclarationSyntax field)
    {
        return field.DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .Select(variable => variable
                .DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .First()
                .Identifier
                .Text)
            .First();
    }

    private static IEnumerable<FieldDeclarationSyntax> GetFields(ClassDeclarationSyntax a)
    {
        return a.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .Where(field => field
                .DescendantTokens()
                .Any(token => token.IsKind(SyntaxKind.PublicKeyword)));
    }

    private static IEnumerable<ClassDeclarationSyntax> GetClassesWithAttribute(Compilation compilation)
    {
        return compilation.SyntaxTrees
            .SelectMany(st => st.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(r => r.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a =>
                    {
                        return a.Name.GetText().ToString() == "OnStack";
                    })));
    }

    private static void CreateHeader(StringBuilder sb, string @namespace)
    {
        sb.AppendLine("// Auto-generated code");
        sb.AppendLine();
        sb.Append("namespace ");
        sb.AppendLine(@namespace);
        sb.Append(";");
        sb.AppendLine();
    }

    private static void CreateClassAndStruct(StringBuilder sb, string typeId)
    {
        sb.Append("public partial class ").Append(typeId).AppendLine(" {");
        sb.AppendLine("    public struct OnStack {");
    }

    private static void CloseClassAndStruct(StringBuilder sb)
    {
        sb.AppendLine("    }");
        sb.AppendLine("}");
    }
}

