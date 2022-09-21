using ModifAmorphic.Outward.Unity.ActionMenus;
using ModifAmorphic.Outward.Unity.ActionUI;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Testing
{
    internal class RandomStack : IStackable
    {
        private bool _isStackable;
        public bool IsStackable => _isStackable;
        static System.Random random = new System.Random();

        private int _stackAmount;
        public RandomStack(MonoBehaviour parent)
        {
            _stackAmount = random.Next(1, 99);
            var stackable = random.Next(1, 100);
            _isStackable = stackable > 50;
            if (_isStackable)
                parent.StartCoroutine(DecreaseStack());
        }

        public int GetAmount() => _stackAmount;

        private IEnumerator DecreaseStack()
        {
            while (_stackAmount > 0)
            {
                yield return new WaitForSeconds(5);
                _stackAmount--;
            }
            yield return null;
        }
    }
}
