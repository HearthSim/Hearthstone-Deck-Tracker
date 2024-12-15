using System;
using System.Collections.Generic;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.RegionDrawer
{
	public class RegionDrawer
	{
		private const double BaseHeight = 1080.0;

		private const double CardHeight = 0.39;
		private const double CardAspectRatio = 28 / (CardHeight * 100);

		private const double HeroPowerHeight = 0.34;
		private const double HeroPowerAspectRatio = 24 / (HeroPowerHeight * 100);

		private const double EnchantHeightFactor = 0.07;
		private const double EnchantWidth = 215.0;
		private const double CardToEnchantOffsetX = 0.035;

		private const double TooltipHeightFactor = 0.08;
		private const double TooltipWidth = 220.0;
		private const double CardToToolTipOffsetX = 0.23;
		private const double CardToToolTipOffsetY = 0.025;

		private const double HandCardToToolTipOffsetX = 0.275;
		private const double HandCardHeight = 0.5;
		private const double HandCardAspectRatio = 34 / (HandCardHeight * 100);
		private const double HandTooltipWidth = 230.0;

		private const double SecretCardHeight = 0.43;
		private const double WeaponCardHeight = 0.37;
		private const double WeaponToToolTipOffsetX = 0.21;

		private const double TrinketHeight = 0.365;
		private const double TrinketToToolTipOffsetX = 0.2;
		private const double TrinketAspectRatio = 25 / (TrinketHeight * 100);

		private const double BgHeroPickHeroWidth = 0.1725;
		private const double BgHeroPickHeroXSpacing = 0.0635;
		private const double BgHeroPickTooltipHeight = 0.32;
		private const double BgHeroPickTooltipAspectRatio = 25 / (BgHeroPickTooltipHeight * 100);

		private double Height { get; }
		private double Width { get; }
		private double ScreenRatio { get; }

		public RegionDrawer(double height, double width, double screenRatio)
		{
			Height = height;
			Width = width;
			ScreenRatio = screenRatio;
		}

		public Rect DrawCardRegion(double offsetX, double offsetY, double cardHeight = CardHeight, double aspectRatio = CardAspectRatio)
		{
			var heightScaling = Height / BaseHeight;

			var heightInPixels = cardHeight * heightScaling * BaseHeight;
			var widthInPixels = heightInPixels * aspectRatio;
			//var widthInPixels = cardHeight * heightScaling * BaseHeight * aspectRatio;

			var normalizedHeight = heightInPixels / Height;
			var normalizedWidth = widthInPixels / Width;

			var posX = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio) / Width;
			var posY = offsetY;

			return new Rect(posX, posY, normalizedWidth, normalizedHeight);
		}

		public Rect DrawFriendsListRegion()
		{
			var heightScaling = Height / BaseHeight;

			var heightInPixels = 0.68 * heightScaling * BaseHeight;
			var widthInPixels = 0.38 * heightScaling * BaseHeight;

			var normalizedHeight = heightInPixels / Height;
			var normalizedWidth = widthInPixels / Width;

			return new Rect(0, 0.27, normalizedWidth, normalizedHeight);
		}

		public Rect DrawHeroPowerRegion(double offsetX, double offsetY, double heroPowerHeight = HeroPowerHeight)
		{
			var heightScaling = Height / BaseHeight;

			var heightInPixels = heroPowerHeight * heightScaling * BaseHeight;
			var widthInPixels = heightInPixels * HeroPowerAspectRatio;

			var normalizedHeight = heightInPixels / Height;
			var normalizedWidth = widthInPixels / Width;

			var posX = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio) / Width;
			var posY = offsetY;

			return new Rect(posX, posY, normalizedWidth, normalizedHeight);
		}

		public Rect DrawCardTooltipRegion(double offsetX, double offsetY, double height, double tooltipWidth = TooltipWidth)
		{
			var heightScaling = Height / BaseHeight;

			var heightInPixels = TooltipHeightFactor * heightScaling * BaseHeight * height;
			var widthInPixels = tooltipWidth * heightScaling;

			var normalizedHeight = heightInPixels / Height;
			var normalizedWidth = widthInPixels / Width;

			var posX = Helper.GetScaledXPos(offsetX, (int)Width, ScreenRatio) / Width;
			var posY = offsetY + CardToToolTipOffsetY;

			return new Rect(posX, posY, normalizedWidth, normalizedHeight);
		}

		public Rect DrawCardEnchantRegion(double offsetX, double offsetY, double height)
		{
			var heightScaling = Height / BaseHeight;

			var heightInPixels = EnchantHeightFactor * heightScaling * BaseHeight * height;
			var widthInPixels = EnchantWidth * heightScaling;

			var normalizedHeight = heightInPixels / Height;
			var normalizedWidth = widthInPixels / Width;

			var posX = Helper.GetScaledXPos(offsetX + CardToEnchantOffsetX, (int)Width, ScreenRatio) / Width;
			var posY = offsetY + CardHeight;

			return new Rect(posX, posY, normalizedWidth, normalizedHeight);
		}

		public List<Rect> DrawBoardCardRegions(int minionCount, int position, bool isPlayerBoard, double tooltipHeight, double enchantHeight)
		{
			var baseOffsetX = 0.2;
			var baseOffsetY = 0.35;
			var regions = new List<Rect>();

			var centerPosition = (minionCount + 1) / 2.0;

			// Calculate the offset for each minion based on its position relative to the center
			var relativePosition = position - centerPosition;
			var offsetX = relativePosition >= -0.5 ? baseOffsetX + (relativePosition) * 0.098 : baseOffsetX + 0.375 + (relativePosition *  0.098);
			var offsetY = isPlayerBoard ? baseOffsetY : 0.175;

			// Draw the regions for each minion using the calculated offsets
			var rectCard = DrawCardRegion(offsetX, offsetY);
			var tooltipOffsetX = offsetX + CardToToolTipOffsetX;
			var rectTooltip = DrawCardTooltipRegion(relativePosition <= 0.5 ? tooltipOffsetX : tooltipOffsetX - CardHeight, offsetY, tooltipHeight);
			var rectEnchants = DrawCardEnchantRegion(offsetX, offsetY, enchantHeight);
			regions.Add(rectCard);
			regions.Add(rectTooltip);
			regions.Add(rectEnchants);

			return regions;
		}

		public List<Rect> DrawSecretCardRegions(int position, bool isPlayerBoard, double tooltipHeight)
		{
			var baseOffsetX = 0.57;
			var baseOffsetY = isPlayerBoard ? 0.445 : 0;
			var regions = new List<Rect>();

			var offsetYByLayer =  new [] { 0.0, 0.030, 0.08 };
			var leftOffsetXByLayer =  new [] { 0.0, 0.037, 0.062 };
			var rightOffsetXByLayer =  new [] { 0.0, 0.034, 0.059 };

			var centerPosition = 1;

			// Calculate the offset for each secret based on its position relative to the center
			var relativePosition = position - centerPosition;
			var isLeftSide = relativePosition % 2 != 0;
			var layer = (int)Math.Ceiling(relativePosition / 2.0);

			// Limit is 5 secrets
			if(layer > 2) return regions;

			var offsetX = isLeftSide ? baseOffsetX - leftOffsetXByLayer[layer] : baseOffsetX + rightOffsetXByLayer[layer];
			var offsetY = baseOffsetY + (isPlayerBoard ? offsetYByLayer[layer] : 0);

			// Draw the regions for each secret using the calculated offsets
			var rectCard = DrawCardRegion(offsetX, offsetY, SecretCardHeight);
			var tooltipOffsetX = offsetX + HandCardToToolTipOffsetX;
			var rectTooltip = DrawCardTooltipRegion(relativePosition < 0 ? tooltipOffsetX : tooltipOffsetX - 0.44, offsetY + 0.008, tooltipHeight * 1.08, HandTooltipWidth);

			regions.Add(rectCard);
			regions.Add(rectTooltip);

			return regions;
		}

		public List<Rect> DrawHandCardRegions(int cardCount, int position, bool isPlayerBoard, string? cardType, double tooltipHeight, double enchantHeight)
		{
			if(!isPlayerBoard) return new List<Rect>();

			var isHero = cardType == "Hero";

			double cardTotal = cardCount > 10 ? cardCount : 10;

			var baseOffsetX = 0.34;
			var baseOffsetY = 1 - HandCardHeight;
			var regions = new List<Rect>();

			var centerPosition = (cardCount + 1) / 2.0;

			// Calculate the offset for each minion based on its position relative to the center
			var relativePosition = position - centerPosition;
			var offsetXScale = cardCount > 3 ? cardTotal / cardCount * 0.037 : 0.098;

			var offsetX = baseOffsetX + relativePosition * offsetXScale;
			var offsetY = baseOffsetY;

			// Draw the regions for each minion using the calculated offsets
			var tooltipOffsetX = offsetX + HandCardToToolTipOffsetX;

			var rectCard = DrawCardRegion(offsetX, offsetY, HandCardHeight, HandCardAspectRatio);

			var rectTooltip = DrawCardTooltipRegion(relativePosition < 0.5 ? tooltipOffsetX : tooltipOffsetX - 0.44, offsetY + 0.008, tooltipHeight * 1.08, HandTooltipWidth);

			if (isHero)
			{
				var isLeftSide = relativePosition >= 0.5;
				rectTooltip = DrawCardTooltipRegion(isLeftSide ? tooltipOffsetX : tooltipOffsetX - 0.44, offsetY + 0.008, tooltipHeight * 1.08, HandTooltipWidth);

				var rectHeroPower = DrawHeroPowerRegion(isLeftSide ? offsetX - 0.22 : offsetX + 0.26, offsetY + 0.01, HeroPowerHeight + 0.08);
				regions.Add(rectHeroPower);
			}

			var rectEnchants = DrawCardEnchantRegion(offsetX, offsetY, enchantHeight);
			regions.Add(rectCard);
			regions.Add(rectTooltip);
			regions.Add(rectEnchants);

			return regions;
		}

		public Rect DrawHeroPowerRegion(bool isPlayerBoard)
		{
			var baseOffsetX = 0.7;
			var baseOffsetY = isPlayerBoard ? 0.57 : 0.04;

			var rect = DrawHeroPowerRegion(baseOffsetX, baseOffsetY, isPlayerBoard ? HeroPowerHeight + 0.02 : HeroPowerHeight);
			return rect;
		}

		public List<Rect> DrawWeaponRegions(bool isPlayerBoard, double tooltipHeight, double enchantHeight)
		{
			var baseOffsetX = isPlayerBoard? 0.45 : 0.46;
			var baseOffsetY = isPlayerBoard ? 0.575 : 0.035;
			var regions = new List<Rect>();

			// Draw the regions for each minion using the calculated offsets
			var rectCard = DrawCardRegion(baseOffsetX, baseOffsetY, WeaponCardHeight);
			var tooltipOffsetX = baseOffsetX + WeaponToToolTipOffsetX;

			var rectTooltip = DrawCardTooltipRegion(tooltipOffsetX, baseOffsetY + 0.01, tooltipHeight);
			var rectEnchants = DrawCardEnchantRegion(baseOffsetX - 0.0075, baseOffsetY - 0.02, enchantHeight);

			regions.Add(rectCard);
			regions.Add(rectTooltip);
			regions.Add(rectEnchants);

			return regions;
		}

		public List<Rect> DrawBgTrinketRegions(int position, bool hasAttachedCard, bool isPlayerBoard, double tooltipHeight)
		{
			var baseOffsetX = 0.463;
			var baseOffsetY = isPlayerBoard ? 0.525 : 0;
			var regions = new List<Rect>();

			var relativePosition = position - 1;

			var offsetX = baseOffsetX - relativePosition * 0.0525;
			var offsetY = isPlayerBoard ? baseOffsetY - relativePosition * 0.06 : baseOffsetY - 0.02 + (relativePosition * 0.01);

			var rectCard = DrawCardRegion(offsetX, offsetY, TrinketHeight, TrinketAspectRatio);
			var tooltipOffsetX = hasAttachedCard ? offsetX - 0.17 : offsetX + TrinketToToolTipOffsetX;
			var rectTooltip = DrawCardTooltipRegion(tooltipOffsetX, offsetY + 0.008, tooltipHeight * 1.08, HandTooltipWidth);

			regions.Add(rectCard);
			regions.Add(rectTooltip);

			if(hasAttachedCard)
			{
				var rectAttachedCard = DrawCardRegion(offsetX + 0.19, offsetY + 0.015, TrinketHeight, TrinketAspectRatio);
				regions.Add(rectAttachedCard);
			}

			return regions;
		}

		public List<Rect> DrawBgHeroTrinketRegions(bool isPlayerBoard,bool hasAttachedCard, double tooltipHeight)
		{
			var baseOffsetX = 0.383;
			var baseOffsetY = isPlayerBoard ? 0.48 : 0;
			var regions = new List<Rect>();

			var offsetX = baseOffsetX;
			var offsetY = isPlayerBoard ? baseOffsetY : baseOffsetY - 0.03;

			var rectCard = DrawCardRegion(offsetX, offsetY, TrinketHeight, TrinketAspectRatio);
			var tooltipOffsetX = offsetX - TrinketToToolTipOffsetX + 0.027;
			var rectTooltip = DrawCardTooltipRegion(tooltipOffsetX, offsetY + 0.008, tooltipHeight * 1.08, HandTooltipWidth);

			regions.Add(rectCard);
			regions.Add(rectTooltip);

			if(hasAttachedCard)
			{
				var rectAttachedCard = DrawCardRegion(offsetX + 0.19, offsetY + 0.015, TrinketHeight, TrinketAspectRatio);
				regions.Add(rectAttachedCard);
			}

			return regions;
		}

		public List<Rect> DrawBgHeroPickingTooltipRegion(int zoneSize, int zonePosition, bool tooltipOnRight, int numCards)
		{
			var regions = new List<Rect>();

			// At this time we're only confident about the layout if the zone contains exactly 4 targets
			// (as is the case for Battlegrounds Hero Picking)
			if(zoneSize != 4)
				return regions;

			var totalWidth = zoneSize * BgHeroPickHeroWidth + (zoneSize - 1) * BgHeroPickHeroXSpacing;
			var leftEdge = 0.50 - totalWidth / 2; //0.065;

			var zoneIndex = Math.Max(zonePosition - 1, 0);
			var heroX = leftEdge + zoneIndex * (BgHeroPickHeroWidth + BgHeroPickHeroXSpacing); //* 0.2365;
			var heroY = 0.29;

			var offsetX = heroX + (tooltipOnRight ? 0.135 : -0.16);
			var offsetY = heroY - 0.075;

			var rectCard = DrawHeroPowerRegion(offsetX, offsetY, 0.39);
			regions.Add(rectCard);

			return regions;
		}
	}
};
