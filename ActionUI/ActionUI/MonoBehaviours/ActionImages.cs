using ModifAmorphic.Outward.Unity.ActionUI;
using ModifAmorphic.Outward.Unity.ActionUI.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionImages : MonoBehaviour
{
    //public Color EnabledColor = new Color(0.8392157f, 0.7215686f, 0.3921569f, 1);
    public static readonly Color DisabledColor = new Color(0f, 0f, 0f, .9f);

    public Image BaseImage;
    private Dictionary<string, Image> _bottomImages = new Dictionary<string, Image>();
    private Dictionary<string, Image> _topImages = new Dictionary<string, Image>();

    private const string DisabledKey = "disabled";

    public Image AddOrUpdateImage(ActionSlotIcon slotIcon)
    {
        if (_bottomImages.TryGetValue(slotIcon.Name, out var image))
        {
            if (image.sprite != slotIcon.Icon)
                image.sprite = slotIcon.Icon;

            return image;
        }

        var newImage = Instantiate(BaseImage, transform);
        newImage.sprite = slotIcon.Icon;
        newImage.overrideSprite = null;
        newImage.gameObject.SetActive(true);
        newImage.name = slotIcon.Name;
        if (!slotIcon.IsTopSprite)
            _bottomImages.Add(slotIcon.Name, newImage);
        else
            _topImages.Add(slotIcon.Name, newImage);

        SetSiblingIndexes();

        return newImage;
    }
    public void ToggleEnabled(bool enabled)
    {
        if (enabled && _bottomImages.TryGetValue(DisabledKey, out var disabledImage))
        {
            Destroy(disabledImage.gameObject);
            _bottomImages.Remove(DisabledKey);
        }
        else if (!enabled && !_bottomImages.ContainsKey(DisabledKey))
        {
            disabledImage = AddOrUpdateImage(new ActionSlotIcon() { Name = DisabledKey, Icon = null });
            disabledImage.color = DisabledColor;
        }
    }

    public void ClearImages()
    {
        foreach (var image in _bottomImages.Values)
        {
            if (image?.gameObject != null)
                image.gameObject.Destroy();
        }
        foreach (var image in _topImages.Values)
        {
            if (image?.gameObject != null)
                image.gameObject.Destroy();
        }

        _bottomImages.Clear();
        _topImages.Clear();
    }

    private void SetSiblingIndexes()
    {
        int siblingIndex = 0;
        foreach (var image in _bottomImages.Values)
        {
            image.transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;
        }

        foreach (var image in _topImages.Values)
        {
            image.transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;
        }
    }
}
