using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothOperator : MonoBehaviour
{
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] ProjectSceneManager projectSceneManager;
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
    }
    public void Apply()
    {
        var toggles = toggleGroup.ActiveToggles();
        foreach (Toggle toggle in toggles)
        {
            projectSceneManager.SetScene(additiveScenes[toggle.name].branch, additiveScenes[toggle.name].index);
        }
    }
}
