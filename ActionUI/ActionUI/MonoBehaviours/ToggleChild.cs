using UnityEngine;
using UnityEngine.UI;

public class ToggleChild : MonoBehaviour
{
    public Color EnabledColor = new Color(0.8392157f, 0.7215686f, 0.3921569f, 1);
    public Color DisabledColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, .7f);

    public InputField InputField;
    public Selectable ChildToggle;
    public Selectable[] ChildToggles;

    public void Toggle(bool enabled)
    {
        InputField.textComponent.color = enabled ? EnabledColor : DisabledColor;

        if (ChildToggles != null)
            for (int i = 0; i < ChildToggles.Length; i++)
                ChildToggles[i].interactable = enabled;
    }

}
