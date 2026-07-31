using System;
using System.IO;
using Hearthstone_Deck_Tracker.Utility.LogConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility
{
	[TestClass]
	public class LogConfigUpdaterTests
	{
		private string _tempDir;

		[TestInitialize]
		public void Setup()
		{
			_tempDir = Path.Combine(Path.GetTempPath(), "hdt_logconfig_" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_tempDir);
		}

		[TestCleanup]
		public void Cleanup()
		{
			if(!Directory.Exists(_tempDir))
				return;
			foreach(var file in Directory.GetFiles(_tempDir, "*", SearchOption.AllDirectories))
				new FileInfo(file) { IsReadOnly = false };
			Directory.Delete(_tempDir, true);
		}

		private string ConfigPath => Path.Combine(_tempDir, LogConfigConstants.LogConfigFile);

		[TestMethod]
		public void GetLogConfigPath_WithoutCommandLine_ReturnsDefaultPath()
		{
			Assert.AreEqual(LogConfigConstants.LogConfigPath, LogConfigConstants.GetLogConfigPath(null));
		}

		[TestMethod]
		public void GetLogConfigPath_WithoutProductUid_ReturnsDefaultPath()
		{
			Assert.AreEqual(LogConfigConstants.LogConfigPath, LogConfigConstants.GetLogConfigPath(@"""C:\Hearthstone\Hearthstone.exe"" -launch"));
		}

		[TestMethod]
		public void GetLogConfigPath_WithRetailProductUid_ReturnsDefaultPath()
		{
			Assert.AreEqual(LogConfigConstants.LogConfigPath, LogConfigConstants.GetLogConfigPath(@"""C:\Hearthstone\Hearthstone.exe"" -uid hs_beta"));
		}

		[TestMethod]
		public void GetLogConfigPath_WithoutProductUidValue_ReturnsDefaultPath()
		{
			Assert.AreEqual(LogConfigConstants.LogConfigPath, LogConfigConstants.GetLogConfigPath(@"""C:\Hearthstone\Hearthstone.exe"" -uid"));
		}

		[TestMethod]
		public void GetLogConfigPath_WithEventProductUid_ReturnsProductSubdirectoryPath()
		{
			var expected = Path.Combine(LogConfigConstants.HearthstoneAppData, "hs_custom", LogConfigConstants.LogConfigFile);
			Assert.AreEqual(expected, LogConfigConstants.GetLogConfigPath(@"""C:\Hearthstone\Hearthstone.exe"" -uid hs_custom -launch"));
		}

		[TestMethod]
		public void CheckLogConfig_WithoutExistingFile_CreatesCompleteConfig()
		{
			Assert.IsTrue(LogConfigUpdater.CheckLogConfig(ConfigPath));
			Assert.IsTrue(File.Exists(ConfigPath));
			Assert.IsFalse(LogConfigUpdater.CheckLogConfig(ConfigPath));
		}

		[TestMethod]
		public void CheckLogConfig_WithoutExistingDirectory_CreatesDirectoryAndConfig()
		{
			var path = Path.Combine(_tempDir, "hs_custom", LogConfigConstants.LogConfigFile);

			Assert.IsTrue(LogConfigUpdater.CheckLogConfig(path));

			Assert.IsTrue(File.Exists(path));
		}

		[TestMethod]
		public void CheckLogConfig_WithIncompleteConfig_UpdatesConfig()
		{
			File.WriteAllText(ConfigPath, "[Power]\nLogLevel=0\nFilePrinting=false\n");

			Assert.IsTrue(LogConfigUpdater.CheckLogConfig(ConfigPath));

			var content = File.ReadAllText(ConfigPath);
			StringAssert.Contains(content, "LogLevel=1");
			StringAssert.Contains(content, "FilePrinting=True");
			StringAssert.Contains(content, "Verbose=True");
		}

		[TestMethod]
		public void CheckLogConfig_WithCompleteConfig_LeavesConfigUnchanged()
		{
			LogConfigUpdater.CheckLogConfig(ConfigPath);
			var content = File.ReadAllText(ConfigPath);

			Assert.IsFalse(LogConfigUpdater.CheckLogConfig(ConfigPath));

			Assert.AreEqual(content, File.ReadAllText(ConfigPath));
		}

		[TestMethod]
		public void CheckLogConfig_WithReadOnlyConfig_UpdatesConfig()
		{
			File.WriteAllText(ConfigPath, "[Power]\nLogLevel=0\n");
			new FileInfo(ConfigPath) { IsReadOnly = true };

			Assert.IsTrue(LogConfigUpdater.CheckLogConfig(ConfigPath));

			Assert.IsFalse(new FileInfo(ConfigPath).IsReadOnly);
			StringAssert.Contains(File.ReadAllText(ConfigPath), "LogLevel=1");
		}
	}
}
