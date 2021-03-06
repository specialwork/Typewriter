﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Typewriter.Metadata.Interfaces;

namespace Typewriter.Metadata.Roslyn
{
    public class RoslynFileMetadata : IFileMetadata
    {
        private readonly Document document;
        private readonly SyntaxNode root;
        private readonly SemanticModel semanticModel;

        public RoslynFileMetadata(Document document)
        {
            this.document = document;
            this.semanticModel = document.GetSemanticModelAsync().Result;
            this.root = semanticModel.SyntaxTree.GetRoot();

            var v = semanticModel.Compilation.GetSpecialType(SpecialType.System_Void);
        }

        public string Name => document.Name;
        public string FullName => document.FilePath;
        
        public IEnumerable<IClassMetadata> Classes => RoslynClassMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<ClassDeclarationSyntax>());
        public IEnumerable<IDelegateMetadata> Delegates => RoslynDelegateMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<DelegateDeclarationSyntax>());
        public IEnumerable<IEnumMetadata> Enums => RoslynEnumMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<EnumDeclarationSyntax>());
        public IEnumerable<IInterfaceMetadata> Interfaces => RoslynInterfaceMetadata.FromNamedTypeSymbols(GetNamespaceChildNodes<InterfaceDeclarationSyntax>());

        private IEnumerable<INamedTypeSymbol> GetNamespaceChildNodes<T>() where T : SyntaxNode
        {
            return root.ChildNodes().OfType<T>().Concat(root.ChildNodes().OfType<NamespaceDeclarationSyntax>()
                .SelectMany(n => n.ChildNodes().OfType<T>()))
                .Select(c => semanticModel.GetDeclaredSymbol(c) as INamedTypeSymbol);

            //return root.ChildNodes().OfType<NamespaceDeclarationSyntax>()
            //    .SelectMany(n => n.ChildNodes().OfType<T>())
            //    .Select(c => semanticModel.GetDeclaredSymbol(c) as INamedTypeSymbol);
        }
    }
}
