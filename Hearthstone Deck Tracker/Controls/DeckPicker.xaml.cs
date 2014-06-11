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
            public List<string> SelectedTags;

            public string TagList
            {
                get
                {
                    return "";
                }
            }

            public string GetName
            {
                get
                {
                    return (Name == "Back" || Name == "All")
                               ? Name
                               : Name + " (" +
                                 Decks.Count(
                                     d =>
                                     SelectedTags.Contains("All") ||
                                     SelectedTags.Any(tag => d.Tags.Any(deckTag => deckTag == tag))) + ")"; }
            }

            public string Name;

            public HsClass(string name)
            {
                Name = name;
                Decks = new List<Deck>();
                SelectedTags = new List<string>();
            }

            public Color ClassColor
            {
                get
                {
                    switch (Name)
                    {
                        case "Druid":
                            return (Color) ColorConverter.ConvertFromString("#FF7D0A");
                            break;
                        case "Death Knight":
                            return (Color) ColorConverter.ConvertFromString("#C41F3B");
                            break;
                        case "Hunter":
                            return (Color) ColorConverter.ConvertFromString("#ABD473");
                            break;
                        case "Mage":
                            return (Color) ColorConverter.ConvertFromString("#69CCF0");
                            break;
                        case "Monk":
                            return (Color) ColorConverter.ConvertFromString("#00FF96");
                            break;
                        case "Paladin":
                            return (Color) ColorConverter.ConvertFromString("#F58CBA");
                            break;
                        case "Priest":
                            return (Color) ColorConverter.ConvertFromString("#FFFFFF");
                            break;
                        case "Rogue":
                            return (Color) ColorConverter.ConvertFromString("#FFF569");
                            break;
                        case "Shaman":
                            return (Color) ColorConverter.ConvertFromString("#0070DE");
                            break;
                        case "Warlock":
                            return (Color) ColorConverter.ConvertFromString("#9482C9");
                            break;
                        case "Warrior":
                            return (Color) ColorConverter.ConvertFromString("#C79C6E");
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
        public bool ShowAll;
        public List<string> SelectedTags;
        private HsClass _selectedClass;

        public Deck SelectedDeck;

        public delegate void SelectedDeckHandler(DeckPicker sender, Deck deck);

        public event SelectedDeckHandler SelectedDeckChanged;

        public DeckPicker()
        {
            InitializeComponent();

            SelectedTags = new List<string>();

            SelectedDeck = null;
            _hsClasses = new List<HsClass>();
            foreach (var className in _classNames)
            {
                _hsClasses.Add(new HsClass(className));
            }
            _hsClasses.Add(new HsClass("Undefined"));

            ListboxPicker.Items.Add(new HsClass("All"));
            foreach (var hsClass in _hsClasses)
            {
                ListboxPicker.Items.Add(hsClass);
            }
            _inClassSelect = true;
            _initialized = true;
            ShowAll = false;
        }

        public void AddDeck(Deck deck)
        {
            if (deck == null) return;
            var hsClass = _hsClasses.FirstOrDefault(c => c.Name == deck.Class) ?? _hsClasses.First(c => c.Name == "Undefined");
            hsClass.Decks.Add(deck);
        }

        public void AddAndSelectDeck(Deck deck)
        {
            if (deck == null) return;
            AddDeck(deck);
            SelectDeck(deck);
            
        }
        public void SelectDeck(Deck deck)
        {
            if (deck == null) return;
            var hsClass = _hsClasses.FirstOrDefault(c => c.Name == deck.Class) ??
                          _hsClasses.First(c => c.Name == "Undefined");
            
                if (hsClass != null)
                {
                    _selectedClass = hsClass;
                    ListboxPicker.Items.Clear();
                    ListboxPicker.Items.Add(new HsClass("Back"));
                    if (ShowAll)
                    {
                        foreach (var d in _hsClasses.SelectMany(hsc => hsc.Decks))
                        {
                            ListboxPicker.Items.Add(d);
                        }
                    }
                    else
                    {
                        foreach (var d in hsClass.Decks)
                        {
                            ListboxPicker.Items.Add(d);
                        }
                    }
                    ListboxPicker.SelectedItem = deck;
                    _inClassSelect = false;
                }
            
            SelectedDeck = deck;
        }

        public void RemoveDeck(Deck deck)
        {
            if (deck == null) return;
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
            if (ListboxPicker.SelectedIndex == -1) return;
            if (!_initialized) return;
            if (_inClassSelect)
            {
                var selectedClass = ListboxPicker.SelectedItem as HsClass;
                if (selectedClass != null)
                {
                    ShowAll = selectedClass.Name == "All";

                    _selectedClass = selectedClass;

                    ListboxPicker.Items.Clear();
                    ListboxPicker.Items.Add(new HsClass("Back"));

                    if (ShowAll)
                    {
                        foreach (var d in _hsClasses.SelectMany(hsc => hsc.Decks))
                        {
                            if (SelectedTags.Any(t => t == "All") || SelectedTags.Any(t => d.Tags.Contains(t)))
                                ListboxPicker.Items.Add(d);
                        }
                    }
                    else
                    {
                        foreach (var d in selectedClass.Decks)
                        {
                            if (SelectedTags.Any(t => t == "All") || SelectedTags.Any(t => d.Tags.Contains(t)))
                                ListboxPicker.Items.Add(d);
                        }
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
                        _selectedClass = null;
                        ListboxPicker.Items.Clear();
                        ListboxPicker.Items.Add(new HsClass("All"));
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
        
        internal void SetSelectedTags(List<string> tags)
        {
            SelectedTags = tags;

            foreach (var hsClass in _hsClasses)
            {
                hsClass.SelectedTags = tags;
            }

            UpdateList();
        }

        public void UpdateList()
        {

            if (!_inClassSelect)
            {
                ListboxPicker.Items.Clear();
                ListboxPicker.Items.Add(new HsClass("Back"));
                if (ShowAll)
                {
                    foreach (var d in _hsClasses.SelectMany(hsc => hsc.Decks))
                    {
                        if (SelectedTags.Any(t => t == "All") || SelectedTags.Any(t => d.Tags.Contains(t)))
                            ListboxPicker.Items.Add(d);
                    }
                }
                else
                {
                    foreach (var d in _selectedClass.Decks)
                    {
                        if (SelectedTags.Any(t => t == "All") || SelectedTags.Any(t => d.Tags.Contains(t)))
                            ListboxPicker.Items.Add(d);
                    }
                }
            }
            else
            {
                _selectedClass = null;
                ListboxPicker.Items.Clear();
                ListboxPicker.Items.Add(new HsClass("All"));
                foreach (var hsClass in _hsClasses)
                {
                    ListboxPicker.Items.Add(hsClass);
                }
                _inClassSelect = true;
            }
        }


    }
}
