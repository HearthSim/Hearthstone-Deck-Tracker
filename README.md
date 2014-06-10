Hearthstone-Deck-Tracker
========================

This is an automatic deck tracker for Hearthstone.

**Features:**  
- Tracks cards drawn from deck (or cards left in deck), handcount and drawchances of remaining cards  
- Tracks cards played by the opponent and his handcount  

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

**Known Issues:**
- There seem to be some issues with fullscreen mode in general.
- Your opponent's deck may cover up some of your friendslist. (You can hide the cards in the options menu though)
- There may still be some rare problems with the deck/hand counting. If you notice this (and optimally notices what caused it) let me know.
- Performance when moving the decks on the overlay it not the greatest currently.
