using System;
using System.Collections.Generic;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Importing;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for BattlegroundsMinion.xaml
	/// </summary>
	public partial class BattlegroundsMinion
	{
		public Visibility PoisonousVisibility { get; set; }

		public Visibility DivineShieldVisibility { get; set; }

		public Visibility TauntVisibility { get; set; }

		public Visibility PremiumTauntVisibility { get; set; }

		public Visibility DeathrattleVisibility { get; set; }

		public Visibility LegendaryBorderVisibility { get; set; }

		public Visibility PremiumLegendaryBorderVisibility { get; set; }

		public Visibility PremiumBorderVisibility { get; set; }

		public Visibility BorderVisibility { get; set; }

		public string CardImagePath { get; set; }

		public string AttackDisplay { get; set; }

		public string HealthDisplay { get; set; }

		private Entity _entity;

		public BattlegroundsMinion(Entity entity)
		{
			_entity = entity;
			SetEffectVisibilites();
			SetDisplayValues();
			CardImagePath = CardImageImporter.StoragePathFor(entity.CardId);
			InitializeComponent();
		}

		private void SetDisplayValues()
		{
			AttackDisplay = _entity.Attack.ToString();
			HealthDisplay = _entity.Health.ToString();
		}

		private void SetEffectVisibilites()
		{
			PoisonousVisibility = _entity.HasTag(GameTag.POISONOUS) ? Visibility.Visible : Visibility.Hidden;
			DivineShieldVisibility = _entity.HasTag(GameTag.DIVINE_SHIELD) ? Visibility.Visible : Visibility.Hidden;
			DeathrattleVisibility = _entity.HasTag(GameTag.DEATHRATTLE) ? Visibility.Visible : Visibility.Hidden;

			if(_entity.HasTag(GameTag.PREMIUM))
			{
				PremiumLegendaryBorderVisibility = _entity.GetTag(GameTag.RARITY) == (int)Rarity.LEGENDARY ? Visibility.Visible : Visibility.Hidden;
				PremiumBorderVisibility = Visibility.Visible;
				BorderVisibility = Visibility.Hidden;
				LegendaryBorderVisibility = Visibility.Hidden;
				TauntVisibility = Visibility.Hidden;
				PremiumTauntVisibility = _entity.HasTag(GameTag.TAUNT) ? Visibility.Visible : Visibility.Hidden;
			}
			else
			{
				PremiumLegendaryBorderVisibility = Visibility.Hidden;
				PremiumBorderVisibility = Visibility.Hidden;
				BorderVisibility = Visibility.Visible;
				LegendaryBorderVisibility = _entity.GetTag(GameTag.RARITY) == (int)Rarity.LEGENDARY ? Visibility.Visible : Visibility.Hidden;
				TauntVisibility = _entity.HasTag(GameTag.TAUNT) ? Visibility.Visible : Visibility.Hidden;
				PremiumTauntVisibility = Visibility.Hidden;
			}



		}

		private void SetVisibility(Visibility setFor, bool makeVisibile) => setFor = makeVisibile ? Visibility.Visible : Visibility.Hidden;
	}
}
