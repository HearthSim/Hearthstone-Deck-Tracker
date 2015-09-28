using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility
{
    public static class Updater
    {
        public static void Cleanup()
        {
            try
            {
                if(File.Exists("HDTUpdate_new.exe"))
                {
                    if(File.Exists("HDTUpdate.exe"))
                        File.Delete("HDTUpdate.exe");
                    File.Move("HDTUpdate_new.exe", "HDTUpdate.exe");
                }
            }
            catch(Exception e)
            {
                Logger.WriteLine("Error updating updater\n" + e);
            }
            try
            {
                //updater used pre v0.9.6
                if(File.Exists("Updater.exe"))
                    File.Delete("Updater.exe");
            }
            catch(Exception e)
            {
                Logger.WriteLine("Error deleting Updater.exe\n" + e);
            }
        }
    }
}
