using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Hearthstone_Deck_Tracker.Properties;

namespace Hearthstone_Deck_Tracker.Utility
{
	/// <summary>
	/// Serves the localized strings from resources embedded in the (signed) main assembly rather than from
	/// unsigned satellite assemblies.
	/// </summary>
	public class EmbeddedResourceManager : ResourceManager
	{
		private readonly Dictionary<string, ResourceSet?> _resourceSets = new();

		private EmbeddedResourceManager(string baseName, Assembly assembly) : base(baseName, assembly)
		{
		}

		// must run before anything (including WPFLocalizeExtension) reads Strings.ResourceManager
		public static void Install()
		{
			var field = typeof(Strings).GetField("resourceMan", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new MissingFieldException(typeof(Strings).FullName, "resourceMan");
			field.SetValue(null, new EmbeddedResourceManager(typeof(Strings).FullName, typeof(Strings).Assembly));
		}

		protected override ResourceSet? InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
		{
			for(var current = culture; !Equals(current, CultureInfo.InvariantCulture); current = current.Parent)
			{
				var resourceSet = GetEmbeddedResourceSet(current, createIfNotExists);
				if(resourceSet != null)
					return resourceSet;
				if(!tryParents)
					return null;
			}
			return base.InternalGetResourceSet(CultureInfo.InvariantCulture, createIfNotExists, tryParents);
		}

		private ResourceSet? GetEmbeddedResourceSet(CultureInfo culture, bool createIfNotExists)
		{
			lock(_resourceSets)
			{
				if(_resourceSets.TryGetValue(culture.Name, out var cached) || !createIfNotExists)
					return cached;
				var resourceName = $"{BaseName}.{culture.Name}.resources";
				var resourceSet = MainAssembly.GetManifestResourceInfo(resourceName) != null ? new LazyResourceSet(MainAssembly, resourceName) : null;
				_resourceSets[culture.Name] = resourceSet;
				return resourceSet;
			}
		}

		// WPFLocalizeExtension requests the set of every culture to discover the available ones, so only read
		// the strings once something is actually looked up
		private class LazyResourceSet : ResourceSet, IEnumerable
		{
			private readonly Assembly _assembly;
			private readonly string _resourceName;
			private volatile bool _loaded;

			public LazyResourceSet(Assembly assembly, string resourceName)
			{
				_assembly = assembly;
				_resourceName = resourceName;
			}

			private void EnsureLoaded()
			{
				if(_loaded)
					return;
				lock(this)
				{
					if(_loaded)
						return;
					Reader = new ResourceReader(_assembly.GetManifestResourceStream(_resourceName));
					ReadResources();
					_loaded = true;
				}
			}

			public override string? GetString(string name)
			{
				EnsureLoaded();
				return base.GetString(name);
			}

			public override string? GetString(string name, bool ignoreCase)
			{
				EnsureLoaded();
				return base.GetString(name, ignoreCase);
			}

			public override object? GetObject(string name)
			{
				EnsureLoaded();
				return base.GetObject(name);
			}

			public override object? GetObject(string name, bool ignoreCase)
			{
				EnsureLoaded();
				return base.GetObject(name, ignoreCase);
			}

			public override IDictionaryEnumerator GetEnumerator()
			{
				EnsureLoaded();
				return base.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		public override void ReleaseAllResources()
		{
			lock(_resourceSets)
			{
				foreach(var resourceSet in _resourceSets.Values)
					resourceSet?.Dispose();
				_resourceSets.Clear();
			}
			base.ReleaseAllResources();
		}
	}
}
