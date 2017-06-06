#region

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckScreenshot
{
	public class DeckScreenshotViewModel : ViewModel
	{
		private const string ClipboardDefault = "DeckScreenshot_Button_Clipboard_Copy";
		private const string ClipboardCopied = "DeckScreenshot_Button_Clipboard_Copied";
		private const string ImgurDefault = "DeckScreenshot_Button_Imgur_Upload";
		private const string ImgurUploading = "DeckScreenshot_Button_Imgur_Uploading";
		private const string ImgurUploaded = "DeckScreenshot_Button_Imgur_Success";

		private bool _cardsOnly;
		private string _copyToClipboardButtonText = LocUtil.Get(ClipboardDefault, true);
		private Deck _deck;
		private BitmapSource _deckImage;
		private string _imgurUrl;
		private FileInfo _savedFile;
		private bool _uploadButtonEnabled = true;
		private string _uploadButtonText = LocUtil.Get(ImgurDefault, true);
		private Visibility _uploadErrorVisibility = Visibility.Collapsed;

		public bool CardsOnly
		{
			get { return _cardsOnly; }
			set
			{
				_cardsOnly = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(TitleTextBoxVisibility));
				UpdateImage();
			}
		}

		public string DeckTitle
		{
			get { return _deck?.Name; }
			set
			{
				_deck.Name = value;
				OnPropertyChanged();
				UpdateImage();
			}
		}

		public BitmapSource DeckImage
		{
			get { return _deckImage; }
			set
			{
				_deckImage = value;
				OnPropertyChanged();
			}
		}

		private FileInfo SavedFile
		{
			get { return _savedFile; }
			set
			{
				_savedFile = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(SavedFilePath));
				OnPropertyChanged(nameof(SavedFileShortName));
				OnPropertyChanged(nameof(SavedFolderPath));
				OnPropertyChanged(nameof(SavedFileVisibility));
			}
		}

		public Deck Deck
		{
			get { return _deck; }
			set
			{
				if(value == null)
					_deck = null;
				else
					_deck = (Deck)value.GetSelectedDeckVersion().Clone();
				OnPropertyChanged(nameof(DeckTitle));
				ImgurUrl = null;
				SavedFile = null;
				UploadButtonText = LocUtil.Get(ImgurDefault, true);
				UploadButtonEnabled = true;
				UploadErrorVisibility = Visibility.Collapsed;
				UpdateImage();
			}
		}

		public string UploadButtonText
		{
			get { return _uploadButtonText; }
			set
			{
				_uploadButtonText = value;
				OnPropertyChanged();
			}
		}

		public string CopyToClipboardButtonText
		{
			get { return _copyToClipboardButtonText; }
			set
			{
				_copyToClipboardButtonText = value;
				OnPropertyChanged();
			}
		}

		public Visibility UploadErrorVisibility
		{
			get { return _uploadErrorVisibility; }
			set
			{
				_uploadErrorVisibility = value;
				OnPropertyChanged();
			}
		}

		public string ImgurUrl
		{
			get { return _imgurUrl; }
			set
			{
				_imgurUrl = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ImgurUrlVisibility));
			}
		}

		public bool UploadButtonEnabled
		{
			get { return _uploadButtonEnabled; }
			set
			{
				_uploadButtonEnabled = value;
				OnPropertyChanged();
			}
		}

		public ICommand SaveCommand => new Command(Save);

		public ICommand UploadCommand => new Command(Upload);

		public ICommand CopyToClipboardCommand => new Command(CopyToClipboard);

		public Visibility ImgurUrlVisibility => ImgurUrl == null ? Visibility.Collapsed : Visibility.Visible;

		public Visibility SavedFileVisibility => SavedFilePath == null ? Visibility.Collapsed : Visibility.Visible;

		public Visibility TitleTextBoxVisibility => CardsOnly ? Visibility.Collapsed : Visibility.Visible;

		public string SavedFilePath => SavedFile?.FullName;

		public string SavedFileShortName => SavedFile?.Name;

		public string SavedFolderPath => SavedFile?.Directory?.FullName;

		public void Save()
		{
			var file = DeckScreenshotHelper.Save(Deck, DeckImage);
			if(file != null)
				SavedFile = new FileInfo(file);
		}

		private async void Upload()
		{
			UploadButtonEnabled = false;
			UploadButtonText = LocUtil.Get(ImgurUploading, true);
			var url = await DeckScreenshotHelper.Upload(DeckImage);
			if(url == null)
			{
				UploadErrorVisibility = Visibility.Visible;
				UploadButtonEnabled = true;
				UploadButtonText = LocUtil.Get(ImgurDefault, true);
			}
			else
			{
				ImgurUrl = url;
				UploadButtonText = LocUtil.Get(ImgurUploaded, true);
			}
		}

		public async void CopyToClipboard()
		{
			var success = DeckScreenshotHelper.CopyToClipboard(DeckImage);
			if(!success)
				return;
			CopyToClipboardButtonText = LocUtil.Get(ClipboardCopied, true);
			await Task.Delay(2000);
			CopyToClipboardButtonText = LocUtil.Get(ClipboardDefault, true);
		}

		public void UpdateImage()
		{
			if(_deck == null)
				return;
			DeckImage = DeckScreenshotHelper.Generate(_deck, CardsOnly);
		}
	}
}
