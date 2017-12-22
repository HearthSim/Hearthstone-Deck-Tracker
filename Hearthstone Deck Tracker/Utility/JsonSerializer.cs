using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class JsonSerializer<T> where T : class, new()
	{
		// This encryption is not intended to be actually secure in any way.
		private readonly bool _encrypt;
		private readonly string _filePath;

		public JsonSerializer(string fileName, bool encrypt) : this(Config.Instance.DataDir, fileName, encrypt)
		{
		}

		public JsonSerializer(string folder, string fileName, bool encrypt)
		{
			_encrypt = encrypt;
			_filePath = Path.Combine(folder, fileName);
		}

		public bool Save(object instance)
		{
			var json = JsonConvert.SerializeObject(instance);
			try
			{
				var bytes = Encoding.UTF8.GetBytes(json);
				if(_encrypt)
					bytes = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
				using(var fs = new FileStream(_filePath, FileMode.Create))
					fs.Write(bytes, 0, bytes.Length);
				return true;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		public void DeleteCacheFile()
		{
			if(!File.Exists(_filePath))
				return;
			try
			{
				File.Delete(_filePath);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public T Load()
		{
			if(!File.Exists(_filePath))
				return new T();
			try
			{
				var bytes = File.ReadAllBytes(_filePath);
				if(_encrypt)
					bytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine);
				return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes)) ?? new T();
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return new T();
			}
		}
	}
}
