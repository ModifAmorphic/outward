using ModifAmorphic.Outward.Unity.ActionUI.Controllers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionUI.Services
{
    internal class StackService : IDisposable
    {
        private readonly ActionSlotController _controller;
        private Func<int> _getAmount;

        private Coroutine _coroutine;
        private bool _coroutineStarted = false;
        private bool _hideNeeded;
        private bool _showWhenZero;

        private readonly Text _text;

        private bool disposedValue;

        public StackService(Text stackText, bool showWhenZero, ActionSlotController controller)
        {
            _text = stackText;
            _showWhenZero = showWhenZero;
            _controller = controller;
        }

        public void Configure(bool showWhenZero) => _showWhenZero = showWhenZero;

        public void TrackStackAmount(Func<int> getStackAmount)
        {
            _getAmount = getStackAmount;

            if (_coroutine == null || !_coroutineStarted)
            {
                _coroutine = _controller.ActionSlot.StartCoroutine(DisplayStackAmount());
                _coroutineStarted = true;
            }
        }
        public void StopTracking()
        {
            if (_coroutine != null)
            {
                _controller.ActionSlot.StopCoroutine(_coroutine);
                _controller.HideStackAmount();
            }
            _coroutineStarted = false;
        }
        private IEnumerator DisplayStackAmount()
        {
            while (true)
            {
                var remaining = _getAmount();
                if (remaining > 0 || _showWhenZero)
                {
                    _hideNeeded = true;
                    _text.text = remaining.ToString();
                }
                else if (_hideNeeded)
                {
                    _controller.HideStackAmount();
                    _hideNeeded = false;
                }

                yield return new WaitForSeconds(Timings.StackDelay);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _controller?.ActionSlot?.StopCoroutine(_coroutine);
                }

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
