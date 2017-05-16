## __Release v1.2.6 - In Development__

__New__:
- Now using Hearthstones new deck code system for importing and exporting decks.
- Added more stats, recent games, as well as global stats (if available) for the selected deck.
- Added "Most Played" as a deck sorting option.

__Changes__:
- Greatly improved the deck screenshot dialog.
  - Deck images can now also be copied to the clipboard.
- Deck editor has been moved to it's own flyout.
- Deck version history has been moved to it's own flyout and can now be accessed via the `DECK` menu item or the decks context menu.
- Importing decks from a list of card names now also gives a language selection option if english is the active language but an alternative language is selected.

__Fixes__:
- Fixed an issue where enchantment cards would be tracked in some cases.


## __Release v1.2.5 - 2017-05-02__
- Fixed several crashes.


## __Release v1.2.4 - 2017-05-01__

__New__:
- Added a simple way on decks to open the corresponding HSReplay.net deck page.
- Added Brazilian Portuguese translation.

__Changes__:
- Exporting decks to Hearthstone will now warn you about missing cards before starting the export process.
- Wild deck are now indicated with the proper icon (instead of the 'S' on standard decks).

__Fixes__:
- Fixed an issue where HDT would have trouble picking up the correct Hearthstone path.


## __Release v1.2.3 - 2017-04-12__

__Changes__:
- Hopefully made some improvements to issues with the overlay hiding behind Hearthstone.
  - If you are still having problems, try running HDT as administrator.
- Drag & Dropping plugins into the plugins options menu will now install them without requiring a restart.

__Fixes__:
- Fixed an issue where Direhorn Hatchling would not add a token to the deck.
- Fixed an issue where Patches would remain in the decklist in some cases.
- Fixes an issue where transformed cards would not be displayed correctly in the opponents deck list (Shifter Zerus, Molten Blade).
- Fixed an issue where quests would not count towards the spell counter.

__API__:
- Quests now properly emit `OnPlay` events.


## __Release v1.2.2 - 2017-04-07__

__Fixes__:
- Fixed an issue where Quests would trigger the secret list.
- Fixed an issue where some cards were not in the correct sets.


## __Release v1.2.1 - 2017-04-04__

__Fixes__:
- Fixed an issue where the Tracking (Hunter) pick would not be tracked correctly.
- Fixed an issue where the Desktop/Start shortcut was not updated properly.


## __Release v1.2.0 - 2017-04-04__

__Updated for 8.0.0.18336__
- Fixed deck tracking :).
- The UI will consider rotating sets wild even before they are. This will not affect your stats.

__New__:
- Added Ukranian translation.

__Changes__:
- Secret helper now only displays standard secrets in arena.
- The local replay system has now been fully removed.

__Fixes__:
- Fixes an issue where region detection would not work in some cases
  - This also fixes some issues with gold progress tracking since it relies on regions.
- Fixed an issue that prevented disabling no-deck mode.
- Fixed an issue where importing decks from meta tags would not properly decode values.


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
