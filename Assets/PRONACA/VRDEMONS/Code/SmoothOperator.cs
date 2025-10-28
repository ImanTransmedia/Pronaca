using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Operator Role
/// </summary>
public class SmoothOperator : MonoBehaviour
{
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] ProjectSceneManager projectSceneManager;
    [SerializeField] Button btnApply;
    Dictionary<string, scenePair> additiveScenes;
    struct scenePair
    {
        public int branch;
        public int index;
    }
    private void Start()
    {
        additiveScenes = new Dictionary<string, scenePair>();
        additiveScenes.Add("Tgg Tienda", new scenePair { branch = 1, index = 0 });
        additiveScenes.Add("Tgg Panadería", new scenePair { branch = 1, index = 1 });
        additiveScenes.Add("Tgg Carnicería", new scenePair { branch = 1, index = 2 });
        additiveScenes.Add("Tgg Percha 1", new scenePair { branch = 3, index = 0 });
        additiveScenes.Add("Tgg Percha 2", new scenePair { branch = 3, index = 1 });
        foreach(Toggle toggle in toggleGroup.ActiveToggles())
        {
            toggle.onValueChanged.AddListener(OnToggleChange);
        }
    }
    public void Apply()
    {
        if(toggleGroup.AnyTogglesOn())
        {
            projectSceneManager.SetScene(additiveScenes[toggleGroup.GetFirstActiveToggle().name].branch, additiveScenes[toggleGroup.GetFirstActiveToggle().name].index);
        }
    }
    public void OnToggleChange(bool value)
    {
        btnApply.interactable = toggleGroup.AnyTogglesOn();
    }
}
