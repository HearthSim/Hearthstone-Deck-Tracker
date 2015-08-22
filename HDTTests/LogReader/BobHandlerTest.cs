using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace HDTTests.LogReader
{
    [TestClass]
    public class BobHandlerTest
    {
        private GameV2 _game;
        private IHsGameState _gameState;
        private IGameHandler _gameHandler;
        private BobHandler _bobHandler;

        [TestInitialize]
        public void SetUp()
        {
            _game = new GameV2();
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
    }
}