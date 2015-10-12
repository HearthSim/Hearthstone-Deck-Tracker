using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace HDTTests.LogReader
{
    [TestClass]
    public class AssetHandlerTest
    {
        private IGame _game;
        private IHsGameState _gameState;
        private IGameHandler _gameHandler;
        private AssetHandler _assetHandler;

        [TestInitialize]
        public void SetUp()
        {
            _game = MockRepository.GenerateMock<IGame>();
            _gameHandler = MockRepository.GenerateMock<IGameHandler>();
            _gameState = MockRepository.GenerateMock<IHsGameState>();
            _gameState.Stub(x => x.GameHandler).Return(_gameHandler);
            _assetHandler = new AssetHandler();
        }

        [TestMethod]
        public void Handle_DetectRank()
        {
            var logLine = "[Asset] CachedAsset.UnloadAssetObject() - unloading name=Medal_Ranked_11 family=Texture persistent=False";
            _assetHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetRank(11));
        }

        [TestMethod]
        public void Handle_SetGameModeRanked()
        {
            var logLine = "[Asset] CachedAsset.UnloadAssetObject() - unloading name=rank_window_expand family=Sound persistent=True";
            _assetHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Ranked));
            _gameState.AssertWasCalled(x => x.FoundRanked = true);
        }

        [TestMethod]
        public void Handle_SetGameModeBrawl()
        {
            var logLine = "[Asset] CachedAsset.UnloadAssetObject() - unloading name=Tavern_Brawl family=Sound persistent=True";
            _assetHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Brawl));
        }
    }
}