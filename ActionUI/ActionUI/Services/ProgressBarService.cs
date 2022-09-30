using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI.Controllers;
using System;
using System.Collections;
using UnityEngine;

namespace ModifAmorphic.Outward.Unity.ActionUI.Services
{
    internal class ProgressBarService : IDisposable
    {
        private readonly ActionSlotController _controller;

        private Coroutine _coroutine;
        private bool _coroutineStarted = false;

        private readonly ProgressBar _bar;
        private readonly BarPositions _barPosition;
        private IBarProgress _barProgress;
        private bool disposedValue;

        public ProgressBarService(ProgressBar bar, BarPositions barPosition, ActionSlotController controller)
        {
            _bar = bar;
            _barPosition = barPosition;
            _controller = controller;
        }

        public void TrackSlider(IBarProgress sliderProgress)
        {
            _barProgress = sliderProgress;

            _bar.ResetColorRanges();
            if (_barProgress.ColorRanges != null)
            {
                foreach (var range in _barProgress.ColorRanges)
                    _bar.AddColorRange(range);
            }
            _controller.ShowSlider(_barPosition);

            if (_coroutine == null || !_coroutineStarted)
            {
                _coroutine = _controller.ActionSlot.StartCoroutine(DisplayProgress());
                _coroutineStarted = true;
            }
        }
        public void StopTracking()
        {
            if (_coroutine != null)
            {
                _controller.ActionSlot?.StopCoroutine(_coroutine);
                //_coroutine = null;
                _controller.HideSlider(_barPosition);
            }
            _coroutineStarted = false;
        }
        private IEnumerator DisplayProgress()
        {
            while (true)
            {
                _bar.SetValue(_barProgress.GetProgress());
                yield return new WaitForSeconds(Timings.ProgressBarWait);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopTracking();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
