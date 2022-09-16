using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System;
using System.Collections;
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
            int timeCeiling;
            while (true)
            {
                if (_cooldown.GetIsInCooldown())
                {
                    _image.fillAmount = Mathf.Clamp01(_cooldown.GetProgress());
                    timeCeiling = Mathf.CeilToInt(_cooldown.GetSecondsRemaining());
                    if (_showTime && (timeCeiling > 0 || _isPreciseTime))
                    {
                        if (_isPreciseTime && _cooldown.GetSecondsRemaining() <= 9.9f)
                            _text.text = _cooldown.GetSecondsRemaining().ToString(_timeFormat);
                        else
                            _text.text = timeCeiling.ToString();

                        if (_controller.ActionSlot.CooldownTextBackground.color.a != .9f && !string.IsNullOrWhiteSpace(_text.text))
                        {
                            var bgColor = _controller.ActionSlot.CooldownTextBackground.color;
                            bgColor.a = .8f;
                            _controller.ActionSlot.CooldownTextBackground.color = bgColor;
                        }
                    }

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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
