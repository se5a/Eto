using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

namespace Eto.Designer.Builders
{
	public class CSharpInterfaceBuilder : CodeInterfaceBuilder
	{
		protected override CodeDomProvider CreateCodeProvider()
		{
			var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
			return new CSharpCodeProvider();
		}
	}
}

