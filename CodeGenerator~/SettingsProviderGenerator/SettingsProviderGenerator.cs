using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SettingsProviderGenerator;

[Generator]
public class SettingsProviderGenerator : ISourceGenerator
{
    private class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<ClassDeclarationSyntax> ProjectSettingsClassDeclarations = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax {AttributeLists.Count: > 0} classDeclaration &&
                IsDerivedFromScriptableObject(classDeclaration))
            {
                var projectSettingsAttributeDeclaration = GetProjectSettingsAttributeDeclaration(classDeclaration);
                var hasSettingsProviderAttribute = projectSettingsAttributeDeclaration != default;
                if (hasSettingsProviderAttribute &&
                    !HasDeclaredCustomSettingsProviderClass(projectSettingsAttributeDeclaration!))
                {
                    ProjectSettingsClassDeclarations.Add(classDeclaration);
                }
            }
        }

        private static bool IsDerivedFromScriptableObject(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.BaseList?.Types.Any(t =>
                t.Type.ToString() is "ScriptableObject" or "UnityEngine.ScriptableObject") == true;
        }

        private static AttributeSyntax? GetProjectSettingsAttributeDeclaration(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists.SelectMany(l => l.Attributes)
                .FirstOrDefault(a =>
                    a.Name.ToString() is "ProjectSettings"
                        or "LizardOfTheCoast.ProjectSettings.ProjectSettingsAttribute") ?? default;
        }

        private static bool HasDeclaredCustomSettingsProviderClass(AttributeSyntax projectSettingsAttributeDeclaration)
        {
            if (projectSettingsAttributeDeclaration.ArgumentList == null)
                return false;

            var arguments = projectSettingsAttributeDeclaration.ArgumentList.Arguments;
            return arguments.Any(a =>
                "LizardOfTheCoast.ProjectSettings.ProjectSettingsAttribute.SettingsProviderMode.CustomProvider"
                    .Contains(a.ToString()));
        }
    }
    
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        GenerateSettingsProviderClasses(context);
    }

    private void GenerateSettingsProviderClasses(GeneratorExecutionContext context)
    {
        foreach (var classDeclaration in ((SyntaxReceiver) context.SyntaxReceiver!).ProjectSettingsClassDeclarations)
        {
            var className = classDeclaration.Identifier.ToString();
            var sourceCode = GenerateSettingsProviderSourceCode(classDeclaration);
            context.AddSource($"{className}Provider.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static string GenerateSettingsProviderSourceCode(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var className = classDeclarationSyntax.Identifier.ToString();
        var classFullName = $"{GetNamespaceFrom(classDeclarationSyntax) ?? string.Empty}.{className}";
        var providerClassName = $"{className}Provider";
        var namespaceValue =
            $"{GetNamespaceFrom(classDeclarationSyntax) ?? "LizardOfTheCoast.ProjectSettings.SettingsProviders"}";

        return $@"#if UNITY_EDITOR

namespace {namespaceValue}
{{
    public static class {providerClassName}
    {{
        [UnityEditor.SettingsProviderAttribute]
        public static UnityEditor.SettingsProvider CreateSettingsProvider() => LizardOfTheCoast.ProjectSettings.SettingsProviderHelper.Create<{classFullName}>();
    }}
}}

#endif // UNITY_EDITOR";
    }

    private static string? GetNamespaceFrom(SyntaxNode s) =>
        s.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
            null => null,
            _ => GetNamespaceFrom(s.Parent)
        };
}