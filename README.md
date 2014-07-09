Hearthstone-Deck-Tracker
========================

This is an automatic deck tracker for Hearthstone.

The ingame overlay:

![Overlay](http://i.imgur.com/EWd6Ung.jpg "The overlay")

The tracker: 

![Overlay](http://i.imgur.com/T7beWjm.png "The tracker")


**Features:**  
- Tracking:
  - Cards left in your deck or cards drawn from your deck.
  - Your handcount, deckcount and draw chances.
  - Cards played by your opponent.
  - Your opponent's handcount, deckcount and probablities of him having/drawing cards.
  - How long your opponent had each card in his hand and what cards have been mulliganed, stolen or returned.
- Importing decks from hearthpwn and hearthstats.
- Exporting saved decks to hearthstone (this pretty much avoids the 9 deck limit).
- Timer for current turn and total time spent for you and your opponent.
- The tracker tries to automatically select the deck you are playing. (This starts working less well, the more similar decks you have saved)

**How it works:**  
The automated tracking is done my reading out Hearthstone logfile.  
More information on that  here: http://www.reddit.com/r/hearthstone/comments/268fkk/simple_hearthstone_logging_see_your_complete_play

**What you need to run this:**
- Windows Vista or higher (I have not actually tested it on vista but anything above XP should work fine)
- .NET Framework 4.5
- If you run this for the first time you will have to restart Hearthstone once for it start logging the way required for this tracker.

**FAQ:**
- Nothing happens
  - Try restarting Hearthstone, the Tracker, and try starting the Tracker as Administrator.
  - If that does not help, go into the config.xml file and delete what's in <HearthstoneDirectory>.
  - Do you have .NET 4.5 installed?

Donations are much appreciated
[![PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PZDMUT88NLFYJ)
