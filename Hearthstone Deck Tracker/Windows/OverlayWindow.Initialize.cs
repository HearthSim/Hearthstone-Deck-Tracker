using System.Windows;
using System.Windows.Controls.Primitives;

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class OverlayWindow
	{
		private bool _initializedCollections;
		private void InitializeCollections()
		{
			if(_initializedCollections)
				return;
			_initializedCollections = true;
			_cardMarks.AddRange(new[] {Marks0, Marks1, Marks2, Marks3, Marks4, Marks5, Marks6, Marks7, Marks8, Marks9});
			_oppBoard.AddRange(new[]
			{
				EllipseBoardOpp0,
				EllipseBoardOpp1,
				EllipseBoardOpp2,
				EllipseBoardOpp3,
				EllipseBoardOpp4,
				EllipseBoardOpp5,
				EllipseBoardOpp6
			});
			_playerBoard.AddRange(new[]
			{
				EllipseBoardPlayer0,
				EllipseBoardPlayer1,
				EllipseBoardPlayer2,
				EllipseBoardPlayer3,
				EllipseBoardPlayer4,
				EllipseBoardPlayer5,
				EllipseBoardPlayer6
			});
			_playerHand.AddRange(new[]
			{
				RectPlayerHand0,
				RectPlayerHand1,
				RectPlayerHand2,
				RectPlayerHand3,
				RectPlayerHand4,
				RectPlayerHand5,
				RectPlayerHand6,
				RectPlayerHand7,
				RectPlayerHand8,
				RectPlayerHand9
			});

			const double tWidth = 1024.0;
			const double tHeight = 768.0;
			_cardMarkPos[0] = new[] { new Point(480 / tWidth, 48 / tHeight) };
			_cardMarkPos[1] = new[] { new Point(439 / tWidth, 47 / tHeight), new Point(520 / tWidth, 48 / tHeight) };
			_cardMarkPos[2] = new[]
			{
				new Point(392 / tWidth, 33 / tHeight),
				new Point(479 / tWidth, 47 / tHeight),
				new Point(569 / tWidth, 40 / tHeight)
			};
			_cardMarkPos[3] = new[]
			{
				new Point(382 / tWidth, 21 / tHeight),
				new Point(446 / tWidth, 41 / tHeight),
				new Point(512 / tWidth, 47 / tHeight),
				new Point(580 / tWidth, 43 / tHeight)
			};
			_cardMarkPos[4] = new[]
			{
				new Point(375 / tWidth, 23 / tHeight),
				new Point(427 / tWidth, 39 / tHeight),
				new Point(479 / tWidth, 47 / tHeight),
				new Point(533 / tWidth, 46 / tHeight),
				new Point(586 / tWidth, 36 / tHeight)
			};
			_cardMarkPos[5] = new[]
			{
				new Point(371 / tWidth, 12 / tHeight),
				new Point(414 / tWidth, 30 / tHeight),
				new Point(458 / tWidth, 43 / tHeight),
				new Point(502 / tWidth, 48 / tHeight),
				new Point(546 / tWidth, 47 / tHeight),
				new Point(591 / tWidth, 39 / tHeight)
			};
			_cardMarkPos[6] = new[]
			{
				new Point(368 / tWidth, 15 / tHeight),
				new Point(405 / tWidth, 31 / tHeight),
				new Point(442 / tWidth, 41 / tHeight),
				new Point(479 / tWidth, 48 / tHeight),
				new Point(517 / tWidth, 47 / tHeight),
				new Point(555 / tWidth, 41 / tHeight),
				new Point(594 / tWidth, 31 / tHeight)
			};
			_cardMarkPos[7] = new[]
			{
				new Point(365 / tWidth, 04 / tHeight),
				new Point(397 / tWidth, 22 / tHeight),
				new Point(430 / tWidth, 35 / tHeight),
				new Point(462 / tWidth, 45 / tHeight),
				new Point(496 / tWidth, 48 / tHeight),
				new Point(530 / tWidth, 48 / tHeight),
				new Point(563 / tWidth, 43 / tHeight),
				new Point(597 / tWidth, 33 / tHeight)
			};
			_cardMarkPos[8] = new[]
			{
				new Point(363 / tWidth, 07 / tHeight),
				new Point(392 / tWidth, 23 / tHeight),
				new Point(421 / tWidth, 35 / tHeight),
				new Point(450 / tWidth, 43 / tHeight),
				new Point(479 / tWidth, 48 / tHeight),
				new Point(508 / tWidth, 47 / tHeight),
				new Point(539 / tWidth, 43 / tHeight),
				new Point(569 / tWidth, 35 / tHeight),
				new Point(599 / tWidth, 23 / tHeight)
			};
			_cardMarkPos[9] = new[]
			{
				new Point(364 / tWidth, 04 / tHeight),
				new Point(388 / tWidth, 13 / tHeight),
				new Point(414 / tWidth, 28 / tHeight),
				new Point(440 / tWidth, 38 / tHeight),
				new Point(467 / tWidth, 45 / tHeight),
				new Point(492 / tWidth, 48 / tHeight),
				new Point(520 / tWidth, 48 / tHeight),
				new Point(546 / tWidth, 44 / tHeight),
				new Point(573 / tWidth, 37 / tHeight),
				new Point(600 / tWidth, 27 / tHeight)
			};
			_movableElements.Add(BorderStackPanelPlayer, new ResizeGrip());
			_movableElements.Add(BorderStackPanelOpponent, new ResizeGrip());
			_movableElements.Add(StackPanelSecrets, new ResizeGrip());
			_movableElements.Add(LblTurnTime, new ResizeGrip());
			_movableElements.Add(IconBoardAttackPlayer, new ResizeGrip());
			_movableElements.Add(IconBoardAttackOpponent, new ResizeGrip());
			_movableElements.Add(WotogIconsPlayer, new ResizeGrip());
			_movableElements.Add(WotogIconsOpponent, new ResizeGrip());
			_movableElements.Add(LblPlayerTurnTime, new ResizeGrip());
		}
	}
}
