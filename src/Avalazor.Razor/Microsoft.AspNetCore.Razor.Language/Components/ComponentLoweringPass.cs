// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language.Components
{
	internal class ComponentLoweringPass : ComponentIntermediateNodePassBase, IRazorOptimizationPass
	{
		// This pass runs earlier than our other passes that 'lower' specific kinds of attributes.
		public override int Order => 0;

		protected override void ExecuteCore( RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode )
		{
			if ( !IsComponentDocument( documentNode ) )
			{
				return;
			}

			var @namespace = documentNode.FindPrimaryNamespace();
			var @class = documentNode.FindPrimaryClass();
			if ( @namespace == null || @class == null )
			{
				// Nothing to do, bail. We can't function without the standard structure.
				return;
			}

			// For each component *usage* we need to rewrite the tag helper node to map to the relevant component
			// APIs.
			var references = documentNode.FindDescendantReferences<TagHelperIntermediateNode>();
			for ( var i = 0; i < references.Count; i++ )
			{
				var reference = references[i];
				var node = (TagHelperIntermediateNode)reference.Node;
				if ( node.TagHelpers.Any( t => t.IsChildContentTagHelper() ) )
				{
					// This is a child content tag helper. This will be rewritten when we visit its parent.
					continue;
				}

				reference.Replace( RewriteAsElement( node ) );
			}
		}

		private static MarkupElementIntermediateNode RewriteAsElement( TagHelperIntermediateNode node )
		{
			var result = new MarkupElementIntermediateNode()
			{
				Source = node.Source,
				TagName = node.TagName,
			};

			for ( var i = 0; i < node.Diagnostics.Count; i++ )
			{
				result.Diagnostics.Add( node.Diagnostics[i] );
			}

			var visitor = new ElementRewriteVisitor( result.Children );
			visitor.Visit( node );

			return result;
		}

		private class ElementRewriteVisitor : IntermediateNodeWalker
		{
			private readonly IntermediateNodeCollection _children;

			public ElementRewriteVisitor( IntermediateNodeCollection children )
			{
				_children = children;
			}

			public override void VisitTagHelper( TagHelperIntermediateNode node )
			{
				// Visit children, we're replacing this node.
				for ( var i = 0; i < node.Children.Count; i++ )
				{
					Visit( node.Children[i] );
				}
			}

			public override void VisitTagHelperBody( TagHelperBodyIntermediateNode node )
			{
				for ( var i = 0; i < node.Children.Count; i++ )
				{
					_children.Add( node.Children[i] );
				}
			}

			public override void VisitTagHelperHtmlAttribute( TagHelperHtmlAttributeIntermediateNode node )
			{
				var attribute = new HtmlAttributeIntermediateNode()
				{
					AttributeName = node.AttributeName,
					Source = node.Source,
				};
				_children.Add( attribute );

				for ( var i = 0; i < node.Diagnostics.Count; i++ )
				{
					attribute.Diagnostics.Add( node.Diagnostics[i] );
				}

				switch ( node.AttributeStructure )
				{
					case AttributeStructure.Minimized:

						attribute.Prefix = node.AttributeName;
						attribute.Suffix = string.Empty;
						break;

					case AttributeStructure.NoQuotes:
					case AttributeStructure.SingleQuotes:
					case AttributeStructure.DoubleQuotes:

						// We're ignoring attribute structure here for simplicity, it doesn't effect us.
						attribute.Prefix = node.AttributeName + "=\"";
						attribute.Suffix = "\"";

						for ( var i = 0; i < node.Children.Count; i++ )
						{
							attribute.Children.Add( RewriteAttributeContent( node.Children[i] ) );
						}

						break;
				}

				static IntermediateNode RewriteAttributeContent( IntermediateNode content )
				{
					if ( content is HtmlContentIntermediateNode html )
					{
						var value = new HtmlAttributeValueIntermediateNode()
						{
							Source = content.Source,
						};

						for ( var i = 0; i < html.Children.Count; i++ )
						{
							value.Children.Add( html.Children[i] );
						}

						for ( var i = 0; i < html.Diagnostics.Count; i++ )
						{
							value.Diagnostics.Add( html.Diagnostics[i] );
						}

						return value;
					}


					return content;
				}
			}

			public override void VisitTagHelperProperty( TagHelperPropertyIntermediateNode node )
			{
				// Each 'tag helper property' belongs to a specific tag helper. We want to handle
				// the cases for components, but leave others alone. This allows our other passes
				// to handle those cases.
				_children.Add( node.TagHelper.IsComponentTagHelper() ? (IntermediateNode)new ComponentAttributeIntermediateNode( node ) : node );
			}

			public override void VisitTagHelperDirectiveAttribute( TagHelperDirectiveAttributeIntermediateNode node )
			{
				// We don't want to do anything special with directive attributes here.
				// Let their corresponding lowering pass take care of processing them.
				_children.Add( node );
			}

			public override void VisitDefault( IntermediateNode node )
			{
				_children.Add( node );
			}
		}
	}
}
