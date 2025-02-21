using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Utility.Overlay
{
    public class OverlayOpacityMask : INotifyPropertyChanged
    {
        private DrawingBrush? _mask;
        public DrawingBrush? Mask
        {
            get => _mask;
            set
            {
                if (_mask != value)
                {
                    _mask = value;
                    OnPropertyChanged(nameof(Mask));
                }
            }
        }

        // The masked regions that will be rendered as transparent, grouped by origin key
        private readonly Dictionary<string, List<Rect>> _maskedRegions = new Dictionary<string, List<Rect>>();

        public event Action? Changed;

        private int _batchChanges;
        private bool _batchInProgress;

        /// <summary>
        /// Suppress mask updates and change events until disposed (by default at the end of the calling function).
        /// </summary>
        public Deferred StartBatchUpdate()
        {
	        _batchInProgress = true;
	        return new Deferred(() =>
	        {
				_batchInProgress = false;
				if(_batchChanges > 0)
				{
					_batchChanges = 0;
					CreateOpacityMask();
					Changed?.Invoke();
				}
	        });
        }

        public void AddMaskedRegion(string key, Rect region)
        {
	        if (!_maskedRegions.ContainsKey(key))
	        {
		        _maskedRegions[key] = new List<Rect>();
	        }
	        _maskedRegions[key].Add(region);

	        if(!_batchInProgress)
	        {
		        CreateOpacityMask();
		        Changed?.Invoke();
	        }
	        else
		        _batchChanges++;
        }

        public void RemoveMaskedRegion(string key)
        {
	        var removed = _maskedRegions.Remove(key);
	        if(!removed)
		        return;

	        if(!_batchInProgress)
	        {
				CreateOpacityMask();
				Changed?.Invoke();
	        }
	        else
		        _batchChanges++;
        }

        private bool _disabled;
        public void Disable()
        {
	        _disabled = true;
	        Mask = null;
			CreateOpacityMask();
			Changed?.Invoke();
        }

        public void Enable()
        {
	        _disabled = false;
	        CreateOpacityMask();
			Changed?.Invoke();
        }

        private void CreateOpacityMask()
        {
	        if(_disabled)
		        return;
	        var fullAreaGeometry = new RectangleGeometry(new Rect(0, 0, 1, 1));
	        Geometry combinedGeometry = fullAreaGeometry;

	        foreach (var regions in _maskedRegions.Values)
	        {
		        foreach (var region in regions)
		        {
			        var regionGeometry = new RectangleGeometry(region);
			        combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, combinedGeometry, regionGeometry);
		        }
	        }

	        var geometryDrawing = new GeometryDrawing
	        {
		        Brush = Brushes.Black,
		        Geometry = combinedGeometry
	        };

	        Mask = new DrawingBrush
	        {
		        Drawing = geometryDrawing,
	        };
        }

        // Property change handler for data binding or UI update
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
