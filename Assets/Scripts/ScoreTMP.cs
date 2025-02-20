// Peter Hu, 2025
using UnityEngine;
using TMPro;


public class ScoreTMP : MonoBehaviour
{
    private ARRaycastPlace arRaycastPlace;
    public TextMeshProUGUI placedObjectsText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the ARRaycastPlace script in the scene
        arRaycastPlace = FindFirstObjectByType<ARRaycastPlace>();  
    }

    // Update is called once per frame
    void Update()
    {
        // update text to show the number of placed objects from ShowFiredSpheresNum
        if (arRaycastPlace != null)
        {
            placedObjectsText.text = "Score: " + arRaycastPlace.ShowPlacedObjectsNum() +
            "-" + arRaycastPlace.ShowFiredSpheresNum();
        }
    }
}
