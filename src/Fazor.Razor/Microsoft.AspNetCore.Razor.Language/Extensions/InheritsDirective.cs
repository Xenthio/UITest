// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.Extensions;

internal class InheritsDirective : IntermediateNodePassBase, IRazorDirectiveClassifierPass
{
	public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateDirective(
		SyntaxConstants.CSharp.InheritsKeyword,
		DirectiveKind.SingleLine,
		builder =>
		{
			builder.AddTypeToken( Resources.InheritsDirective_TypeToken_Name, Resources.InheritsDirective_TypeToken_Description );
			builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
			builder.Description = Resources.InheritsDirective_Description;
		} );

	public static void Register( RazorProjectEngineBuilder builder )
	{
		if ( builder == null )
		{
			throw new ArgumentNullException( nameof( builder ) );
		}

		builder.AddDirective( Directive, FileKinds.Legacy, FileKinds.Component, FileKinds.ComponentImport );
		builder.Features.Add( new InheritsDirective() );
	}

	protected override void ExecuteCore( RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode )
	{
		var @class = documentNode.FindPrimaryClass();
		if ( @class == null ) return;

		foreach ( var inherits in documentNode.FindDirectiveReferences( InheritsDirective.Directive ) )
		{
			var token = ((DirectiveIntermediateNode)inherits.Node).Tokens.FirstOrDefault();
			if ( token != null )
			{
				@class.BaseType = token.Content;
				break;
			}
		}
	}
}


