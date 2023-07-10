using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using OnStackGenerator.General;
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
                sb.AppendLine("/*");
                //CreateHeader(sb, "OnStackTests");

                foreach (var a in classesWithAttribute)
                {
                    var fields = GetFields(a);
                    var typeId = a.Identifier.Text;

                    var onStackTypeInfo = new OnStackTypeInfo()
                    {
                        AccessModifier = TypeAccessModifier.Public, // TODO: pass the right access modifier
                        Namespace = "OnStackTests", // TODO: pass the right namespace
                        TypeName = typeId,
                        IsNullableEnabled = true, // TODO: look in project if nullable is needed
                        Fields = GetFieldsInfo(fields),
                    };

                    var fileText = OnStackStructCreator.CreateClassForFile(onStackTypeInfo)
                        .ToString();

                    sb.AppendLine(typeId);

                    context.AddSource($"{onStackTypeInfo.TypeName}.g.cs", fileText);

                    //CreateClassAndStruct(sb, typeId);

                    //CreateFields(sb, fields);

                    //CreateAllocateMethod(sb, typeId, fields);

                    //CloseClassAndStruct(sb);
                }

                sb.Append("*/");
                context.AddSource("OnStack.g.cs", sb.ToString());
            });

    }

    private static Field[] GetFieldsInfo(FieldDeclarationSyntax[] fields)
    {
        var result = new Field[fields.Length];
        for (int a = 0; a < fields.Length; a++)
        {
            var rawField = fields[a];
            var field = new Field();
            var fieldType = GetFieldType(rawField);
            field.Name = GetFieldName(rawField);
            field.AccessModifier = FieldAccessModifier.Public; // TODO: get the correct access modifier
            field.IsNullable = fieldType.IsNullable;
            field.Type = fieldType.Type;
            result[a] = field;
        }

        return result;
    }

    private static void CreateAllocateMethod(StringBuilder sb, string typeId, IEnumerable<FieldDeclarationSyntax> fields)
    {
        sb.Append("        public ");
        sb.Append(typeId);
        sb.AppendLine(" Allocate()");
        sb.AppendLine("        {");
        sb.Append("            return new ");
        sb.Append(typeId);
        sb.AppendLine("()");
        sb.AppendLine("            {");
        foreach (var field in fields)
        {
            var name = GetFieldName(field);

            sb.Append("                ");
            sb.Append(name);
            sb.Append(" = this.");
            sb.Append(name);
            sb.AppendLine(",");
        }
        sb.AppendLine("            };");
        sb.AppendLine("        }");
    }

    private static void CreateFields(StringBuilder sb, IEnumerable<FieldDeclarationSyntax> fields)
    {
        foreach (var field in fields)
        {
            var type = GetFieldType(field);
            var name = GetFieldName(field);

            sb.Append("        public ");
            sb.Append(type.Type);
            if (type.IsNullable)
            {
                sb.Append('?');
            }
            sb.Append(" ");
            sb.Append(name);
            sb.AppendLine(";");
            sb.AppendLine();
        }
    }

    private static (bool IsNullable, string Type) GetFieldType(FieldDeclarationSyntax field)
    {
        return field.DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .Select(variable => variable
                .DescendantNodes()
                .OfType<PredefinedTypeSyntax>()
                .Select(type => type
                    .DescendantTokens()
                    .Select(token => (type.Parent is NullableTypeSyntax, token.Text))
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

    private static FieldDeclarationSyntax[] GetFields(ClassDeclarationSyntax a)
    {
        return a.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .Where(field => field
                .DescendantTokens()
                .Any(token => token.IsKind(SyntaxKind.PublicKeyword))).ToArray();
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
        sb.Append(@namespace);
        sb.Append(";");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
    }

    private static void CreateClassAndStruct(StringBuilder sb, string typeId)
    {
        sb.Append("public partial class ").AppendLine(typeId);
        sb.AppendLine("{");
        sb.AppendLine("    public struct OnStack");
        sb.AppendLine("    {");
    }

    private static void CloseClassAndStruct(StringBuilder sb)
    {
        sb.AppendLine("    }");
        sb.AppendLine("}");
    }
}

