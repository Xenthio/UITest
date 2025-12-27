// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Language
{
	public abstract class RazorConfiguration : IEquatable<RazorConfiguration>
	{
		public static readonly RazorConfiguration Default = new DefaultRazorConfiguration( RazorLanguageVersion.Latest, "unnamed" );

		public abstract string ConfigurationName { get; }

		public abstract RazorLanguageVersion LanguageVersion { get; }

		public override bool Equals( object obj )
		{
			return base.Equals( obj as RazorConfiguration );
		}

		public virtual bool Equals( RazorConfiguration other )
		{
			if ( object.ReferenceEquals( other, null ) )
			{
				return false;
			}

			if ( LanguageVersion != other.LanguageVersion )
			{
				return false;
			}

			if ( ConfigurationName != other.ConfigurationName )
			{
				return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			var hash = new HashCodeCombiner();
			hash.Add( LanguageVersion );
			hash.Add( ConfigurationName );

			return hash;
		}

		private class DefaultRazorConfiguration : RazorConfiguration
		{
			public DefaultRazorConfiguration(
				RazorLanguageVersion languageVersion,
				string configurationName )
			{
				LanguageVersion = languageVersion;
				ConfigurationName = configurationName;
			}

			public override string ConfigurationName { get; }

			public override RazorLanguageVersion LanguageVersion { get; }
		}
	}
}
