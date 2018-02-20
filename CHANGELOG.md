## __Release v1.5.12 - 2018-02-06__
__Updated for Hearthstone 10.2.0.23180__

__New__:
- Added an option for Hearthstone to flash when receiving a friendly challenge `(options > tracker > general)`.

__Fixes__:
- Fixed an issue where stats on deck tiles would not update.
- Fixed an issue where spectator games would somtimes be attached to the wrong decks.
- Fixed an issue with the card tooltip font for some languages.


## __Release v1.5.11 - 2018-01-17__

__Fixes__:
- Fixed an issue where the "Toggle Secrets" hotkey wasn't working.
- Fixed several issues with UI components not updating when changing languages/colors.
- Fixed an issue where The Candle wasn't tracked correctly.
- Fixed the image for Psychic Scream.


## __Release v1.5.10 - 2017-12-18__

- Fixed tracking of cards created by: Dead Man's Hand, Psychic Scream, Spiteful Summoner, Dragon's Fury, and several others.


## __Release v1.5.9 - 2017-12-15__

- Fixed an issue where some other cards would no longer be tracked due to the previous update.


## __Release v1.5.8 - 2017-12-14__

- Fixed an issue where Togwaggle and his King's Ransom would reveal more information than they should.


## __Release v1.5.7 - 2017-12-13__

__New:__
- Added support for dungeon run decks.
	- Settings for this can be found under `options (advanced) > tracker > importing`.
- Added filters for dungeon and brawl decks to the decks list filters.

__Fixes:__
- Fixed tracking for cards created by: Fal'dorei Strider, Kingsbane and Scroll of Wonders.
- Fixed an issue where Sudden Betrayal would sometimes grey out when attacking with the hero.
- Fixed an issue where the The Darkness token would count towards the attack counter.
- Fixed a performance issue when "Advanced window search" was enabled.
- Fixed several crashes.


## __Release v1.5.6 - 2017-12-07__

__Updated for Kobolds and Catacombs__

__Fixes__:
- Fixed an issue where flavor text tooltips were barely readable in some cases.

__Changes__:
- Improved secret tracking logic for Hidden Cache.


## __Release v1.5.5 - 2017-11-06__

__Updated for Hearthstone 9.4.0.22115__


## __Release v1.5.4 - 2017-10-28__

- Fixed an issue that caused transformed cards to not be recognized correctly (e.g. Fatespinner or Shifter Zerus).


## __Release v1.5.3 - 2017-10-26__

- Fixed an issue that could cause HDT to crash when trying to authenticate with HSReplay.net.


## __Release v1.5.2 - 2017-10-26__

__New__:
- Our Twitch Overlay Extension is now available! (`options (advanced) > streaming > twitch extension`) 

__Fixes__:
- Fixed an issue where starting Battle.net/Hearthstone was not working with the Battle.net beta client.


## __Release v1.5.1 - 2017-10-18__

- Fixed an issue where games played with the new Warlock hero would not be recorded correctly.


## __Release v1.5.0 - 2017-10-17__

__Updated for Hearthstone 9.2.0.21517__

__Fixes__:
- Fixed an issue where Wild Secrets would show up in Arena.


## __Release v1.4.4 - 2017-09-20__

Fixed a crash on startup when the config was set to be stored locally.


## __Release v1.4.3 - 2017-09-19__

__Updated for Hearthstone 9.1.0.20970__

__New__:
- Added support to use the windows native accent color.
- Added hotkey functionality for toggling the "My Games" panel.

__Changes__:
- Deck codes can now also be pasted into the deck editor.
- Several opponent overlay options have been moved from `general` to `opponent`.


## __Release v1.4.2 - 2017-09-05__

- Fixed an issue where cards from the Classic set could not be set as arena rewards.


## __Release v1.4.1 - 2017-08-24__

__Fixes__:
- Fixed an issue where cards destroyed by Skulking Geist would sometimes not be tracked.
- Fixed an issue where Adventure heroes would show up in the card lists.
- Fixed an issue where entering card names in the arena rewards dialog would not work if the UI language was not english.
- Fixed several issues related to secrets tracking.


## __Release v1.4.0 - 2017-08-08__

__Updated for Knights of the Frozen Throne__

__Changes__:
- Opponent card markers now scale with the opponent scaling value set in `options > overlay > opponent`.

__API__:
- Added GameEvents.OnModeChanged


## __Release v1.3.6 - 2017-07-30__

__New__:
- Added German and French translations.
- Added a "Missing cards" tab to the "Export" flyout, which will show you any cards you are missing for that deck.

__Fixes__:
- Fixed an issue where deck importing would not find an existing deck in versions.
- Fixed an issue where auto deck selection would not always find the correct version.
- Fixed an issue where the deck editor would not find non-english card names.
- Fixed an issue where the "This week" timeframe filter would not always use the correct first day of the week.
- Fixed an issue where global statistics would not be loaded for wild decks.

__Changes__:
- Deck code export now has an option to include the version number in the name.
- The global statistics is now hidden while an arena deck is selected.
- Improved touch screen behavior in several places.


## __Release v1.3.5 - 2017-06-03__

__Fixes__:
- Fixed an issue where the export deck dialog would not respect the selected deck version.
- Fixed an issue where editing and saving a deck as the current version would not update it in the UI.
- Fixed an issue where Mirror Entity would be grayed out while the opponent has a full board.
- Fixed several crashes.

__Changes__:
- Added an option to hide the new "my games" panel: `options (advanced) > tracker > general`


## __Release v1.3.2 - 2017-06-02__

__Updated for 8.2.0.19506__
- Fixed a crash when starting/exiting a match.
- Now using Hearthstones new deck code system for importing and exporting decks.


## __Release v1.3.1 - 2017-06-01__
- Fixed a crash on startup if the legacy deck style was selected.


## __Release v1.3.0 - 2017-06-01__

__New__:
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
