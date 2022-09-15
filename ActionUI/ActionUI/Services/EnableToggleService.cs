using ModifAmorphic.Outward.Unity.ActionMenus.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Services
{
    internal class EnableToggleService : IDisposable
    {
        private readonly ActionSlotController _controller;
        private Func<bool> _getEnabled;

        private Coroutine _coroutine;
        private bool _coroutineStarted = false;

        private bool disposedValue;

        public EnableToggleService(ActionSlotController controller) => _controller = controller;

        public void TrackEnableToggle(Func<bool> getEnabled)
        {
            _getEnabled = getEnabled;

            if (_coroutine == null || !_coroutineStarted)
            {
                _coroutine = _controller.ActionSlot.StartCoroutine(ToggleActionSlot());
                _coroutineStarted = true;
            }
        }
        public void StopTracking()
        {
            if (_coroutine != null)
            {
                _controller.ActionSlot.StopCoroutine(_coroutine);
            }
        }
        private IEnumerator ToggleActionSlot()
        {
            while (true)
            {
                if (_getEnabled() || _controller.ActionSlot.IsInEditMode)
                    _controller.ToggleEnabled(true);
                else
                    _controller.ToggleEnabled(false);

                yield return new WaitForSeconds(Timings.EnabledWait);
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
