Hearthstone-Deck-Tracker
========================

###[Download](https://github.com/Epix37/Hearthstone-Deck-Tracker/releases)
###[FAQ](https://github.com/Epix37/Hearthstone-Deck-Tracker/wiki/FAQ)

[![Join the chat at https://gitter.im/Epix37/Hearthstone-Deck-Tracker](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Epix37/Hearthstone-Deck-Tracker?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This is an automatic deck tracker for Hearthstone.

The ingame overlay:

![Overlay](http://i.imgur.com/EWd6Ung.jpg "The overlay")

The app: 

![Tracker](http://i.imgur.com/FE7Xwxl.png "The tracker")


Features:
=========
- **Tracks**:
  - Cards left in your deck or cards drawn from your deck.
  - Your handcount, deckcount and draw chances.
  - Cards played by your opponent.
  - Your opponent's handcount, deckcount and probablities of him having/drawing cards.
  - How long your opponent had each card in his hand and what cards have been mulliganed, stolen or returned.  
- **Timer** for the current turn and total time spent for you and your opponent.  
- The tracker tries to **automatically select the deck you are playing**.  
- The cards and timer can either be displayed in an overlay (see screenshot) or in **extra windows** (Options > General > Additional Windows)  
- **Deck Manager**:
  - **Import** decks from websites: arenavalue, hearthstats, hearthpwn, hearthhead, hearthstoneplayers, tempostorm, hearthstonetopdeck and hearthnews  
  - Circumvent the 9 deck limit: Saved decks can be **exported to Hearthstone**. (My Decks > More...)   
  - Decks can be filtered by custom **tags** and sorted by name, date and tags.  
  - Set **notes** for each deck (My Decks > More...)  
  - Create **screenshots** of decks (My Decks > More...)  
  - **Share** your decks by exporting them as xml files or id-strings (My Decks > More...). Both can be imported via New Deck > Import.  
- **Notifications**: get notified when a game or a turn starts (either by the tray icon flashing or hearthstone popping up)  
- **Customization**: Almost every feature can be turned on/off separately.
- **Stats per deck**:
  - Track the result of each game (win/loss), opponents, game mode and more
  - Win/loss rate vs each class.
  - Details for each game (cards drawn, played, etc.).
  - Select which game modes to track (Options > Other).  
  - Import your opponent's (partial) deck from a tracked game as a new deck.

![Stats](http://i.imgur.com/Wke3Cuw.png "Deck stats")

- **Replays**:
![Stats](http://i.imgur.com/tuxOFmg.png "Deck stats")


How to use: 
===========
1) Download latest release [here](https://github.com/Epix37/Hearthstone-Deck-Tracker/releases) (Hearthstone.Deck.Tracker-vx.xx.xx.zip)  
2) Extract file  
3) Run "Hearthstone Deck Tracker.exe"  
4) Create your decks under "New" (or import from any of the supported websites), click save.  
5) Play!

[Video guide / feature overview by TheAdipose](https://www.youtube.com/watch?v=gNVlF83w-wY) (v0.5.6)

FAQ:
=============
[Go to FAQ](https://github.com/Epix37/Hearthstone-Deck-Tracker/wiki/FAQ)

How it works:
=============
The automated tracking is done my reading out Hearthstone logfile.  
More information on that here:  http://www.reddit.com/r/hearthstone/comments/268fkk/simple_hearthstone_logging_see_your_complete_play

Is Blizzard okay with this?
=============
[Yes](https://twitter.com/bdbrode/status/511151446038179840)  
[It's not against the TOS](https://twitter.com/CM_Zeriyah/status/589171381381672960)

How to start Hearthstone-Deck-Tracker with the launcher?
=============
- Create a .bat file with this code inside:
```
@echo off
start "" "C:\Program Files (x86)\Hearthstone\Hearthstone Beta Launcher.exe"
start "" "C:\Program Files (x86)\Hearthstone Deck Tracker\Hearthstone Deck Tracker.exe"
```
  
Requirements:
=============
- Windows Vista or higher
- .NET Framework 4.5


Donations are always appreciated
[![PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PZDMUT88NLFYJ)
