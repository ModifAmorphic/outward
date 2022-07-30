using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Services
{
    internal class CooldownService : IDisposable
    {
        private readonly ActionSlotController _controller;

        private ICooldown _cooldown;

        private Coroutine _coroutine;
        private bool _coroutineStarted = false;
        private bool _hideNeeded;

        private readonly Image _image;
        private readonly Text _text;
        private bool _showTime;
        private bool _isPreciseTime;
        private bool disposedValue;
        private string _timeFormat;

        public CooldownService(Image cooldownImage, Text cooldownText, bool showTime, bool isPreciseTime, ActionSlotController controller)
        {
            _image = cooldownImage;
            _text = cooldownText;
            _showTime = showTime;
            _isPreciseTime = isPreciseTime;
            _controller = controller;
            UpdateTimeFormat();
        }
        public void Configure(bool showTime, bool isPreciseTime)
        {
            (_showTime, _isPreciseTime) = (showTime, isPreciseTime);
            UpdateTimeFormat();
        }

        public void TrackCooldown(ICooldown cooldown)
        {
            _cooldown = cooldown;

            if (_coroutine == null || !_coroutineStarted)
            {
                _coroutine = _controller.ActionSlot.StartCoroutine(DisplayCooldown());
                _coroutineStarted = true;
            }
        }
        public void StopTracking()
        {
            if (_coroutine != null)
            {
                _controller.ActionSlot?.StopCoroutine(_coroutine);
                _coroutineStarted = false;
                //_coroutine = null;
                _controller.HideCooldown();
            }
        }
        private void UpdateTimeFormat()
        {
            _timeFormat = _isPreciseTime ? "0.0" : "0";
        }
        private IEnumerator DisplayCooldown()
        {
            while (true)
            {
                if (_cooldown.GetIsInCooldown())
                {
                    _image.fillAmount = Mathf.Clamp01(_cooldown.GetProgress());
                    if (_showTime && _cooldown.GetSecondsRemaining() > 0f)
                        _text.text = _cooldown.GetSecondsRemaining().ToString(_timeFormat);
                    _hideNeeded = true;
                }
                else if (_hideNeeded)
                {
                    _controller.HideCooldown();
                    _hideNeeded = false;
                }
                yield return new WaitForSeconds(Timings.CooldownWait);
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CooldownService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
