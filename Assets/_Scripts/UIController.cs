using NBodyUniverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Dropdown systemTypeDropdown;
    public Button generateButton;
    public Toggle statusToggle;
    public Slider zoomSlider;
    private SystemType defaultSystemType = SystemType.Stock;
    private Camera mainCamera;

    public SystemType SystemType
    {
        get
        {
            return (SystemType)systemTypeDropdown.value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main.GetComponent<Camera>();
        systemTypeDropdown.ClearOptions();
        var typeOptions = Enum.GetNames(typeof(SystemType)).Cast<String>().ToList();
        systemTypeDropdown.AddOptions(typeOptions);
        systemTypeDropdown.value = (int)defaultSystemType;

        // wire up statustoggle enabling
        generateButton.onClick.AddListener(GenerateButtonOnClick);

        statusToggle.interactable = false;

        zoomSlider.onValueChanged.AddListener(delegate { HandleZoomChange(); });
    }

    // Update is called once per frame
    void Update()
    {
        generateButton.interactable = systemTypeDropdown.interactable = !statusToggle.isOn;
        
    }

    public void GenerateButtonOnClick()
    {
        statusToggle.interactable = true;
    }

    public void HandleZoomChange()
    {
        mainCamera.orthographicSize = zoomSlider.value;
    }
}
