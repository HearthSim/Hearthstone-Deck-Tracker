using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using HSReplay.OAuth;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HSReplay.Test
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void KeyGen_AccountStatus_Upload()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			var client = new HsReplayClient("89c8bbc1-474a-4b1b-91b5-2a116d19df7a", "HSReplay-API-Test/1.0", true);

			var token = client.CreateUploadToken().Result;
			Assert.IsFalse(string.IsNullOrEmpty(token), "string.IsNullOrEmpty(key)");

			var account = client.GetAccountStatus(token).Result;
			Assert.AreEqual(token, account.Key, "Key matches sent token");
			Assert.IsTrue(account.TestData, "account.TestData");
			Assert.IsNull(account.User);

			var claimUrl = client.GetClaimAccountUrl(token).Result;
			Assert.IsNotNull(claimUrl);
			Assert.IsTrue(claimUrl.StartsWith("https://hsreplay.net/"));

			var metaData = new UploadMetaData()
			{
				TestData = true,
				HearthstoneBuild = 1,
				MatchStart = DateTime.Now.ToString("o")
			};
			var uploadEvent = client.CreateUploadRequest(metaData, token).Result;
			Assert.IsFalse(string.IsNullOrEmpty(uploadEvent.PutUrl));
			Assert.IsFalse(string.IsNullOrEmpty(uploadEvent.ShortId));
			Assert.IsFalse(string.IsNullOrEmpty(uploadEvent.ReplayUrl));

			var packUpload = client.UploadPack(
				new PackData
				{
					AccountHi = 1,
					AccountLo = 1,
					BoosterType = 1,
					Date = DateTime.Now.ToString("o"),
					Cards =
						new[]
						{
							new CardData {CardId = "GAME_005", Premium = true},
							new CardData {CardId = "GAME_005", Premium = true},
							new CardData {CardId = "GAME_005", Premium = true},
							new CardData {CardId = "GAME_005", Premium = true},
							new CardData {CardId = "GAME_005", Premium = true}
						}
				}, token).Result;
			string[] log;
			using(var sr = new StreamReader("TestData/Power.log"))
				log = sr.ReadToEnd().Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			client.UploadLog(uploadEvent, log).Wait();
		}

		[TestMethod]
		public void ClientConfig_InvalidUrl_Error()
		{
			var webException = false;
			var config = new ClientConfig() { TokensUrl  = "https://hsreplay.net/api/v0/tokens/" };
			var client = new HsReplayClient("89c8bbc1-474a-4b1b-91b5-2a116d19df7a", "HSReplay-API-Test/1.0", true, config);
			try
			{
				var token = client.CreateUploadToken().Result;
			}
			catch(AggregateException aggregateException)
			{
				Assert.IsInstanceOfType(aggregateException.InnerExceptions[0], typeof(WebException));
				webException = true;
			}
			Assert.IsTrue(webException);
		}

		[TestMethod]
		public void TestDeckWinrate()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			var client = new HsReplayClient("89c8bbc1-474a-4b1b-91b5-2a116d19df7a", "HSReplay-API-Test/1.0", true);

			const string wildDeckId = "AslFFZWw1KEskJD94V00fh";

			var token = client.CreateUploadToken().Result;
			var dataStandard = client.GetDeckWinrates(wildDeckId, false, token).Result;
			Assert.IsNull(dataStandard);

			var dataWild = client.GetDeckWinrates(wildDeckId, true, token).Result;
			Assert.IsNotNull(dataWild);
		}

		[TestMethod]
		public void OAuthTest()
		{
			var oauth = new OAuthClient("sYJm96GE3pgVtK9gjJ9Ghr456p3cCUUv3qPRsaXL", "HSReplay-API-Test/1.0");
			var url = oauth.GetAuthenticationUrl(new []{Scope.ReadGames}, new[] {17784, 17785, 17786});
			var callbackTask = oauth.ReceiveAuthenticationCallback("https://hsreplay.net", "https://hsreplay.net");
			Process.Start(url);
			var data = callbackTask.Result;
			var token = oauth.GetToken(data.Code, data.RedirectUrl).Result;
			Console.WriteLine(token);
			var games = oauth.GetGames("Epix#2966");
			Console.WriteLine(games.Result);
			var foo = oauth.RefreshToken().Result;
			Console.WriteLine(foo);
		}
	}
}
