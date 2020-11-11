using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hairibar.Ragdoll.Demo
{
    public class DemoSceneSwicther : MonoBehaviour
    {
        Dropdown dropdown;

        void Start()
        {
            GetComponentInChildren<Button>().onClick.AddListener(OnLoadSceneClicked);
            dropdown = GetComponentInChildren<Dropdown>();

            dropdown.SetValueWithoutNotify(SceneManager.GetActiveScene().buildIndex);
        }

        void OnLoadSceneClicked()
        {
            int dropdownValue = dropdown.value;
            SceneManager.LoadScene(dropdownValue);
        }
    }
}