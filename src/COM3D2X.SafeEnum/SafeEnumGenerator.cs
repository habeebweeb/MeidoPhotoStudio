using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace COM3D2X.SafeEnum;

[Generator]
public class SafeEnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource(
                "SafeEnum.g.cs",
                SourceText.From(
                    """
                    using System;

                    namespace SafeEnum;

                    [AttributeUsage(AttributeTargets.Class)]
                    internal sealed class SafeEnumAttribute<T> : Attribute
                        where T : struct, Enum
                    {
                    }
                    """,
                    Encoding.UTF8)));

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            "SafeEnum.SafeEnumAttribute`1",
            static (node, _) =>
            {
                if (node is not ClassDeclarationSyntax syntax)
                    return false;

                if (!syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                    return false;

                if (!syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
                    return false;

                return true;
            },
            static (ctx, _) =>
            {
                var attribute = ctx.Attributes.Single();
                var enumType = attribute.AttributeClass!.TypeArguments.Single();

                return new SafeEnumModel()
                {
                    Namespace = GetNameSpace(ctx.TargetNode),
                    Accessibility = GetClassAccessibility(ctx.TargetSymbol),
                    Name = GetClassName(ctx.TargetSymbol),
                    EnumType = GetEnumType(attribute),
                    EnumMembers = GetEnumMembers(enumType),
                };

                static string GetNameSpace(SyntaxNode syntax)
                {
                    var finalNamespace = string.Empty;

                    var namespaceParent = syntax.Parent;

                    while (namespaceParent is not null and not (NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax))
                        namespaceParent = namespaceParent.Parent;

                    if (namespaceParent is BaseNamespaceDeclarationSyntax baseNamespace)
                    {
                        finalNamespace = baseNamespace.Name.ToString();

                        while (true)
                        {
                            if (baseNamespace.Parent is not NamespaceDeclarationSyntax parent)
                                break;

                            baseNamespace = parent;
                            finalNamespace = $"{baseNamespace.Name}.{finalNamespace}";
                        }
                    }

                    return finalNamespace;
                }

                static Accessibility GetClassAccessibility(ISymbol symbol) =>
                    symbol.DeclaredAccessibility;

                static string GetClassName(ISymbol symbol) =>
                    symbol.Name;

                static string GetEnumType(AttributeData attribute)
                {
                    var typeParam = attribute.AttributeClass?.TypeArguments.Single();

                    if (typeParam is not INamedTypeSymbol symbol)
                        return string.Empty;

                    return symbol.ToDisplayString(
                        new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
                }

                static ValueCollection<string> GetEnumMembers(ITypeSymbol symbol) =>
                    new(symbol
                        .GetMembers()
                        .Where(member => member is IFieldSymbol { ConstantValue: { } })
                        .Select(member => member.Name));
            });

        context.RegisterSourceOutput(
            pipeline,
            static (ctx, model) =>
            {
                var sb = new StringBuilder();

                sb.AppendLine(
                    """
                    using System;
                    using System.Collections.Generic;
                    """);

                sb.AppendLine();

                var hasNamespace = !string.IsNullOrEmpty(model.Namespace);

                if (hasNamespace)
                {
                    sb.Append("namespace ");
                    sb.AppendLine(model.Namespace);
                    sb.AppendLine(
                        """
                        {
                        """);
                }

                sb.AppendLine($"    {SyntaxFacts.GetText(model.Accessibility)} static partial class {model.Name}");
                sb.AppendLine(
                    """
                        {
                    """);
                sb.AppendLine(
                    $$"""
                            private static readonly Dictionary<string, {{model.EnumType}}> cache = new Dictionary<string, {{model.EnumType}}>(StringComparer.Ordinal);
                    """);

                foreach (var member in model.EnumMembers)
                {
                    var propertyName = member;

                    if (member.StartsWith("set_", StringComparison.Ordinal) || member.StartsWith("get_", StringComparison.Ordinal))
                        propertyName = string.Concat(char.ToUpper(member[0]), member.Substring(1));

                    sb.AppendLine();
                    sb.AppendLine($"        public static {model.EnumType} {propertyName} =>");
                    sb.AppendLine($"            GetValue(\"{member}\");");
                }

                sb.AppendLine();

                sb.AppendLine(
                    $$"""
                            private static {{model.EnumType}} GetValue(string name) =>
                                cache.TryGetValue(name, out {{model.EnumType}} value)
                                    ? value
                                    : cache[name] = ({{model.EnumType}})Enum.Parse(typeof({{model.EnumType}}), name);
                    """);

                sb.AppendLine(
                    """
                        }
                    """);

                if (hasNamespace)
                    sb.AppendLine(
                        """
                        }
                        """);

                // TODO: Generated the enum class.
                ctx.AddSource(
                    $"Safe{model.Name}.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            });
    }
}
