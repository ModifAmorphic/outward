using ModifAmorphic.Outward.UnityScripts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts.Services
{
    public class CharacterUIProxy
    {
        private readonly Func<Logging.Logger> _loggerFactory;
        private Logging.Logger Logger => _loggerFactory.Invoke();

        private static MethodInfo _showInfoNotification;

        public CharacterUIProxy(Func<Logging.Logger> loggerFactory) => _loggerFactory = loggerFactory;


        public static void ShowInfoNotification(MonoBehaviour characterUI, string message)
        {
            if (_showInfoNotification == null)
                _showInfoNotification = OutwardAssembly.Types.CharacterUI.GetMethod("ShowInfoNotification", BindingFlags.Public | BindingFlags.Instance, null, new Type[1] { typeof(string) }, null);

            _showInfoNotification.Invoke(characterUI, new object[1] { message });
        }
    }
}
