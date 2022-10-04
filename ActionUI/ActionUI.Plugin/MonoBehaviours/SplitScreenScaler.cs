using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Monobehaviours
{
    public class SplitScreenScaler : MonoBehaviour
    {
        public CharacterUI CharacterUI { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            SplitScreenManager.Instance.onSplitScreenRefresh += Scale;
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        //private void Update()
        //{

        //    Scale();
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            Scale();
            InvokeRepeating(nameof(Scale), 1.0f, 1.0f);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void OnDisable()
        {
            CancelInvoke(nameof(Scale));
            SplitScreenManager.Instance.onSplitScreenRefresh -= Scale;
        }

        private void Scale()
        {
            if (CharacterUI == null || !gameObject.activeSelf)
                return;

            var rectTransform = GetComponent<RectTransform>();
            var rect = rectTransform.rect;
            var charRectTransform = CharacterUI.transform as RectTransform;
            var charRect = charRectTransform.rect;

            if (charRect.width >= rect.width && charRect.height >= rect.height)
            {
                if (rectTransform.localScale != Vector3.one)
                    rectTransform.localScale = Vector3.one;
                return;
            }

            float xScale = charRect.width / rect.width;
            float yScale = charRect.height / rect.height;
            float xyScale = (xScale <= yScale ? xScale : yScale) - .05f;

            rectTransform.localScale = new Vector3(xyScale, xyScale);
        }
    }
}