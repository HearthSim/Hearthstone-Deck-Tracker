using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPFLocalizeExtension.Engine;

namespace HDTTests.Utility.MVVM
{
	[TestClass]
	public class LocalizedPropTests
	{
		private class TestViewModel : ViewModel
		{
			[LocalizedProp]
			public string LocalizedText => "localized";

			public string PlainText => "plain";
		}

		private class TestControl : INotifyPropertyChanged
		{
			private readonly LocalizedPropNotifier _localizedPropNotifier;

			public TestControl()
			{
				_localizedPropNotifier = new LocalizedPropNotifier(GetType(), OnPropertyChanged);
			}

			[LocalizedProp]
			public string LocalizedText => "localized";

			public event PropertyChangedEventHandler PropertyChanged;

			private void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private static List<string> RecordPropertiesChangedOnLanguageChange(INotifyPropertyChanged target)
		{
			var changed = new List<string>();
			target.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

			var culture = LocalizeDictionary.Instance.Culture;
			try
			{
				LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(culture.Name == "de-DE" ? "en-US" : "de-DE");
			}
			finally
			{
				LocalizeDictionary.Instance.Culture = culture;
			}

			return changed;
		}

		[TestMethod]
		public void ViewModelNotifiesLocalizedPropsWhenTheLanguageChanges()
		{
			var changed = RecordPropertiesChangedOnLanguageChange(new TestViewModel());

			CollectionAssert.Contains(changed, nameof(TestViewModel.LocalizedText));
			CollectionAssert.DoesNotContain(changed, nameof(TestViewModel.PlainText));
		}

		[TestMethod]
		public void ControlNotifiesLocalizedPropsWhenTheLanguageChanges()
		{
			var changed = RecordPropertiesChangedOnLanguageChange(new TestControl());

			CollectionAssert.Contains(changed, nameof(TestControl.LocalizedText));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference CreateDiscardedViewModel() => new WeakReference(new TestViewModel());

		[TestMethod]
		public void DiscardedViewModelsAreCollected()
		{
			var discarded = CreateDiscardedViewModel();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.IsFalse(discarded.IsAlive);
		}
	}
}
