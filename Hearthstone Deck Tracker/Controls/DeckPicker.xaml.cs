using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hearthstone_Deck_Tracker
{
    /// <summary>
    /// Interaction logic for DeckPicker.xaml
    /// </summary>
    public partial class DeckPicker : ListBox
    {
        public class HsClass
        {
            public List<Deck> Decks;
            public string GetName { get { return (Name=="Back") ? "Back" : Name + " (" + Decks.Count + ")"; } }
            public string Name;
            public HsClass(string name)
            {
                Name = name;
                Decks = new List<Deck>();
            }
            public Color ClassColor
            {
                get
                {
                    switch (Name)
                    {
                        case "Druid":
                            return (Color)ColorConverter.ConvertFromString("#FF7D0A");
                            break;
                        case "Death Knight":
                            return (Color)ColorConverter.ConvertFromString("#C41F3B");
                            break;
                        case "Hunter":
                            return (Color)ColorConverter.ConvertFromString("#ABD473");
                            break;
                        case "Mage":
                            return (Color)ColorConverter.ConvertFromString("#69CCF0");
                            break;
                        case "Monk":
                            return (Color)ColorConverter.ConvertFromString("#00FF96");
                            break;
                        case "Paladin":
                            return (Color)ColorConverter.ConvertFromString("#F58CBA");
                            break;
                        case "Priest":
                            return (Color)ColorConverter.ConvertFromString("#FFFFFF");
                            break;
                        case "Rogue":
                            return (Color)ColorConverter.ConvertFromString("#FFF569");
                            break;
                        case "Shaman":
                            return (Color)ColorConverter.ConvertFromString("#0070DE");
                            break;
                        case "Warlock":
                            return (Color)ColorConverter.ConvertFromString("#9482C9");
                            break;
                        case "Warrior":
                            return (Color)ColorConverter.ConvertFromString("#C79C6E");
                            break;
                        default:
                            return Colors.Gray;
                            break;
                    }
                }
            }
        }

        private readonly List<string> _classNames = new List<string> { "Druid", "Hunter", "Mage", "Paladin", "Priest", "Rogue", "Shaman", "Warlock", "Warrior"};
        private readonly List<HsClass> _hsClasses;
        private readonly bool _initialized;
        private bool _inClassSelect;

        public Deck SelectedDeck;

        public delegate void SelectedDeckHandler(DeckPicker sender, Deck deck);

        public event SelectedDeckHandler SelectedDeckChanged;

        public DeckPicker()
        {
            InitializeComponent();

            SelectedDeck = null;
            _hsClasses = new List<HsClass>();
            foreach (var className in _classNames)
            {
                _hsClasses.Add(new HsClass(className));
            }
            _hsClasses.Add(new HsClass("Undefined"));
            foreach (var hsClass in _hsClasses)
            {
                ListboxPicker.Items.Add(hsClass);
            }
            _inClassSelect = true;
            _initialized = true;
        }

        public void AddDeck(Deck deck)
        {
            var hsClass = _hsClasses.FirstOrDefault(c => c.Name == deck.Class) ?? _hsClasses.First(c => c.Name == "Undefined");
            hsClass.Decks.Add(deck);
        }

        public void AddAndSelectDeck(Deck deck)
        {
            AddDeck(deck);
            SelectDeck(deck);
            
        }
        public void SelectDeck(Deck deck)
        {
            var hsClass = _hsClasses.FirstOrDefault(c => c.Name == deck.Class) ?? _hsClasses.First(c => c.Name == "Undefined");
            if (hsClass != null)
            {
                ListboxPicker.Items.Clear();
                ListboxPicker.Items.Add(new HsClass("Back"));
                foreach (var d in hsClass.Decks)
                {
                    ListboxPicker.Items.Add(d);
                }
                ListboxPicker.SelectedItem = deck;
                _inClassSelect = false;
            }
        }

        public void RemoveDeck(Deck deck)
        {
            var hsClass = _hsClasses.FirstOrDefault(c => c.Decks.Contains(deck));
            if (hsClass != null)
            {
                hsClass.Decks.Remove(deck);
                
            }
            if (ListboxPicker.Items.Contains(deck))
            {
                ListboxPicker.Items.Remove(deck);
            }
        }
        
        private void ListboxPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            if (ListboxPicker.SelectedIndex == -1) return;

            if (_inClassSelect)
            {
                var selectedClass = ListboxPicker.SelectedItem as HsClass;
                if (selectedClass != null)
                {
                    ListboxPicker.Items.Clear();
                    ListboxPicker.Items.Add(new HsClass("Back"));
                    foreach (var deck in selectedClass.Decks)
                    {
                        ListboxPicker.Items.Add(deck);
                    }
                    _inClassSelect = false;
                }
                
            }
            else
            {
                var selectedClass = ListboxPicker.SelectedItem as HsClass;
                if (selectedClass != null)
                {
                    if (selectedClass.Name == "Back")
                    {
                        ListboxPicker.Items.Clear();
                        foreach (var hsClass in _hsClasses)
                        {
                            ListboxPicker.Items.Add(hsClass);
                        }
                        _inClassSelect = true;
                    }
                }
                else
                {
                    var selectedDeck = ListboxPicker.SelectedItem as Deck;
                    if (selectedDeck != null)
                    {
                        SelectedDeckChanged(this, selectedDeck);
                        SelectedDeck = selectedDeck;
                    }
                } 

            }
        }

    }
}
