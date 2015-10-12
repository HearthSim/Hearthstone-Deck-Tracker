using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace HDTTests.LogReader
{
    [TestClass]
    public class PowerHandlerTest
    {
        private IGame _game;
        private IHsGameState _gameState;
        private IGameHandler _gameHandler;
        private PowerHandler _powerHandler;

        [TestInitialize]
        public void SetUp()
        {
            _game = MockRepository.GenerateMock<IGame>();
            _gameHandler = MockRepository.GenerateMock<IGameHandler>();
            _gameState = MockRepository.GenerateMock<IHsGameState>();
            _gameState.Stub(x => x.GameHandler).Return(_gameHandler);
            _powerHandler = new PowerHandler();
        }

        [TestMethod]
        public void Handle_StartSpectating()
        {
            var logLine = "[Power] ================== Begin Spectating 1st player ==================";
            _game.Stub(x => x.IsInMenu).Return(true);
            _powerHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Spectator));
            _gameState.AssertWasCalled(x => x.FoundSpectatorStart = false);
        }

        [TestMethod]
        public void Handle_EndSpectating()
        {
            var logLine = "[Power] ================== End Spectator Mode ==================";
            _game.Stub(x => x.IsInMenu).Return(true);
            _powerHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Spectator));
            _gameHandler.AssertWasCalled(x => x.HandleGameEnd());
        }

    }
}