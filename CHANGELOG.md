## __Release v1.1.7 - 2017-03-01__

__Updated for 7.1.0.17720__

__New__:
- HDT will now notify you if important options (automatic importing and deck selection) are disabled.
- Added Korean and Japanese translations.

__Changes__:
- Trying to start Battle.net/Hearthstone should now time out later.

__Fixes__:
- Drag and drop installing should now work for all plugins.


## __Release v1.1.6 - 2017-01-12__

__New__:
- Added an option to remove detected secrets instead of just greying them out.
  - The option can be found under `options (advanced) > overlay > general`.

__Fixes__:
- Fixed an issue where cards revealed by Jousts would not be tracked.
- Fixed an issue where HDT would crash when starting some brawls.
- Fixed an issue where HDT would force no-deck mode when spectating.


## __Release v1.1.5 - 2016-12-22__

__New__:
- Added Jade Golem counters.
- Now translated into Traditional Chinese (zh-TW).

__Changes__:
- Fully disabled HearthStats integration to prevent deck corruption.
- The arena rewards dialog now accepts and auto completes card names for selected secondary card languages.

__Fixes__:
- Fixed an issue where auto deck importing and selection would not work for brawl.
- Fixed an issue where cards created by Manic Soulcaster and White Eyes would not be tracked.
- Fixed an issue where the "this week" stats filter would not work properly.
- Fixed an issue where the "max rank" stats filter would not recognize legend ranks.


## __Release v1.1.4 - 2016-12-04__

__Changes__:
- Cards created by Jade Idol and White Eyes are now properly tracked.

__Fixes__:
- Fixed an issue where the secrets list would not disappear after all secrets were triggered.
- Fixed an issue where importing decks from the game would not work in some cases.
- Fixed an issue where the "Minimal" card theme would not display any images.
- Fixed an issue where creating deck screenshots would crash HDT.


## __Release v1.1.2 - 2016-11-30__

__Updated for 7.0.0.15590__
- Known issue: The "Hidden Cache" Hunter secret does not automatically grey out.

__Changes__:
- You can now paste (ctrl-v) deck urls directly into HDT, rather than having to use the `IMPORT` menu.
- The installation and update process should be faster now.

__Fixes__:
- Fixed an issue where games would not be sorted by duration properly.


## __Release v1.1.1 - 2016-10-04__

__Updated for 6.1.3.14830__

__Fixes__:
- Fixed an issue where the deck builder would not work properly with some UI languages.
- Minor text changes.


## __Release v1.1.0 - 2016-09-21__

__New__:
- Greatly improved replays! More information after the update notes.
- HDT is now translated into Chinese and Russian.
  - You can change the language under `options > tracker > appearance`
  - Other languages are coming soon! If you want to help with translation, there is a link next to the opion.
- The (Yogg-Saron) counter `Auto` mode now also works for Arcane Giant.

__Fixes__:
- Fixed an issue where Competitive Spirit would sometimes incorrectly be greyed out.
- Fixed an issue where Cat Trick was missing from the secrets list.
- Fixed an issue where card images were missing occasionally in some cases.
- Fixed an issue where HDT would crash during exporting or when opening/closing the friendslist.

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
