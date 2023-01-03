using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class FranchiseProperties
	{
		private readonly Dictionary<HearthstoneSettings, bool>? _hearthstoneSettings;
		private readonly Dictionary<BattlegroundsSettings, bool>? _battlegroundsSettings;
		private readonly Dictionary<MercenariesSettings, bool>? _mercenariesSettings;

		public FranchiseProperties(Dictionary<HearthstoneExtraData, object> extraData)
		{
			HearthstoneExtraData = extraData;
			_hearthstoneSettings = new Dictionary<HearthstoneSettings, bool> {
				{ HearthstoneSettings.HideDecks, Config.Instance.HideDecksInOverlay },
				{ HearthstoneSettings.HideTimers, Config.Instance.HideTimers },
			};
		}

		public FranchiseProperties(Dictionary<BattlegroundsExtraData, object> extraData)
		{
			BattlegroundsExtraData = extraData;
			_battlegroundsSettings = new Dictionary<BattlegroundsSettings, bool> {
				{ BattlegroundsSettings.Tiers, Config.Instance.ShowBattlegroundsTiers },
				{ BattlegroundsSettings.TurnCounter, Config.Instance.ShowBattlegroundsTurnCounter },
				{ BattlegroundsSettings.BobsBuddyCombatSimulations, Config.Instance.RunBobsBuddy },
				{ BattlegroundsSettings.BobsBuddyResultsDuringCombat, Config.Instance.ShowBobsBuddyDuringCombat },
				{ BattlegroundsSettings.BobsBuddyResultsDuringShopping, Config.Instance.ShowBobsBuddyDuringShopping },
				{ BattlegroundsSettings.BobsBuddyAlwaysShowAverageDamage, Config.Instance.AlwaysShowAverageDamage },
				{ BattlegroundsSettings.SessionRecap, Config.Instance.ShowSessionRecap },
				{ BattlegroundsSettings.SessionRecapBetweenGames, Config.Instance.ShowSessionRecapBetweenGames },
				{ BattlegroundsSettings.MinionsBanned, Config.Instance.ShowSessionRecapMinionsBanned },
				{ BattlegroundsSettings.StartAndCurrentMMR, Config.Instance.ShowSessionRecapStartCurrentMMR },
				{ BattlegroundsSettings.Latest10Game, Config.Instance.ShowSessionRecapLatestGames },
				{ BattlegroundsSettings.Tier7Overlay, Config.Instance.EnableBattlegroundsTier7Overlay },
				{ BattlegroundsSettings.Tier7PrelobbyOverlay, Config.Instance.ShowBattlegroundsTier7PreLobby },
				{ BattlegroundsSettings.Tier7HeroOverlay, Config.Instance.ShowBattlegroundsHeroPicking },
				{ BattlegroundsSettings.Tier7QuestOverlay, Config.Instance.ShowBattlegroundsQuestPicking },
				{ BattlegroundsSettings.Tier7QuestOverlayCompositions, Config.Instance.ShowBattlegroundsQuestPickingComps },
			};
		}

		public FranchiseProperties(Dictionary<MercenariesExtraData, object> extraData)
		{
			MercenariesExtraData = extraData;
			_mercenariesSettings = new Dictionary<MercenariesSettings, bool> {
				{ MercenariesSettings.OpponentMercAbilitiesOnHover, Config.Instance.ShowMercsOpponentHover },
				{ MercenariesSettings.PlayerMercAbilitiesOnHover, Config.Instance.ShowMercsPlayerHover },
				{ MercenariesSettings.AbilityIconsAboveOpponentMercs, Config.Instance.ShowMercsOpponentAbilityIcons },
				{ MercenariesSettings.AbilityIconsBelowPlayerMercs, Config.Instance.ShowMercsPlayerAbilityIcons },
				{ MercenariesSettings.TasksPanel, Config.Instance.ShowMercsTasks },
			};
		}
		
		public Dictionary<HearthstoneExtraData, object>? HearthstoneExtraData { get; }
		public Dictionary<BattlegroundsExtraData, object>? BattlegroundsExtraData { get; }
		public Dictionary<MercenariesExtraData, object>? MercenariesExtraData { get; }

		public HearthstoneSettings[]? HearthstoneSettingsEnabled
		{
			get => _hearthstoneSettings?.Where(x => x.Value)
				.Select(x => x.Key)
				.ToArray();
		}

		public HearthstoneSettings[]? HearthstoneSettingsDisabled
		{
			get => _hearthstoneSettings?.Where(x => !x.Value)
				.Select(x => x.Key)
				.ToArray();
		}

		public BattlegroundsSettings[]? BattlegroundsSettingsEnabled
		{
			get => _battlegroundsSettings?.Where(x => x.Value)
				.Select(x => x.Key)
				.ToArray();
		}

		public BattlegroundsSettings[]? BattlegroundsSettingsDisabled
		{
			get => _battlegroundsSettings?.Where(x => !x.Value)
				.Select(x => x.Key)
				.ToArray();
		}

		public MercenariesSettings[]? MercenariesSettingsEnabled
		{
			get => _mercenariesSettings?.Where(x => x.Value)
				.Select(x => x.Key)
				.ToArray();
		}

		public MercenariesSettings[]? MercenariesSettingsDisabled
		{
			get => _mercenariesSettings?.Where(x => !x.Value)
				.Select(x => x.Key)
				.ToArray();
		}
	}
}
