using System;
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
    public class BobHandlerTest
    {
        private IGame _game;
        private IHsGameState _gameState;
        private IGameHandler _gameHandler;
        private BobHandler _bobHandler;

        [TestInitialize]
        public void SetUp()
        {
            _game = MockRepository.GenerateMock<IGame>();
            _gameHandler = MockRepository.GenerateMock<IGameHandler>();
            _gameState = MockRepository.GenerateMock<IHsGameState>();
            _gameState.Stub(x => x.GameHandler).Return(_gameHandler);
            _bobHandler = new BobHandler();
        }

        [TestMethod]
        public void Handle_RegisterFriendChallenge()
        {
            var logLine = "[Bob] ---RegisterFriendChallenge---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.HandleInMenu());
        }

        [TestMethod]
        public void Handle_RegisterScreenPractice()
        {
            var logLine = "[Bob] ---RegisterScreenPractice---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Practice));
        }

        [TestMethod]
        public void Handle_RegisterScreenTourneys()
        {
            var logLine = "[Bob] ---RegisterScreenTourneys---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Casual));
        }

        [TestMethod]
        public void Handle_RegisterScreenFriendly()
        {
            var logLine = "[Bob] ---RegisterScreenFriendly---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Friendly));
        }

        [TestMethod]
        public void Handle_RegisterScreenCollectionManager()
        {
            var logLine = "[Bob] ---RegisterScreenCollectionManager---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.ResetConstructedImporting());
        }

        [TestMethod]
        public void Handle_RegisterScreenForge()
        {
            var logLine = "[Bob] ---RegisterScreenForge---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameHandler.AssertWasCalled(x => x.SetGameMode(GameMode.Arena));
            _game.AssertWasCalled(x => x.ResetArenaCards());
        }

        [TestMethod]
        public void Handle_RegisterScreenBox()
        {
            var logLine = "[Bob] ---RegisterScreenBox---";
            _game.Stub(x => x.CurrentGameMode).Return(GameMode.Spectator);
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameState.AssertWasCalled(x => x.GameEnd());
        }

        [TestMethod]
        public void Handle_RegisterProfileNotices()
        {
            var logLine = "[Bob] ---RegisterProfileNotices---";
            _bobHandler.Handle(logLine, _gameState, _game);
            _gameState.AssertWasCalled(x => x.GameLoaded = true);
        }
    }
}