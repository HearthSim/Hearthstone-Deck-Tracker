using System.ComponentModel;


// This is required to use `init` properties while we are not yet on .NET 5.
// Remove this file after upgrading.
namespace System.Runtime.CompilerServices
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class IsExternalInit { }
}
