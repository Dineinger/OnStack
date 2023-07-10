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

                foreach (var classWithAttribute in classesWithAttribute)
                {
                    var namespaceOfType = GetNamespace(classWithAttribute.SyntaxTree);
                    var accessModifier = GetAccessModifier(classWithAttribute);
                    var fields = GetFields(classWithAttribute);
                    var typeId = classWithAttribute.Identifier.Text;
                    var isNullableEnabled = IsNullableEnabled(classWithAttribute);

                    var onStackTypeInfo = new OnStackTypeInfo()
                    {
                        AccessModifier = accessModifier,
                        Namespace = namespaceOfType,
                        TypeName = typeId,
                        IsNullableEnabled = true,
                        Fields = GetFieldsInfo(fields),
                    };

                    var fileText = OnStackStructCreator.CreateClassForFile(onStackTypeInfo)
                        .ToString();

                    context.AddSource($"{onStackTypeInfo.TypeName}.g.cs", fileText);
                }
            });

    }

    private static bool IsNullableEnabled(ClassDeclarationSyntax syntaxTree)
    {
        return true; // check if project and file and type as null enabled or not
    }

    private static string GetNamespace(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .Select(ns => ns.Name.ToString())
            .FirstOrDefault()
        ?? syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .Select(ns => ns.Name.ToString())
            .First();
    }

    private static TypeAccessModifier GetAccessModifier(ClassDeclarationSyntax classesWithAttribute)
    {
        foreach (var token in classesWithAttribute.DescendantTokens())
        {
            if (token.IsKind(SyntaxKind.PublicKeyword))
            {
                return TypeAccessModifier.Public;
            }
            else if (token.IsKind(SyntaxKind.InternalKeyword))
            {
                return TypeAccessModifier.Internal;
            }
            else continue;
        }

        return TypeAccessModifier.Internal;
        /*
        class X { }
        is seen as internal by the compiler
        */
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
}

