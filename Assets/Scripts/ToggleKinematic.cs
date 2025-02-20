// Peter Hu, 2025
using UnityEngine;
using TMPro;

public class ToggleKinematic : MonoBehaviour
{
    public GameObject targetObject;
    public TMPro.TextMeshProUGUI buttonText;

    private Rigidbody targetRb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        if(targetObject == null)
        {
            Debug.LogError("Target object is not set.");
            targetRb = GetComponent<Rigidbody>();
        }
        targetRb = targetObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update(){}

    public void ToggleKinematicState()
    {
        targetRb.isKinematic = !targetRb.isKinematic;
        // debug log to show the state of the rigidbody
        Debug.Log("Kinematic state: " + targetRb.isKinematic);

        // change button color and text
        if(targetRb.isKinematic)
        {
            GetComponent<UnityEngine.UI.Button>().image.color = Color.green;
            buttonText.text = "Kinematic";
        }
        else
        {
            GetComponent<UnityEngine.UI.Button>().image.color = Color.red;
            buttonText.text = "Not\n Kinematic";
        }
    }
}
