using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleChild : MonoBehaviour
{
    public Color EnabledColor = new Color(0.8392157f, 0.7215686f, 0.3921569f, 1);
    public Color DisabledColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, .7f);

    public InputField InputField;
    public Toggle ChildToggle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle(bool enabled)
    {
        InputField.textComponent.color = enabled ? EnabledColor : DisabledColor;
        ChildToggle.interactable = enabled;
    }
}
