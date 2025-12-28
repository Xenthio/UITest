// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.IO;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Components
{
	internal class ComponentDocumentClassifierPass : DocumentClassifierPassBase
	{
		public const string ComponentDocumentKind = "component.1.0";

		/// <summary>
		/// Gets or sets whether to mangle class names.
		///
		/// Set to true in the IDE so we can generated mangled class names. This is needed
		/// to avoid conflicts between generated design-time code and the code in the editor.
		///
		/// A better workaround for this would be to create a singlefilegenerator that overrides
		/// the codegen process when a document is open, but this is more involved, so hacking
		/// it for now.
		/// </summary>
		public bool MangleClassNames { get; set; }

		protected override string DocumentKind => ComponentDocumentKind;

		// Ensure this runs before the MVC classifiers which have Order = 0
		public override int Order => -100;

		protected override bool IsMatch( RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode )
		{
			return FileKinds.IsComponent( codeDocument.GetFileKind() );
		}

		protected override CodeTarget CreateTarget( RazorCodeDocument codeDocument, RazorCodeGenerationOptions options )
		{
			return new ComponentCodeTarget( options, TargetExtensions );
		}

		/// <inheritdoc />
		protected override void OnDocumentStructureCreated( RazorCodeDocument codeDocument, NamespaceDeclarationIntermediateNode @namespace, ClassDeclarationIntermediateNode @class, MethodDeclarationIntermediateNode method )
		{
			@class.ChecksumHash = Checksum.BytesToString( codeDocument.Source.GetChecksum() );

			if ( !codeDocument.TryComputeNamespace( fallbackToRootNamespace: true, out var computedNamespace ) )
				computedNamespace = "";

			if ( !TryComputeClassName( codeDocument, out var computedClass ) )
			{
				// If we can't compute a nice namespace (no relative path) then just generate something
				// mangled.

				computedClass = $"AspNetCore_{@class.ChecksumHash}";
			}

			var documentNode = codeDocument.GetDocumentIntermediateNode();
			if ( char.IsLower( computedClass, 0 ) )
			{
				// We don't allow component names to start with a lowercase character.
				documentNode.Diagnostics.Add(
					ComponentDiagnosticFactory.Create_ComponentNamesCannotStartWithLowerCase( computedClass, documentNode.Source ) );
			}

			if ( MangleClassNames )
			{
				computedClass = ComponentMetadata.MangleClassName( computedClass );
			}

			@namespace.Content = computedNamespace;
			@class.ClassName = computedClass;
			@class.Modifiers.Clear();
			@class.Modifiers.Add( "partial" );

			@class.BaseType = "global::Sandbox.UI.Panel";

			// Constrained type parameters are only supported in Razor language versions v6.0
			var directiveType = ComponentConstrainedTypeParamDirective.Directive;
			var typeParamReferences = documentNode.FindDirectiveReferences( directiveType );
			for ( var i = 0; i < typeParamReferences.Count; i++ )
			{
				var typeParamNode = (DirectiveIntermediateNode)typeParamReferences[i].Node;
				if ( typeParamNode.HasDiagnostics )
				{
					continue;
				}

				@class.TypeParameters.Add( new TypeParameter()
				{
					ParameterName = typeParamNode.Tokens.First().Content,
					Constraints = typeParamNode.Tokens.Skip( 1 ).FirstOrDefault()?.Content
				} );
			}

			method.ReturnType = "void";
			method.MethodName = ComponentsApi.ComponentBase.BuildRenderTree;
			method.Modifiers.Clear();
			method.Modifiers.Add( "protected" );
			method.Modifiers.Add( "override" );

			method.Parameters.Clear();
			method.Parameters.Add( new MethodParameter()
			{
				ParameterName = ComponentsApi.RenderTreeBuilder.BuilderParameter,
				TypeName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
			} );
		}

		private bool TryComputeClassName( RazorCodeDocument codeDocument, out string className )
		{
			className = null;

			if ( codeDocument.Source.FilePath == null && codeDocument.Source.RelativePath == null )
				return false;

			className = CSharpIdentifier.SanitizeIdentifier( Path.GetFileNameWithoutExtension( codeDocument.Source.FilePath ?? codeDocument.Source.RelativePath ) );
			return true;
		}
	}
}
