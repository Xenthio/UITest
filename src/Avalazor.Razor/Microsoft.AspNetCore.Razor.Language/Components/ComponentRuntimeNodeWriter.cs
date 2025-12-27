// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.Razor.Language.Components
{
	/// <summary>
	/// Generates the C# code corresponding to Razor source document contents.
	/// </summary>
	internal class ComponentRuntimeNodeWriter : ComponentNodeWriter
	{
		private readonly List<IntermediateToken> _currentAttributeValues = new List<IntermediateToken>();
		private readonly ScopeStack _scopeStack = new ScopeStack();
		private int _sourceSequence;

		public override void WriteCSharpCode( CodeRenderingContext context, CSharpCodeIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			var isWhitespaceStatement = true;
			for ( var i = 0; i < node.Children.Count; i++ )
			{
				var token = node.Children[i] as IntermediateToken;
				if ( token == null || !string.IsNullOrWhiteSpace( token.Content ) )
				{
					isWhitespaceStatement = false;
					break;
				}
			}

			if ( isWhitespaceStatement )
			{
				// The runtime and design time code differ in their handling of whitespace-only
				// statements. At runtime we can discard them completely. At design time we need
				// to keep them for the editor.
				return;
			}

			IDisposable linePragmaScope = null;
			if ( node.Source != null )
			{
				linePragmaScope = context.CodeWriter.BuildLinePragma( node.Source.Value, context );
				context.CodeWriter.WritePadding( 0, node.Source.Value, context );
			}

			for ( var i = 0; i < node.Children.Count; i++ )
			{
				if ( node.Children[i] is IntermediateToken token && token.IsCSharp )
				{
					context.AddSourceMappingFor( token );
					context.CodeWriter.Write( token.Content );
				}
				else
				{
					// There may be something else inside the statement like an extension node.
					context.RenderNode( node.Children[i] );
				}
			}

			if ( linePragmaScope != null )
			{
				linePragmaScope.Dispose();
			}
			else
			{
				context.CodeWriter.WriteLine();
			}
		}

		public override void WriteCSharpExpression( CodeRenderingContext context, CSharpExpressionIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			var sourceSequenceAsString = _sourceSequence.ToString( CultureInfo.InvariantCulture );
			var methodInvocation = _scopeStack.BuilderVarName + '.' + ComponentsApi.RenderTreeBuilder.AddContent + '(' + sourceSequenceAsString;
			_sourceSequence++;

			using ( context.CodeWriter.BuildLinePragma( node.Source.Value, context ) )
			{
				// Since we're not in the middle of writing an element, this must evaluate as some
				// text to display
				context.CodeWriter
					.Write( methodInvocation )
					.WriteParameterSeparator();

				for ( var i = 0; i < node.Children.Count; i++ )
				{
					if ( node.Children[i] is IntermediateToken token && token.IsCSharp )
					{
						WriteCSharpToken( context, token, includeLinePragma: false );
					}
					else
					{
						// There may be something else inside the expression like a Template or another extension node.
						context.RenderNode( node.Children[i] );
					}
				}
				context.CodeWriter.WriteEndMethodInvocation();
			}
		}

		public override void WriteCSharpExpressionAttributeValue( CodeRenderingContext context, CSharpExpressionAttributeValueIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			// In cases like "somestring @variable", Razor tokenizes it as:
			//  [0] HtmlContent="somestring"
			//  [1] CsharpContent="variable" Prefix=" "
			// ... so to avoid losing whitespace, convert the prefix to a further token in the list
			if ( !string.IsNullOrEmpty( node.Prefix ) )
			{
				_currentAttributeValues.Add( new IntermediateToken() { Kind = TokenKind.Html, Content = node.Prefix } );
			}

			for ( var i = 0; i < node.Children.Count; i++ )
			{
				_currentAttributeValues.Add( (IntermediateToken)node.Children[i] );
			}
		}

		public override void WriteMarkupBlock( CodeRenderingContext context, MarkupBlockIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );
			context.CodeWriter.WriteSetCurrentFileLine( _scopeStack.BuilderVarName, node.Source );

			context.CodeWriter
				.Write( $"{_scopeStack.BuilderVarName}.AddMarkupContent( {seq}, " )
				.WriteStringLiteral( node.Content )
				.WriteLine( " );" );
		}

		public override void WriteMarkupElement( CodeRenderingContext context, MarkupElementIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );
			var isPanelClass = node.LooksLikeAPanelClass;
			var isRenderFragment = node.Annotations.Any( x => x.Key is string key && key == "DeclaringType" && x.Value is string str && !string.IsNullOrWhiteSpace( str ) );

			if ( node.TagName == "style" && node.Body.Count() > 0 )
			{
				context.CodeWriter.WriteSetCurrentFileLine( _scopeStack.BuilderVarName, node.Source );
				using var lineScope = context.CodeWriter.BuildLinePragma( node.Source, context );
				context.CodeWriter.Write( $"{_scopeStack.BuilderVarName}.AddStyleDefinitions( {seq}, " );

				int iBlockCount = 0;
				foreach ( var child in node.Body )
				{
					if ( iBlockCount > 0 )
					{
						context.CodeWriter.Write( "+" );
					}

					if ( child is HtmlContentIntermediateNode htmlNode )
					{
						context.CodeWriter.WriteStringLiteral( GetHtmlContent( htmlNode ) );
					}
					else if ( child is MarkupBlockIntermediateNode markupBlock )
					{
						context.CodeWriter.WriteStringLiteral( markupBlock.Content );
					}
					else if ( child is CSharpExpressionIntermediateNode cs )
					{
						for ( var i = 0; i < cs.Children.Count; i++ )
						{
							if ( cs.Children[i] is IntermediateToken token && token.IsCSharp )
							{
								context.CodeWriter.Write( token.Content );
							}
						}
					}

					iBlockCount++;
				}

				context.CodeWriter.Write( " );" );
				context.CodeWriter.WriteLine();

				return;
			}

			string panelClassName = null;

			context.CodeWriter.WriteSetCurrentFileLine( _scopeStack.BuilderVarName, node.Source );

			string key = "null";
			var keyAttribute = node.Children.OfType<HtmlAttributeIntermediateNode>().Where( x => x.AttributeName == "@key" ).FirstOrDefault();
			if ( keyAttribute != null )
			{
				var child = keyAttribute.Children.FirstOrDefault();
				if ( child != null )
				{
					key = ((IntermediateToken)child.Children.Single()).Content;
				}
			}

			if ( isRenderFragment )
			{
				var parentType = node.Annotations["DeclaringType"] as string;
				var contextAttribute = node.Children.OfType<HtmlAttributeIntermediateNode>().Where( x => x.AttributeName == "Context" ).FirstOrDefault()?.Children?.FirstOrDefault();
				var tag = node.TagName;

				using var lineScope = context.CodeWriter.BuildLinePragma( node.Source, context );

				if ( contextAttribute != null )
				{
					// this is the name of the variable
					var contextValue = key = ((IntermediateToken)contextAttribute.Children.Single()).Content;

					context.CodeWriter
						.WriteLine( $"{_scopeStack.BuilderVarName}.SetRenderFragmentWithContext( ( {parentType} x ) => x.{tag}, (x, y) => x.{tag} = y, ( {contextValue} ) => ( Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder ) =>" );
				}
				else
				{
					context.CodeWriter
						.WriteLine( $"{_scopeStack.BuilderVarName}.SetRenderFragment<{parentType}>( (x, y) => x.{tag} = y, ( __builder ) =>" );
				}
			}
			else if ( isPanelClass )
			{
				var root = context.Ancestors.OfType<ClassDeclarationIntermediateNode>().LastOrDefault().ClassName;

				// Have we tried to include ourselves - if so, don't
				if ( root == node.TagName )
				{
					context.CodeWriter.WriteLine( $"// Tried to include recursively here" );
					context.CodeWriter.WriteLine( $"throw new System.Exception(\"Tried to include element '{root}' recursively!\");" );
					return;
				}

				panelClassName = node.TagName;

				var genericTypes = node.Children.OfType<HtmlAttributeIntermediateNode>().Where( x => x.AttributeName == "T" ).FirstOrDefault();
				if ( genericTypes != null )
				{
					var values = genericTypes.Children
											.OfType<HtmlAttributeValueIntermediateNode>()
											.SelectMany( x => x.Children.OfType<IntermediateToken>().Select( y => y.Content ) );

					panelClassName = $"{panelClassName}<{string.Join( "", values )}>";
				}

				using var lineScope = context.CodeWriter.BuildLinePragma( node.Source, context );
				context.CodeWriter
					.Write( $"{_scopeStack.BuilderVarName}.OpenElement<{panelClassName}>( {seq} " )
					.Write( $", {key}" )
					.Write( $" );" )
					.WriteLine();
			}
			else
			{
				context.CodeWriter
					.Write( $"{_scopeStack.BuilderVarName}.OpenElement( {seq}, " )
					.WriteStringLiteral( node.TagName )
					.Write( $", {key}" )
					.Write( $" );" )
					.WriteLine();
			}

			var scope = context.CodeWriter.BuildScope();

			// Render attributes and splats (in order) before creating the scope.
			foreach ( var child in node.Children )
			{
				if ( isPanelClass )
					child.Annotations["DeclaringType"] = panelClassName;

				if ( child is HtmlAttributeIntermediateNode attribute )
				{
					WriteHtmlAttribute( context, attribute );
					//context.RenderNode(attribute);
				}
				else if ( child is ComponentAttributeIntermediateNode componentAttribute )
				{
					context.RenderNode( componentAttribute );
				}
				else if ( child is SplatIntermediateNode splat )
				{
					context.RenderNode( splat );
				}
			}

			foreach ( var setKey in node.SetKeys )
			{
				context.RenderNode( setKey );
			}

			// Render body of the tag inside the scope
			foreach ( var child in node.Body )
			{
				context.RenderNode( child );
			}

			scope.Dispose();

			if ( isRenderFragment )
			{
				context.CodeWriter.WriteLine( ");" );
				// nothing
			}
			else if ( isPanelClass )
			{
				context.CodeWriter.WriteLine( $"{_scopeStack.BuilderVarName}.CloseElement();" );
			}
			else
			{
				context.CodeWriter.WriteLine( $"{_scopeStack.BuilderVarName}.CloseElement();" );
			}

		}

		public override void WriteHtmlAttribute( CodeRenderingContext context, HtmlAttributeIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			var tagname = node.Annotations["DeclaringType"] as string;
			var build = _scopeStack.BuilderVarName;

			try
			{
				Debug.Assert( _currentAttributeValues.Count == 0 );
				context.RenderChildren( node );

				// Ignore T attribute - it's used to build the generic type
				if ( node.AttributeName == "T" )
					return;

				if ( node.AttributeName == "@key" ) // handled elsewhere
					return;

				if ( node.AttributeName == "@ref" )
				{
					WriteReferenceCaptureAttribute( context, node );
					return;
				}

				if ( node.AttributeName.StartsWith( "@on" ) )
				{
					WriteEventCallbackAttribute( context, node );
					return;
				}

				//
				// Else treat it as a callback
				//
				if ( node.AttributeName.StartsWith( "@" ) && !string.IsNullOrEmpty( tagname ) )
				{
					WriteCallbackAttribute( context, node, tagname );
					return;
				}

				bool looksLikeAProperty = !string.IsNullOrEmpty( tagname ) && char.IsUpper( node.AttributeName[0] );

				var attributeParts = node.AttributeName.Split( new[] { ':' }, StringSplitOptions.RemoveEmptyEntries );
				var attributeName = attributeParts[0];
				var attributeExtras = attributeParts.Skip( 1 ).Select( x => x ).ToList();

				//
				// Garry: if the parent element is a panel (its first letter is a capital letter) and the attribute
				// name starts with a capital, then we treat it like a property. In which case we should be able to
				// set the value directly instead of converting to and from string/object etc. So we form a function that
				// sets the value, pass it to the render tree - which calls it.
				//
				// todo - we could set up a cache mechanism here so it doesn't get called every render
				//
				// var temp = (value);
				// if ( __builder.IsCached( temp.GetHashCode() ) ) return;
				// o.Property = temp;				 
				//
				if ( looksLikeAProperty )
				{
					using var lineScope = context.CodeWriter.BuildLinePragma( node.Source, context );
					bool isBind = attributeExtras.Contains( "bind", StringComparer.OrdinalIgnoreCase );

					var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );

					if ( isBind )
					{
						context.CodeWriter.Write( $"{build}.AddBind( {seq}, " );


						context.CodeWriter.WriteStringLiteral( attributeName );

						// Us get set
						context.CodeWriter.Write( ", () => " );
						WriteAttributeValue( context, _currentAttributeValues );
						context.CodeWriter.Write( ", ( v ) => " );
						WriteAttributeValue( context, _currentAttributeValues );
						context.CodeWriter.Write( " = v" );

						// finish
						context.CodeWriter.WriteLine( " );" );

						return;
					}

					// AddAttribute<paneltype>( seq, attrName, value )

					using var scope = context.CodeWriter.BuildScope();

					context.CodeWriter.WriteLine( "var __v = (" );

					if ( _currentAttributeValues.Count() > 0 )
					{
						WriteAttributeValue( context, _currentAttributeValues );
					}
					else
					{
						context.CodeWriter.Write( " true " );
					}

					context.CodeWriter.Write( ");" );



					{
						context.CodeWriter.Write( $"{build}.AddAttribute<{tagname}>( {seq}, __v, ( _o ) => _o.{attributeName} = __v );" );
						context.CodeWriter.WriteLine();
					}

					return;
				}

				//
				// Garry: Else just set it like a regular attribute
				//

				if ( node.AttributeNameExpression == null )
				{
					using var liner = context.CodeWriter.BuildLinePragma( node.Source, context );
					WriteAttribute( context, node.AttributeName, _currentAttributeValues );
				}
				else
				{
					using var liner = context.CodeWriter.BuildLinePragma( node.Source, context );
					WriteAttribute( context, node.AttributeNameExpression, _currentAttributeValues );
				}
			}
			finally
			{
				_currentAttributeValues.Clear();
			}

			if ( !string.IsNullOrEmpty( node.EventUpdatesAttributeName ) )
			{
				context.CodeWriter
					.WriteStartMethodInvocation( $"{build}.{ComponentsApi.RenderTreeBuilder.SetUpdatesAttributeName}" )
					.WriteStringLiteral( node.EventUpdatesAttributeName )
					.WriteEndMethodInvocation();
			}
		}

		public override void WriteHtmlAttributeValue( CodeRenderingContext context, HtmlAttributeValueIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			var stringContent = ((IntermediateToken)node.Children.Single()).Content;
			_currentAttributeValues.Add( new IntermediateToken() { Kind = TokenKind.Html, Content = node.Prefix + stringContent, } );
		}

		/// <summary>
		/// Handles @ref attribute
		/// </summary>
		void WriteReferenceCaptureAttribute( CodeRenderingContext context, HtmlAttributeIntermediateNode node )
		{
			var codeWriter = context.CodeWriter;
			var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );

			if ( node.Children.Count() != 1 )
				return;

			var child = node.Children.First() as HtmlAttributeValueIntermediateNode;
			if ( child == null )
				return;

			var stringContent = ((IntermediateToken)child.Children.Single()).Content;

			using ( codeWriter.BuildLinePragma( node.Source, context ) )
			{
				var methodName = "AddReferenceCapture";
				codeWriter
					.Write( $"{_scopeStack.BuilderVarName}.{methodName}( {seq}, " )
					.Write( $"{stringContent}, " )
					.Write( $"( _v ) => {{ {stringContent} = _v; }}" )
					.Write( ");" );
			}
		}

		/// <summary>
		/// Handles @onclick= events (which use the panel's event system)
		/// </summary>
		void WriteEventCallbackAttribute( CodeRenderingContext context, HtmlAttributeIntermediateNode node )
		{
			var codeWriter = context.CodeWriter;
			var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );

			var attributeName = node.AttributeName;

			// strip the @
			attributeName = attributeName.Trim( '@' );

			using ( codeWriter.BuildLinePragma( node.Source, context ) )
			{
				codeWriter
					.Write( $"{_scopeStack.BuilderVarName}.AddAttribute( {seq}, " )
					.Write( $"\"{attributeName}\", " );

				WriteAttributeValue( context, _currentAttributeValues );

				codeWriter.Write( " );" );
			}
		}

		/// <summary>
		/// Handles panel callbacks directly
		/// </summary>
		void WriteCallbackAttribute( CodeRenderingContext context, HtmlAttributeIntermediateNode node, string declaringType )
		{
			var codeWriter = context.CodeWriter;
			var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );

			var attributeName = node.AttributeName;

			// strip the @
			attributeName = attributeName.Trim( '@' );

			using ( codeWriter.BuildLinePragma( node.Source, context ) )
			{
				codeWriter
					.Write( $"{_scopeStack.BuilderVarName}.AddAttribute<{declaringType}>( {seq}, ( o ) => {{  " )
					.Write( $"o.{attributeName} = " );

				WriteAttributeValue( context, _currentAttributeValues );

				codeWriter.Write( "; } );" );
			}
		}

		public override void WriteHtmlContent( CodeRenderingContext context, HtmlContentIntermediateNode node )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( nameof( context ) );
			}

			if ( node == null )
			{
				throw new ArgumentNullException( nameof( node ) );
			}

			var seq = (_sourceSequence++).ToString( CultureInfo.InvariantCulture );

			// Text node
			var content = GetHtmlContent( node );
			if ( node.IsEncoded() )
			{
				context.CodeWriter.WriteSetCurrentFileLine( _scopeStack.BuilderVarName, node.Source );

				context.CodeWriter
					.Write( $"{_scopeStack.BuilderVarName}.AddMarkupContent( {seq}, " )
					.WriteStringLiteral( content )
					.Write( $" );" )
					.WriteLine();
			}
			else
			{
				context.CodeWriter
					.Write( $"{_scopeStack.BuilderVarName}.AddContent( {seq}, " )
					.WriteStringLiteral( content )
					.Write( $" );" )
					.WriteLine();
			}

		}

		public override void WriteUsingDirective( CodeRenderingContext context, UsingDirectiveIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			if ( node.Source.HasValue )
			{
				using ( context.CodeWriter.BuildLinePragma( node.Source.Value, context ) )
				{
					context.CodeWriter.WriteUsing( node.Content );
				}
			}
			else
			{
				context.CodeWriter.WriteUsing( node.Content, endLine: true );
			}
		}

		private IReadOnlyList<IntermediateToken> GetCSharpTokens( IntermediateNode node )
		{
			// We generally expect all children to be CSharp, this is here just in case.
			return node.FindDescendantNodes<IntermediateToken>().Where( t => t.IsCSharp ).ToArray();
		}

		public override void WriteComponentChildContent( CodeRenderingContext context, ComponentChildContentIntermediateNode node )
		{
			if ( context == null ) throw new ArgumentNullException( nameof( context ) );
			if ( node == null ) throw new ArgumentNullException( nameof( node ) );

			using var liner = context.CodeWriter.BuildLinePragma( node.Source, context );

			// Writes something like:
			//
			// _builder.AddAttribute(1, "ChildContent", (RenderFragment)((__builder73) => { ... }));
			// OR
			// _builder.AddAttribute(1, "ChildContent", (RenderFragment<Person>)((person) => (__builder73) => { ... }));
			context.CodeWriter.WriteLine( "// WriteComponentChildContent " );
			BeginWriteAttribute( context, node.AttributeName );
			context.CodeWriter.WriteParameterSeparator();
			context.CodeWriter.Write( $"({node.TypeName})(" );

			WriteComponentChildContentInnards( context, node );

			context.CodeWriter.Write( ")" );
			context.CodeWriter.WriteEndMethodInvocation();
		}

		private void WriteComponentChildContentInnards( CodeRenderingContext context, ComponentChildContentIntermediateNode node )
		{
			// Writes something like:
			//
			// ((__builder73) => { ... })
			// OR
			// ((person) => (__builder73) => { })
			_scopeStack.OpenComponentScope(
				context,
				node.AttributeName,
				node.IsParameterized ? node.ParameterName : null );
			for ( var i = 0; i < node.Children.Count; i++ )
			{
				context.RenderNode( node.Children[i] );
			}
			_scopeStack.CloseScope( context );
		}

		public override void WriteComponentTypeArgument( CodeRenderingContext context, ComponentTypeArgumentIntermediateNode node )
		{
			// We can skip type arguments during runtime codegen, they are handled in the
			// type/parameter declarations.
		}

		public override void WriteTemplate( CodeRenderingContext context, TemplateIntermediateNode node )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( nameof( context ) );
			}

			if ( node == null )
			{
				throw new ArgumentNullException( nameof( node ) );
			}

			// Looks like:
			//
			// (__builder73) => { ... }
			_scopeStack.OpenTemplateScope( context );
			context.RenderChildren( node );
			_scopeStack.CloseScope( context );
		}

		public override void WriteSetKey( CodeRenderingContext context, SetKeyIntermediateNode node )
		{
			// Looks like:
			//
			// _builder.SetKey(_keyValue);

			var codeWriter = context.CodeWriter;

			codeWriter
				.WriteStartMethodInvocation( $"{_scopeStack.BuilderVarName}.{ComponentsApi.RenderTreeBuilder.SetKey}" );
			WriteSetKeyInnards( context, node );
			codeWriter.WriteEndMethodInvocation();
		}

		private void WriteSetKeyInnards( CodeRenderingContext context, SetKeyIntermediateNode node )
		{
			context.CodeWriter.WriteLine( "// DEBUG: WriteSetKeyInnards" );

			WriteCSharpCode( context, new CSharpCodeIntermediateNode
			{
				Source = node.Source,
				Children =
					{
						node.KeyValueToken
					}
			} );
		}

		public override void WriteSplat( CodeRenderingContext context, SplatIntermediateNode node )
		{
			context.CodeWriter.WriteLine( "// DEBUG: WriteSplat" );

			if ( context == null )
			{
				throw new ArgumentNullException( nameof( context ) );
			}

			if ( node == null )
			{
				throw new ArgumentNullException( nameof( node ) );
			}

			// Looks like:
			//
			// _builder.AddMultipleAttributes(2, ...);
			context.CodeWriter.WriteStartMethodInvocation( $"{_scopeStack.BuilderVarName}.{ComponentsApi.RenderTreeBuilder.AddMultipleAttributes}" );
			context.CodeWriter.Write( (_sourceSequence++).ToString( CultureInfo.InvariantCulture ) );
			context.CodeWriter.WriteParameterSeparator();

			WriteSplatInnards( context, node, canTypeCheck: true );

			context.CodeWriter.WriteEndMethodInvocation();
		}

		private void WriteSplatInnards( CodeRenderingContext context, SplatIntermediateNode node, bool canTypeCheck )
		{
			var tokens = GetCSharpTokens( node );

			if ( canTypeCheck )
			{
				context.CodeWriter.Write( ComponentsApi.RuntimeHelpers.TypeCheck );
				context.CodeWriter.Write( "<" );
				context.CodeWriter.Write( ComponentsApi.AddMultipleAttributesTypeFullName );
				context.CodeWriter.Write( ">" );
				context.CodeWriter.Write( "(" );
			}

			for ( var i = 0; i < tokens.Count; i++ )
			{
				WriteCSharpToken( context, tokens[i] );
			}

			if ( canTypeCheck )
			{
				context.CodeWriter.Write( ")" );
			}
		}

		private void WriteAttribute( CodeRenderingContext context, string key, IList<IntermediateToken> value )
		{
			BeginWriteAttribute( context, key );

			if ( value.Count > 0 )
			{
				context.CodeWriter.WriteParameterSeparator();
				WriteAttributeValue( context, value );
			}
			else if ( !context.Options.OmitMinimizedComponentAttributeValues )
			{
				// In version 5+, there's no need to supply a value for a minimized attribute.
				// But for older language versions, minimized attributes were represented as "true".
				context.CodeWriter.WriteParameterSeparator();
				context.CodeWriter.WriteBooleanLiteral( true );
			}

			context.CodeWriter.WriteEndMethodInvocation();
		}

		private void WriteAttribute( CodeRenderingContext context, IntermediateNode nameExpression, IList<IntermediateToken> value )
		{
			context.CodeWriter.WriteLine( "// nameExpression " );
			BeginWriteAttribute( context, nameExpression );
			if ( value.Count > 0 )
			{
				context.CodeWriter.WriteParameterSeparator();
				WriteAttributeValue( context, value );
			}
			context.CodeWriter.WriteEndMethodInvocation();
		}

		protected override void BeginWriteAttribute( CodeRenderingContext context, string key )
		{
			context.CodeWriter
				.WriteStartMethodInvocation( $"{_scopeStack.BuilderVarName}.{ComponentsApi.RenderTreeBuilder.AddAttribute}" )
				.Write( (_sourceSequence++).ToString( CultureInfo.InvariantCulture ) )
				.WriteParameterSeparator()
				.WriteStringLiteral( key );
		}

		protected override void BeginWriteAttribute( CodeRenderingContext context, IntermediateNode nameExpression )
		{
			context.CodeWriter.WriteStartMethodInvocation( $"{_scopeStack.BuilderVarName}.{ComponentsApi.RenderTreeBuilder.AddAttribute}" );
			context.CodeWriter.Write( (_sourceSequence++).ToString( CultureInfo.InvariantCulture ) );
			context.CodeWriter.WriteParameterSeparator();

			var tokens = GetCSharpTokens( nameExpression );
			for ( var i = 0; i < tokens.Count; i++ )
			{
				WriteCSharpToken( context, tokens[i] );
			}
		}

		private static string GetHtmlContent( HtmlContentIntermediateNode node )
		{
			var builder = new StringBuilder();
			var htmlTokens = node.Children.OfType<IntermediateToken>().Where( t => t.IsHtml );
			foreach ( var htmlToken in htmlTokens )
			{
				builder.Append( htmlToken.Content );
			}
			return builder.ToString();
		}

		// There are a few cases here, we need to handle:
		// - Pure HTML
		// - Pure CSharp
		// - Mixed HTML and CSharp
		//
		// Only the mixed case is complicated, we want to turn it into code that will concatenate
		// the values into a string at runtime.

		private static void WriteAttributeValue( CodeRenderingContext context, IList<IntermediateToken> tokens )
		{
			if ( tokens == null )
			{
				throw new ArgumentNullException( nameof( tokens ) );
			}

			var writer = context.CodeWriter;
			var hasHtml = false;
			var hasCSharp = false;
			for ( var i = 0; i < tokens.Count; i++ )
			{
				if ( tokens[i].IsCSharp )
				{
					hasCSharp |= true;
				}
				else
				{
					hasHtml |= true;
				}
			}

			if ( hasHtml && hasCSharp )
			{
				// If it's a C# expression, we have to wrap it in parens, otherwise things like ternary
				// expressions don't compose with concatenation. However, this is a little complicated
				// because C# tokens themselves aren't guaranteed to be distinct expressions. We want
				// to treat all contiguous C# tokens as a single expression.
				var insideCSharp = false;
				for ( var i = 0; i < tokens.Count; i++ )
				{
					var token = tokens[i];
					if ( token.IsCSharp )
					{
						if ( !insideCSharp )
						{
							if ( i != 0 )
							{
								writer.Write( " + " );
							}

							writer.Write( "(" );
							insideCSharp = true;
						}

						WriteCSharpToken( context, token, false );
					}
					else
					{
						if ( insideCSharp )
						{
							writer.Write( ")" );
							insideCSharp = false;
						}

						if ( i != 0 )
						{
							writer.Write( " + " );
						}

						writer.WriteStringLiteral( token.Content );
					}
				}

				if ( insideCSharp )
				{
					writer.Write( ")" );
				}
			}
			else if ( hasCSharp )
			{
				foreach ( var token in tokens )
				{
					WriteCSharpToken( context, token, false );
				}
			}
			else if ( hasHtml )
			{
				writer.WriteStringLiteral( string.Join( "", tokens.Select( t => t.Content ) ) );
			}
			else
			{
				throw new InvalidOperationException( "Found attribute whose value is neither HTML nor CSharp" );
			}
		}

		private static void WriteCSharpToken( CodeRenderingContext context, IntermediateToken token, bool includeLinePragma = true )
		{
			if ( string.IsNullOrWhiteSpace( token.Content ) )
			{
				return;
			}

			if ( token.Source?.FilePath == null )
			{
				context.CodeWriter.Write( token.Content );
				return;
			}

			if ( includeLinePragma )
			{
				using ( context.CodeWriter.BuildLinePragma( token.Source, context ) )
				{
					context.CodeWriter.WritePadding( 0, token.Source.Value, context );
					context.CodeWriter.Write( token.Content );
				}
				return;
			}

			context.CodeWriter.Write( token.Content );
		}
	}
}
