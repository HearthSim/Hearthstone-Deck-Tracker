Hearthstone-Deck-Tracker
========================

This is an automatic deck tracker for Hearthstone.

**Features:**  
- Tracking:
  - Cards left in your deck or cards drawn from your deck.
  - Your handcount, deckcount and draw chances.
  - Cards played by your opponent.
  - Your opponent's handcount, deckcount and probablities of him having/drawing cards.
  - How long your opponent had each card in his hand.
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
- My deck is not on the screen or far off to the right
 - This seems to be a problem with laptops for some reason. You will have to set Custom Width/Height and maybe Offset values in the config.xml. [See here for reference](http://www.reddit.com/r/hearthstone/comments/26seey/automatic_deck_tracker_and_more_with_ingame/chv32lx) 
tl;dr: Open the config.xml, set VisibleOverlay to true, set CustomWidth and CustomHeight so something around 20% lower than Hearthstone's resolution. You may have to play around with those values a bit. If you have any idea why this happens please contact me

Donations are much appreciated
[![PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PZDMUT88NLFYJ)
