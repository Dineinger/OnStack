using System;
using System.Text;
namespace OnStackGenerator.General
{
    public static class OnStackStructCreator
    {
        public static StringBuilder CreateClassForFile(OnStackTypeInfo codeInfo, StringBuilder? sb = null)
        {
            sb ??= new StringBuilder();

            sb.AppendLine("// Auto-generated code");
            sb.AppendLine();
            sb.AppendLine("using System.CodeDom.Compiler;");
            sb.AppendLine();
            sb.Append("namespace ").Append(codeInfo.Namespace).AppendLine(";");
            sb.AppendLine();
            if (codeInfo.IsNullableEnabled)
            {
                sb.AppendLine("#nullable enable");
                sb.AppendLine();
            }
            OpenClass(sb, codeInfo);
            OpenStruct(sb);
            AddFields(sb, codeInfo.Fields);
            AddAllocateMethod(sb, codeInfo);
            CloseStruct(sb);
            CloseClass(sb);

            return sb;
        }

        private static void AddAllocateMethod(StringBuilder sb, OnStackTypeInfo codeInfo)
        {
            sb.AppendLine();
            sb.AppendLine("        [GeneratedCode(\"OnStack\", \"Alpha_0\")]");
            sb.Append("        public ");
            sb.Append(codeInfo.TypeName);
            sb.AppendLine(" Allocate()");
            sb.AppendLine("        {");
            sb.Append("            return new ");
            sb.Append(codeInfo.TypeName);
            sb.AppendLine("()");
            sb.AppendLine("            {");
            foreach (var field in codeInfo.Fields)
            {
                var name = field.Name;

                sb.Append("                ");
                sb.Append(name);
                sb.Append(" = this.");
                sb.Append(name);
                sb.AppendLine(",");
            }
            sb.AppendLine("            };");
            sb.AppendLine("        }");
        }

        private static void AddFields(StringBuilder sb, Field[] fields)
        {
            foreach (Field field in fields)
            {
                sb.Append("        ");
                sb.Append(field.AccessModifier switch
                {
                    FieldAccessModifier.Public => "public",
                    FieldAccessModifier.Internal => "internal",
                    FieldAccessModifier.Private => "private",
                    FieldAccessModifier.Protected => "protected",
                    _ => throw new InvalidOperationException("should never happen")
                });
                sb.Append(' ');
                sb.Append(field.Type);
                if (field.IsNullable)
                {
                    sb.Append('?');
                }
                sb.Append(' ');
                sb.Append(field.Name);
                sb.AppendLine(";");
            }
        }

        private static void CloseStruct(StringBuilder sb)
        {
            sb.AppendLine("    }");
        }

        private static void OpenStruct(StringBuilder sb)
        {
            sb.Append("""
                    [GeneratedCode("OnStack", "Alpha_0")]
                    public struct OnStack
                    {
               
                """);
        }

        private static void OpenClass(StringBuilder sb, OnStackTypeInfo codeInfo)
        {
            sb.Append(
                codeInfo.AccessModifier switch
                {
                    TypeAccessModifier.Public => "public",
                    TypeAccessModifier.Internal => "internal",
                    _ => new Exception("should never happen")
                });
            sb.Append(" partial class ");
            sb.AppendLine(codeInfo.TypeName);
            sb.AppendLine("{");
        }

        private static void CloseClass(StringBuilder sb)
        {
            sb.AppendLine("}");
        }
    }
}
