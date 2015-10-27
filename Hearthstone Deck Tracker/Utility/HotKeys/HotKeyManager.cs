#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.HotKeys
{
	public class HotKeyManager
	{
		private static readonly KeyboardHook KeyboardHook = new KeyboardHook();
		private static readonly Dictionary<HotKey, int> _hotKeyIds = new Dictionary<HotKey, int>(); 
		private static readonly Dictionary<HotKey, Action> _registeredHotKeys = new Dictionary<HotKey, Action>();
		private static readonly ObservableCollection<KeyValuePair<HotKey, string>> _registeredHotKeysInfo = new ObservableCollection<KeyValuePair<HotKey, string>>();
		public static ObservableCollection<KeyValuePair<HotKey, string>> RegisteredHotKeysInfo
		{
			get { return _registeredHotKeysInfo; }
		}

		static HotKeyManager()
		{
			KeyboardHook.KeyPressed += KeyboardHookOnKeyPressed;
		}

		public static bool RegisterHotkey(HotKey hotKey, Action action, string name)
		{
			if(_registeredHotKeys.ContainsKey(hotKey))
			{
				Logger.WriteLine(string.Format("HotKey {0} already registered.", hotKey), "HotKeyManager");
				return false;
			}
			try
			{
				var id = KeyboardHook.RegisterHotKey(hotKey.Mod, hotKey.Key);
				_hotKeyIds.Add(hotKey, id);
				_registeredHotKeys.Add(hotKey, action);

				var predefined = PredefinedHotKeyActions.PredefinedActionNames.FirstOrDefault(x => x.MethodName == name);
				var title = predefined != null ? predefined.Title : name;
				RegisteredHotKeysInfo.Add(new KeyValuePair<HotKey, string>(hotKey, title));
				return true;
			}
			catch(Exception ex)
			{
				Logger.WriteLine(ex.ToString(), "HotKeyManager");
				return false;
			}
		}

		private static void KeyboardHookOnKeyPressed(object sender, KeyPressedEventArgs e)
		{
			Action action;
			if(_registeredHotKeys.TryGetValue(HotKey.FromKeyPressedEventArgs(e), out action))
				action.Invoke();
		}

		public static void Load()
		{
			foreach(var item in HotKeyConfig.Instance.HotKeys)
				LoadPredefinedHotkeyAction(item.HotKey, item.Action);
		}

		public static bool AddPredefinedHotkey(HotKey hotKey, string actionName)
		{
			if(LoadPredefinedHotkeyAction(hotKey, actionName))
			{
				HotKeyConfig.Instance.AddHotKey(hotKey, actionName);
				return true;
			}
			return false;
		}

		public static void RemovePredefinedHotkey(HotKey hotKey)
		{
			HotKeyConfig.Instance.RemoveHotKey(hotKey);
			if(_registeredHotKeys.ContainsKey(hotKey))
				_registeredHotKeys.Remove(hotKey);
			var info = RegisteredHotKeysInfo.FirstOrDefault(x => x.Key.Equals(hotKey));
			if(!info.Equals(default(KeyValuePair<HotKey, string>)))
				RegisteredHotKeysInfo.Remove(info);
			try
			{
				KeyboardHook.UnRegisterHotKey(_hotKeyIds[hotKey]);
				_hotKeyIds.Remove(hotKey);
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error removing hotkey: " + ex, "HotKeyManager");
			}
		}

		private static bool LoadPredefinedHotkeyAction(HotKey hotKey, string actionName)
		{
			var action = typeof(PredefinedHotKeyActions).GetMethods().FirstOrDefault(x => x.Name == actionName);
			if(action != null)
				return RegisterHotkey(hotKey, () => action.Invoke(null, null), actionName);
			Logger.WriteLine(string.Format("Could not find predefined action \"{0}\"", actionName), "HotKeyManager");
			HotKeyConfig.Instance.RemoveHotKey(hotKey);
			return false;
		}
	}
}