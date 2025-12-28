// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language
{
	public static class RazorCodeDocumentExtensions
	{
		private static readonly char[] PathSeparators = new char[] { '/', '\\' };
		private static readonly char[] NamespaceSeparators = new char[] { '.' };
		private static readonly object CssScopeKey = new object();

		public static TagHelperDocumentContext GetTagHelperContext( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (TagHelperDocumentContext)document.Items[typeof( TagHelperDocumentContext )];
		}

		public static void SetTagHelperContext( this RazorCodeDocument document, TagHelperDocumentContext context )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( TagHelperDocumentContext )] = context;
		}

		internal static IReadOnlyList<TagHelperDescriptor> GetTagHelpers( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (document.Items[typeof( TagHelpersHolder )] as TagHelpersHolder)?.TagHelpers;
		}

		internal static void SetTagHelpers( this RazorCodeDocument document, IReadOnlyList<TagHelperDescriptor> tagHelpers )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( TagHelpersHolder )] = new TagHelpersHolder( tagHelpers );
		}

		public static RazorSyntaxTree GetSyntaxTree( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return document.Items[typeof( RazorSyntaxTree )] as RazorSyntaxTree;
		}

		public static void SetSyntaxTree( this RazorCodeDocument document, RazorSyntaxTree syntaxTree )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( RazorSyntaxTree )] = syntaxTree;
		}

		public static IReadOnlyList<RazorSyntaxTree> GetImportSyntaxTrees( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (document.Items[typeof( ImportSyntaxTreesHolder )] as ImportSyntaxTreesHolder)?.SyntaxTrees;
		}

		public static void SetImportSyntaxTrees( this RazorCodeDocument document, IReadOnlyList<RazorSyntaxTree> syntaxTrees )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( ImportSyntaxTreesHolder )] = new ImportSyntaxTreesHolder( syntaxTrees );
		}

		public static DocumentIntermediateNode GetDocumentIntermediateNode( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return document.Items[typeof( DocumentIntermediateNode )] as DocumentIntermediateNode;
		}

		public static void SetDocumentIntermediateNode( this RazorCodeDocument document, DocumentIntermediateNode documentNode )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( DocumentIntermediateNode )] = documentNode;
		}

		internal static RazorHtmlDocument GetHtmlDocument( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			var razorHtmlObj = document.Items[typeof( RazorHtmlDocument )];
			if ( razorHtmlObj == null )
			{
				var razorHtmlDocument = RazorHtmlWriter.GetHtmlDocument( document );
				if ( razorHtmlDocument != null )
				{
					document.Items[typeof( RazorHtmlDocument )] = razorHtmlDocument;
					return razorHtmlDocument;
				}
			}

			return (RazorHtmlDocument)razorHtmlObj;
		}

		public static RazorCSharpDocument GetCSharpDocument( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (RazorCSharpDocument)document.Items[typeof( RazorCSharpDocument )];
		}

		public static void SetCSharpDocument( this RazorCodeDocument document, RazorCSharpDocument csharp )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( RazorCSharpDocument )] = csharp;
		}

		public static RazorParserOptions GetParserOptions( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (RazorParserOptions)document.Items[typeof( RazorParserOptions )];
		}

		public static void SetParserOptions( this RazorCodeDocument document, RazorParserOptions parserOptions )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( RazorParserOptions )] = parserOptions;
		}

		public static RazorCodeGenerationOptions GetCodeGenerationOptions( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (RazorCodeGenerationOptions)document.Items[typeof( RazorCodeGenerationOptions )];
		}

		public static void SetCodeGenerationOptions( this RazorCodeDocument document, RazorCodeGenerationOptions codeGenerationOptions )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( RazorCodeGenerationOptions )] = codeGenerationOptions;
		}

		public static string GetFileKind( this RazorCodeDocument document )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			return (string)document.Items[typeof( FileKinds )];
		}

		public static void SetFileKind( this RazorCodeDocument document, string fileKind )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			document.Items[typeof( FileKinds )] = fileKind;
		}

		// In general documents will have a relative path (relative to the project root).
		// We can only really compute a nice namespace when we know a relative path.
		//
		// However all kinds of thing are possible in tools. We shouldn't barf here if the document isn't
		// set up correctly.
		public static bool TryComputeNamespace( this RazorCodeDocument document, bool fallbackToRootNamespace, out string @namespace )
		{
			if ( document == null )
			{
				throw new ArgumentNullException( nameof( document ) );
			}

			// If the document or it's imports contains a @namespace directive, we want to use that over the root namespace.
			var importSyntaxTrees = document.GetImportSyntaxTrees();
			if ( importSyntaxTrees != null )
			{
				// ImportSyntaxTrees is usually set. Just being defensive.
				foreach ( var importSyntaxTree in importSyntaxTrees )
				{
					if ( importSyntaxTree != null && NamespaceVisitor.TryGetLastNamespaceDirective( importSyntaxTree, out var importNamespaceContent, out var importNamespaceLocation ) )
					{
						@namespace = importNamespaceContent;
						return true;
					}
				}
			}

			var syntaxTree = document.GetSyntaxTree();
			if ( syntaxTree != null && NamespaceVisitor.TryGetLastNamespaceDirective( syntaxTree, out var namespaceContent, out var namespaceLocation ) )
			{
				@namespace = namespaceContent;
				return true;
			}

			@namespace = "";
			return true;
		}

		private class ImportSyntaxTreesHolder
		{
			public ImportSyntaxTreesHolder( IReadOnlyList<RazorSyntaxTree> syntaxTrees )
			{
				SyntaxTrees = syntaxTrees;
			}

			public IReadOnlyList<RazorSyntaxTree> SyntaxTrees { get; }
		}

		private class IncludeSyntaxTreesHolder
		{
			public IncludeSyntaxTreesHolder( IReadOnlyList<RazorSyntaxTree> syntaxTrees )
			{
				SyntaxTrees = syntaxTrees;
			}

			public IReadOnlyList<RazorSyntaxTree> SyntaxTrees { get; }
		}

		private class TagHelpersHolder
		{
			public TagHelpersHolder( IReadOnlyList<TagHelperDescriptor> tagHelpers )
			{
				TagHelpers = tagHelpers;
			}

			public IReadOnlyList<TagHelperDescriptor> TagHelpers { get; }
		}

		private class NamespaceVisitor : SyntaxWalker
		{
			private readonly RazorSourceDocument _source;

			private NamespaceVisitor( RazorSourceDocument source )
			{
				_source = source;
			}

			public string LastNamespaceContent { get; set; }

			public SourceSpan LastNamespaceLocation { get; set; }

			public static bool TryGetLastNamespaceDirective(
				RazorSyntaxTree syntaxTree,
				out string namespaceDirectiveContent,
				out SourceSpan namespaceDirectiveSpan )
			{
				var visitor = new NamespaceVisitor( syntaxTree.Source );
				visitor.Visit( syntaxTree.Root );
				if ( string.IsNullOrEmpty( visitor.LastNamespaceContent ) )
				{
					namespaceDirectiveContent = null;
					namespaceDirectiveSpan = SourceSpan.Undefined;
					return false;
				}

				namespaceDirectiveContent = visitor.LastNamespaceContent;
				namespaceDirectiveSpan = visitor.LastNamespaceLocation;
				return true;
			}

			public override void VisitRazorDirective( RazorDirectiveSyntax node )
			{
				if ( node != null && node.DirectiveDescriptor == NamespaceDirective.Directive )
				{
					var directiveContent = node.Body?.GetContent();

					// In practice, this should never be null and always start with 'namespace'. Just being defensive here.
					if ( directiveContent != null && directiveContent.StartsWith( NamespaceDirective.Directive.Directive, StringComparison.Ordinal ) )
					{
						LastNamespaceContent = directiveContent.Substring( NamespaceDirective.Directive.Directive.Length ).Trim();
						LastNamespaceLocation = node.GetSourceSpan( _source );
					}
				}

				base.VisitRazorDirective( node );
			}
		}
	}
}
