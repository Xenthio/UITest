// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language
{
	public static class RazorProjectEngineBuilderExtensions
	{
		/// <summary>
		/// Registers a class configuration delegate that gets invoked during code generation.
		/// </summary>
		/// <param name="builder">The <see cref="RazorProjectEngineBuilder"/>.</param>
		/// <param name="configureClass"><see cref="Action"/> invoked to configure
		/// <see cref="ClassDeclarationIntermediateNode"/> during code generation.</param>
		/// <returns>The <see cref="RazorProjectEngineBuilder"/>.</returns>
		public static RazorProjectEngineBuilder ConfigureClass(
			this RazorProjectEngineBuilder builder,
			Action<RazorCodeDocument, ClassDeclarationIntermediateNode> configureClass )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( nameof( builder ) );
			}

			if ( configureClass == null )
			{
				throw new ArgumentNullException( nameof( configureClass ) );
			}

			var configurationFeature = GetDefaultDocumentClassifierPassFeature( builder );
			configurationFeature.ConfigureClass.Add( configureClass );
			return builder;
		}

		/// <summary>
		/// Sets the base type for generated types.
		/// </summary>
		/// <param name="builder">The <see cref="RazorProjectEngineBuilder"/>.</param>
		/// <param name="baseType">The name of the base type.</param>
		/// <returns>The <see cref="RazorProjectEngineBuilder"/>.</returns>
		public static RazorProjectEngineBuilder SetBaseType( this RazorProjectEngineBuilder builder, string baseType )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( nameof( builder ) );
			}

			var configurationFeature = GetDefaultDocumentClassifierPassFeature( builder );
			configurationFeature.ConfigureClass.Add( ( document, @class ) => @class.BaseType = baseType );
			return builder;
		}

		/// <summary>
		/// Sets the namespace for generated types.
		/// </summary>
		/// <param name="builder">The <see cref="RazorProjectEngineBuilder"/>.</param>
		/// <param name="namespaceName">The name of the namespace.</param>
		/// <returns>The <see cref="RazorProjectEngineBuilder"/>.</returns>
		public static RazorProjectEngineBuilder SetNamespace( this RazorProjectEngineBuilder builder, string namespaceName )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( nameof( builder ) );
			}

			var configurationFeature = GetDefaultDocumentClassifierPassFeature( builder );
			configurationFeature.ConfigureNamespace.Add( ( document, @namespace ) => @namespace.Content = namespaceName );
			return builder;
		}

		/// <summary>
		/// Adds the specified <see cref="ICodeTargetExtension"/>.
		/// </summary>
		/// <param name="builder">The <see cref="RazorProjectEngineBuilder"/>.</param>
		/// <param name="extension">The <see cref="ICodeTargetExtension"/> to add.</param>
		/// <returns>The <see cref="RazorProjectEngineBuilder"/>.</returns>
		public static RazorProjectEngineBuilder AddTargetExtension( this RazorProjectEngineBuilder builder, ICodeTargetExtension extension )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( nameof( builder ) );
			}

			if ( extension == null )
			{
				throw new ArgumentNullException( nameof( extension ) );
			}

			var targetExtensionFeature = GetTargetExtensionFeature( builder );
			targetExtensionFeature.TargetExtensions.Add( extension );

			return builder;
		}

		/// <summary>
		/// Adds the specified <see cref="DirectiveDescriptor"/>.
		/// </summary>
		/// <param name="builder">The <see cref="RazorProjectEngineBuilder"/>.</param>
		/// <param name="directive">The <see cref="DirectiveDescriptor"/> to add.</param>
		/// <returns>The <see cref="RazorProjectEngineBuilder"/>.</returns>
		public static RazorProjectEngineBuilder AddDirective( this RazorProjectEngineBuilder builder, DirectiveDescriptor directive )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( nameof( builder ) );
			}

			if ( directive == null )
			{
				throw new ArgumentNullException( nameof( directive ) );
			}

			var directiveFeature = GetDirectiveFeature( builder );
			directiveFeature.Directives.Add( directive );

			return builder;
		}

		/// <summary>
		/// Adds the specified <see cref="DirectiveDescriptor"/> for the provided file kind.
		/// </summary>
		/// <param name="builder">The <see cref="RazorProjectEngineBuilder"/>.</param>
		/// <param name="directive">The <see cref="DirectiveDescriptor"/> to add.</param>
		/// <param name="fileKinds">The file kinds, for which to register the directive. See <see cref="FileKinds"/>.</param>
		/// <returns>The <see cref="RazorProjectEngineBuilder"/>.</returns>
		public static RazorProjectEngineBuilder AddDirective( this RazorProjectEngineBuilder builder, DirectiveDescriptor directive, params string[] fileKinds )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( nameof( builder ) );
			}

			if ( directive == null )
			{
				throw new ArgumentNullException( nameof( directive ) );
			}

			if ( fileKinds == null )
			{
				throw new ArgumentNullException( nameof( fileKinds ) );
			}

			var directiveFeature = GetDirectiveFeature( builder );

			foreach ( var fileKind in fileKinds )
			{
				if ( !directiveFeature.DirectivesByFileKind.TryGetValue( fileKind, out var directives ) )
				{
					directives = new List<DirectiveDescriptor>();
					directiveFeature.DirectivesByFileKind.Add( fileKind, directives );
				}

				directives.Add( directive );
			}

			return builder;
		}

		private static DefaultRazorDirectiveFeature GetDirectiveFeature( RazorProjectEngineBuilder builder )
		{
			var directiveFeature = builder.Features.OfType<DefaultRazorDirectiveFeature>().FirstOrDefault();
			if ( directiveFeature == null )
			{
				directiveFeature = new DefaultRazorDirectiveFeature();
				builder.Features.Add( directiveFeature );
			}

			return directiveFeature;
		}

		private static IRazorTargetExtensionFeature GetTargetExtensionFeature( RazorProjectEngineBuilder builder )
		{
			var targetExtensionFeature = builder.Features.OfType<IRazorTargetExtensionFeature>().FirstOrDefault();
			if ( targetExtensionFeature == null )
			{
				targetExtensionFeature = new DefaultRazorTargetExtensionFeature();
				builder.Features.Add( targetExtensionFeature );
			}

			return targetExtensionFeature;
		}

		private static DefaultDocumentClassifierPassFeature GetDefaultDocumentClassifierPassFeature( RazorProjectEngineBuilder builder )
		{
			var configurationFeature = builder.Features.OfType<DefaultDocumentClassifierPassFeature>().FirstOrDefault();
			if ( configurationFeature == null )
			{
				configurationFeature = new DefaultDocumentClassifierPassFeature();
				builder.Features.Add( configurationFeature );
			}

			return configurationFeature;
		}
	}
}
