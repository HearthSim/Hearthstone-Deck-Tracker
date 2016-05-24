using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace HDTTests.LogReader
{
    [TestClass]
    public class ArenaHandlerTest
    {
        private IGame _game;
        private IHsGameState _gameState;
        private IGameHandler _gameHandler;
        private ArenaHandler _arenaHandler;

        [TestInitialize]
        public void SetUp()
        {
            _game = MockRepository.GenerateMock<IGame>();
            _gameHandler = MockRepository.GenerateMock<IGameHandler>();
            _gameState = MockRepository.GenerateMock<IHsGameState>();
            _gameState.Stub(x => x.GameHandler).Return(_gameHandler);
            _arenaHandler = new ArenaHandler();
        }

        [TestMethod]
        public void Handle_DetectArenaHero()
        {
            var logLine = "[Arena] DraftManager.OnChoicesAndContents - Draft Deck ID: 420647254, Hero Card = HERO_08";
            //_arenaHandler.Handle(logLine, _gameState, _game);
            //_game.AssertWasCalled(x => x.NewArenaDeck("HERO_08"));
        }

        [TestMethod]
        public void Handle_DetectArenaCard()
        {
            var logLine = "[Arena] DraftManager.OnChoicesAndContents - Draft deck contains card BRM_003";
            //_arenaHandler.Handle(logLine, _gameState, _game);
            //_game.AssertWasCalled(x => x.NewArenaCard("BRM_003"));
        }
    }
}