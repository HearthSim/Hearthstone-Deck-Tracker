using System.Collections.Generic;
using System.Linq;
using HearthMirror.Objects;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	abstract public class CollectionBase
	{
		public CollectionBase(ulong accountHi, ulong accountLo, BattleTag battleTag)
		{
			AccountHi = accountHi;
			AccountLo = accountLo;
			BattleTag = $"{battleTag.Name}#{battleTag.Number}";
		}

		[JsonIgnore]
		public ulong AccountHi { get; }

		[JsonIgnore]
		public ulong AccountLo { get; }

		[JsonIgnore]
		public string BattleTag { get; }

		public override int GetHashCode() => JsonConvert.SerializeObject(this).GetHashCode();
	}

	public class Collection : CollectionBase
	{
		public Collection(ulong accountHi, ulong accountLo, BattleTag battleTag, HearthMirror.Objects.Collection collection) : base(accountHi, accountLo, battleTag)
		{
			Cards = new SortedDictionary<int, int[]>(
				collection.Cards.Select(x => new {Key=GetDbfId(x.Id), Card=x}).GroupBy(x => x.Key)
					.ToDictionary(x => x.Key,
						x => new[]
						{
							x.FirstOrDefault(c => c.Card.PremiumType == 0)?.Card.Count ?? 0,
							x.FirstOrDefault(c => c.Card.PremiumType == 1)?.Card.Count ?? 0,
							x.FirstOrDefault(c => c.Card.PremiumType == 2)?.Card.Count ?? 0,
							x.FirstOrDefault(c => c.Card.PremiumType == 3)?.Card.Count ?? 0,

							x.FirstOrDefault(c => c.Card.PremiumType == 0)?.Card.TrialCount ?? 0,
							x.FirstOrDefault(c => c.Card.PremiumType == 1)?.Card.TrialCount ?? 0,
							x.FirstOrDefault(c => c.Card.PremiumType == 2)?.Card.TrialCount ?? 0,
							x.FirstOrDefault(c => c.Card.PremiumType == 3)?.Card.TrialCount ?? 0,
						}));
			FavoriteHeroes = new SortedDictionary<int, int>(collection.FavoriteHeroes.ToDictionary(x => x.Key, x => GetDbfId(x.Value.Id)));
			CardBacks = collection.CardBacks.OrderBy(x => x).ToList();
			FavoriteCardBack = collection.FavoriteCardBack;
			Dust = collection.Dust;

			PlayerRecords = new SortedDictionary<int, SortedDictionary<int, int[]>> (
				collection.PlayerRecords.GroupBy(x => x.Type)
					.ToDictionary(x =>
						x.Key,
						x => new SortedDictionary<int, int[]>(
							x.GroupBy(y => y.Data)
								.ToDictionary(y =>
									y.Key,
									y => y.Select(pr => new[] { pr.Wins, pr.Losses, pr.Ties }).FirstOrDefault()
								)
						)
					)
			);

			Pets = new SortedDictionary<int, Pet>(
				collection.Pets.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => new Pet(x.First()))
			);
			PetVariants = new SortedDictionary<int, PetVariant>(
				collection.PetVariants.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => new PetVariant(x.First()))
			);
		}

		public int Size()
		{
			return Cards.Sum(x => x.Value.Sum());
		}

		private static int GetDbfId(string cardId) => HearthDb.Cards.CardIdToDbfId.TryGetValue(cardId, out var dbfId) ? dbfId : 0;

		[JsonProperty("collection")]
		public SortedDictionary<int, int[]> Cards { get; }

		[JsonProperty("favorite_heroes")]
		public SortedDictionary<int, int> FavoriteHeroes { get; }

		[JsonProperty("cardbacks")]
		public List<int> CardBacks { get; }

		[JsonProperty("favorite_cardback")]
		public int FavoriteCardBack { get; }

		[JsonProperty("dust")]
		public int Dust { get; }

		[JsonProperty("player_records")]
		public SortedDictionary<int, SortedDictionary<int, int[]>> PlayerRecords { get; }

		/// <summary>
		/// Owned pets by PET dbf id.
		/// </summary>
		[JsonProperty("pets")]
		public SortedDictionary<int, Pet> Pets { get; }

		/// <summary>
		/// Owned pet variants (the looks of the pets) by PET_VARIANT dbf id.
		/// </summary>
		[JsonProperty("pet_variants")]
		public SortedDictionary<int, PetVariant> PetVariants { get; }

		public class Pet
		{
			public Pet(HearthMirror.Objects.Pet pet)
			{
				Level = pet.Level;
				Xp = pet.Xp;
				IsHsFavorite = pet.IsHsFavorite;
				IsBgFavorite = pet.IsBgFavorite;
			}

			[JsonProperty("level")]
			public int Level { get; }

			[JsonProperty("xp")]
			public int Xp { get; }

			[JsonProperty("hs_favorite")]
			public bool IsHsFavorite { get; }

			[JsonProperty("bg_favorite")]
			public bool IsBgFavorite { get; }
		}

		public class PetVariant
		{
			public PetVariant(HearthMirror.Objects.PetVariant variant)
			{
				PetId = variant.PetId;
				CardDbfId = variant.CardDbfId;
				IsHsFavorite = variant.IsHsFavorite;
				IsBgFavorite = variant.IsBgFavorite;
			}

			[JsonProperty("pet_id")]
			public int PetId { get; }

			[JsonProperty("card_dbf_id")]
			public int CardDbfId { get; }

			[JsonProperty("hs_favorite")]
			public bool IsHsFavorite { get; }

			[JsonProperty("bg_favorite")]
			public bool IsBgFavorite { get; }
		}
	}

	public class MercenariesCollection : CollectionBase
	{
		public MercenariesCollection(ulong accountHi, ulong accountLo, BattleTag battleTag, List<CollectionMercenary> collection) : base(accountHi, accountLo, battleTag)
		{
			Mercenaries = collection.Select(x => new Mercenary(x)).OrderBy(x => x.Id).ToList();
		}

		[JsonProperty("mercenaries")]
		public List<Mercenary> Mercenaries { get; }

		public class Mercenary
		{
			public Mercenary(CollectionMercenary merc)
			{
				Id = merc.Id;
				Level = merc.Level;
				Coins = merc.CurrencyAmount;
				Abilities = merc.Abilities.Select(x => new Ability(x)).ToList();
				Equipment = merc.Equipments.Select(x => new Ability(x)).ToList();
				ArtVariations = merc.ArtVariations.Select(x => new ArtVariation(x)).ToList();
			}

			[JsonProperty("id")]
			public int Id { get; }

			[JsonProperty("level")]
			public int Level { get; }

			[JsonProperty("coins")]
			public long Coins { get; }

			[JsonProperty("abilities")]
			public List<Ability> Abilities { get; }

			[JsonProperty("equipment")]
			public List<Ability> Equipment { get; }

			[JsonProperty("art_variations")]
			public List<ArtVariation> ArtVariations { get; }

			public class Ability
			{
				public Ability(CollectionMercenary.Ability ability)
				{
					Id = ability.Id;
					Tier = ability.Tier;
				}

				[JsonProperty("id")]
				public int Id { get; set; }

				[JsonProperty("tier")]
				public int Tier { get; set; }
			}

			public class ArtVariation
			{
				public ArtVariation(CollectionMercenary.ArtVariation artVariation)
				{
					DbfId = artVariation.DbfId;
					Premium = artVariation.Premium;
				}

				[JsonProperty("dbf_id")]
				public int DbfId { get; set; }

				[JsonProperty("premium")]
				public int Premium { get; set; }
			}
		}
	}
}
