using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Utility.Updating;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility.Updating
{
	[TestClass]
	public class UpdaterMirrorsTests
	{

		[TestMethod]
		public void MirrorUrls_HasUrlForAllMirrors()
		{
			foreach(var value in Enum.GetValues(typeof(UpdateMirror)).Cast<UpdateMirror>())
			{
				try
				{
					_ = Updater.MirrorUrls[value];
				}
				catch(Exception e)
				{
					Assert.Fail(e.Message);
				}
			}
		}

		[TestMethod]
		public void GetCurrentMirror_ReturnsExpectedValue()
		{
			Config.Instance.SquirrelRemote = (int)UpdateMirror.Github;
			var (mirror, url) = Updater.GetCurrentMirror();
			Assert.AreEqual(mirror, UpdateMirror.Github);

			Config.Instance.SquirrelRemote = (int)UpdateMirror.AwsHongKong;
			(mirror, url) = Updater.GetCurrentMirror();
			Assert.AreEqual(mirror, UpdateMirror.AwsHongKong);

			Config.Instance.SquirrelRemote = 12345;
			(mirror, url) = Updater.GetCurrentMirror();
			Assert.AreEqual(mirror, UpdateMirror.Github); // invalid value defaults to github
		}

		private string _id;

		[TestInitialize]
		public void Initialize()
		{
			_id = Config.Instance.Id;
		}

		[TestCleanup]
		public void Cleanup()
		{
			Config.Instance.Id = _id;
			Config.Instance.MigrateToVelopack = false;
			Config.Instance.SquirrelRemote = (int)UpdateMirror.Github;
		}

		private static RemoteData.Config RemoteConfig(double? rollout) => new RemoteData.Config
		{
			Velopack = rollout.HasValue ? new RemoteData.VelopackData { Rollout = rollout.Value } : null
		};

		[TestMethod]
		public void UpdateVelopackRollout_SwitchesFeedWithRollout()
		{
			Config.Instance.Id = Guid.NewGuid().ToString();

			Updater.UpdateVelopackRollout(RemoteConfig(1));
			Assert.IsTrue(Config.Instance.MigrateToVelopack);
			Assert.AreEqual("https://github.com/HearthSim/HDT-Velopack-Releases", Updater.MirrorUrls[UpdateMirror.Github]);
			Assert.AreEqual("https://hdt-downloads-hongkong.s3.ap-east-1.amazonaws.com/velopack", Updater.MirrorUrls[UpdateMirror.AwsHongKong]);

			Updater.UpdateVelopackRollout(null);
			Assert.IsTrue(Config.Instance.MigrateToVelopack);

			Updater.UpdateVelopackRollout(RemoteConfig(0));
			Assert.IsFalse(Config.Instance.MigrateToVelopack);
			Assert.AreEqual("https://github.com/HearthSim/HDT-Releases", Updater.MirrorUrls[UpdateMirror.Github]);
			Assert.AreEqual("https://hdt-downloads-hongkong.s3.ap-east-1.amazonaws.com", Updater.MirrorUrls[UpdateMirror.AwsHongKong]);

			Updater.UpdateVelopackRollout(RemoteConfig(1));
			Updater.UpdateVelopackRollout(RemoteConfig(null));
			Assert.IsFalse(Config.Instance.MigrateToVelopack);
		}

		[TestMethod]
		public void IsInVelopackRollout_KeepsInstallsAsRolloutRampsUp()
		{
			var random = new Random(42);
			var ids = Enumerable.Range(0, 100_000).Select(_ =>
			{
				var bytes = new byte[16];
				random.NextBytes(bytes);
				return new Guid(bytes).ToString();
			}).ToList();

			var previous = new HashSet<string>();
			foreach(var rollout in new[] { 0, 0.0001, 0.001, 0.01, 0.1, 0.5, 1 })
			{
				var included = new HashSet<string>(ids.Where(id => Updater.IsInVelopackRollout(id, rollout)));
				Assert.IsTrue(previous.IsSubsetOf(included), $"installs left the rollout at {rollout}");
				Assert.AreEqual(rollout * ids.Count, included.Count, Math.Max(10, rollout * ids.Count * 0.1), $"unexpected share at {rollout}");
				previous = included;
			}
		}

		[TestMethod]
		public void IsInVelopackRollout_ExcludesInvalidIds()
		{
			Assert.IsFalse(Updater.IsInVelopackRollout(Guid.Empty.ToString(), 1));
			Assert.IsFalse(Updater.IsInVelopackRollout("not a guid", 1));
		}
	}

}
