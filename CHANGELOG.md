## __Unreleased__

__New__:
- The (Yogg-Saron) counter `Auto` mode now also works for Arcane Giant.

__Fixes__:
- Fixed an issue where Competitive Spirit would sometimes incorrectly be greyed out.
- Fixed an issue where Cat Trick was missing from the secrets list.
- Fixed an issue where card images were missing occasionally in some cases.
- Fixed a memory leak.

__Changes__:
- Made several further improvemets to auto deck importing.
- Made several improvements to auto deck seletion.
- Made several improvements to the "Export to Hearthstone" feature.
- Doubleclicking decks will now activate them. With the changes to deck auto importing, editing decks within HDT should rarely be necessary.

__API__:
- Fixed an issue where `OnTurnStart` would not fire for the first turn of the game.
- Importing from web now supports meta tags (#2769)


## __Release v1.0.5 - 2016-08-10__

__Updated for Hearthstone 6.0.0.13921__:
- Added support for restarted adventure matches.

__Fixes__:
- Fixed an issue where the mana curve would not correctly display spells.
- Fixed an issue where automatic constructed importing would in some cases import multiple decks to the same local deck, resulting in a high number of versions on the deck.

__API/Plugins__:
- Added `API.GameEvents.OnEntityWillTakeDamage.`
- Added support for loading plugins with embedded dependencies - see #2735.


## __Release v1.0.4 - 2016-08-04__

__New__:
- We have a new, awesome updater! HDT will tell you all about it after you close the update notes.
- Added support for importing decks from marduktv.com.br

__Changes__:
- HDT will now try to automatically restore corrupted data files, e.g. from an unexpected shutdown.
- Plugins are now stored in `%AppData%/HearthstoneDeckTracker/Plugins`. Your existing plugins are moved there automatically.
- Custom themes are now stored in `%AppData%/HearthstoneDeckTracker/Themes`. Your existing custom themes are moved there automatically.
- You can now select all/no decks when importing from constructed.
- Plugins are now less likely to cause HDT to crash.

__Fixes__:
- Fixed an issue where the cost reduction would still be displayed for cards that were returned to hand.
- Fixed an issue where the card sorting would no longer match the game's new sorting.
- Fixed an issue where the Yogg or C'Thun counter would sometimes be incorrectly shown during brawls.
- Fixed several crashes.