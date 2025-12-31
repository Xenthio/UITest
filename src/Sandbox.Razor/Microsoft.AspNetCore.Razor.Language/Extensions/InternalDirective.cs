// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.Extensions;

internal class InternalDirective : IntermediateNodePassBase, IRazorDirectiveClassifierPass
{
	public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateDirective(
		"internal",
		DirectiveKind.SingleLine,
		builder =>
		{
			builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
		} );

	public static void Register( RazorProjectEngineBuilder builder )
	{
		if ( builder == null )
			throw new ArgumentNullException( nameof( builder ) );

		builder.AddDirective( Directive, FileKinds.Legacy, FileKinds.Component, FileKinds.ComponentImport );
		builder.Features.Add( new InternalDirective() );
	}

	protected override void ExecuteCore( RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode )
	{
		var @class = documentNode.FindPrimaryClass();
		if ( @class == null ) return;
		if ( !documentNode.FindDirectiveReferences( InternalDirective.Directive ).Any() ) return;

		@class.Visibility = "internal";
	}

}

