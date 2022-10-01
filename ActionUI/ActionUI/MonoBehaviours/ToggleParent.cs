using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleParent : MonoBehaviour
{
    public List<ToggleChild> Children;

    private void Awake() => GetComponent<Toggle>().onValueChanged.AddListener(ToggleChildren);

    private void Start() => ToggleChildren(GetComponent<Toggle>().isOn);
    public void ToggleChildren(bool enabled)
    {
        if (Children == null)
            return;

        foreach (var child in Children)
            child.Toggle(enabled);
    }
}
