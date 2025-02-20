// Peter Hu, 2025
// reference: https://learn.unity.com/tutorial/placing-and-manipulating-objects-in-ar#605103a5edbc2a6c32bf5663
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
// List<>
using System.Collections.Generic;
// Path
using System.IO;


[System.Serializable]
public class TransformData
{
    public string objectName;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float scaleX, scaleY, scaleZ;

    public TransformData(Transform transform)
    {
        objectName = transform.name;
        posX = transform.position.x;
        posY = transform.position.y;
        posZ = transform.position.z;

        rotX = transform.rotation.x;
        rotY = transform.rotation.y;
        rotZ = transform.rotation.z;
        rotW = transform.rotation.w;

        scaleX = transform.localScale.x;
        scaleY = transform.localScale.y;
        scaleZ = transform.localScale.z;
    }
}

[System.Serializable]
public class TransformDataList
{
    public List<TransformData> transforms = new List<TransformData>();
}





public class ARRaycastPlace : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject objectToPlace;

    public Camera arCamera;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Store placed objects
    private List<GameObject> placedObjects = new List<GameObject>();
    private List<GameObject> firedSpheres = new List<GameObject>();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        Debug.Log(Application.persistentDataPath);
    }
  
   void Update()
    {
        // check not at UI button
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(ray, hits, TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;
                Vector3 stackPosition = hitPose.position;

                GameObject closestObject = null;

                // default set to the distance to the hit point on the plane
                float closestDistance = Vector3.Distance(ray.origin, hitPose.position);
                Debug.Log("Distance to plane: " + closestDistance);

                // int placeableLayer = LayerMask.NameToLayer("PlaceableObject");
                
                // Find the closest object to the camera along the ray
                foreach (GameObject obj in placedObjects)
                {
                    // continue; // skip all the below code

                    Vector3 objScreenPos = arCamera.WorldToScreenPoint(obj.transform.position);

                    // Check if object is in front of the camera
                    if(objScreenPos.z <= 0)
                    {
                        Debug.Log("Object is behind the camera!");
                        continue;
                    }
                 
                    Ray objRay = arCamera.ScreenPointToRay(objScreenPos);
                    // Check if the object is in the same general direction

                    if (Vector3.Dot(ray.direction, objRay.direction) <= 0) 
                    {
                        Debug.Log("Object is not in the same direction!");
                        continue;
                    }

                    RaycastHit hit2;
                    bool hitResult = Physics.Raycast(ray, out hit2, Mathf.Infinity);
                    //1 << placeableLayer // layer mask
                    
                    if (!hitResult)  
                    {
                        Debug.Log("Ray does not hit anything!");
                        continue;
                    }

                    if(hit2.collider.gameObject != obj){
                        Debug.Log("Ray hit: " + hit2.collider.gameObject.name);
                        continue;
                    }

                    // float distance = hit2.distance;
                    float distance = Vector3.Distance(ray.origin, obj.transform.position);
                    Debug.Log("Distance to object: " + distance);

                    if (distance < closestDistance) 
                    {
                        // Update the closest distance and object
                        closestDistance = distance;
                        closestObject = obj;
                    }
                }

                if (closestObject != null)
                {
                    // Stack on the closest object
                    Debug.Log("Updated based on object!");

                    float objectHeight = closestObject.GetComponent<Renderer>().bounds.size.y;
                    stackPosition = closestObject.transform.position;
                    stackPosition.y += objectHeight;
                    hitPose.rotation = closestObject.transform.rotation; // Match rotation
                }
                else{
                    // Stack on the plane
                    Debug.Log("Updated based on plane!");
                    stackPosition.y += 0.08f;
                }
                // Clone objectToPlace and place it at stackPosition
                GameObject newObject = Instantiate(objectToPlace, stackPosition, hitPose.rotation);
                placedObjects.Add(newObject);
            }
        }
    }

    public void SwitchObjects(){
        ChangeObjectTypes();
        UpdateScale();
        UpdateMat();   
    }

    public void UpdateMat(){
        // for each object in placedObjects assign color 
        int total_objects = placedObjects.Count;

        for (int counter = 0; counter < placedObjects.Count; counter++)
        {
            GameObject obj = placedObjects[counter];
            Renderer rend = obj.GetComponent<Renderer>();

            // pick a random number between 0 and 1
            float randomValue = Random.value;
            if (randomValue < 0.3)
            {
                /* blending from original color to white */
                
                Color originalColor = rend.material.color;
                // blend the color to white
                float coefficient = 1.0f - counter / total_objects;
                coefficient = randomValue;
                rend.material.color = Color.Lerp(originalColor, Color.white, coefficient);
            }
            else
            {
                /* random color */
                rend.material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
            }
        }
    }

    public void UpdateScale(){

        foreach (GameObject obj in placedObjects)
        {
            float randomScale = Random.Range(0.5f, 1.5f);
            // times the original scale
            obj.transform.localScale = obj.transform.localScale * randomScale;

            randomScale = Random.Range(0.03f, 0.3f);
            obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
    }

    public void ChangeObjectTypes(){

        PrimitiveType objPrimitiveType = PrimitiveType.Cube;
        // random value between 0 and 1
        float randomValue = Random.value;
        if (randomValue < 0.2)
        {
            objPrimitiveType = PrimitiveType.Cube;
        }
        else if (randomValue < 0.4)
        {
            objPrimitiveType = PrimitiveType.Capsule;
        }
        else if (randomValue < 0.6)
        {
            objPrimitiveType = PrimitiveType.Cylinder;
        }
        else if (randomValue < 0.8)
        {
            objPrimitiveType = PrimitiveType.Sphere;
        }
        else
        {
            objPrimitiveType = PrimitiveType.Quad;
        }

        for (int counter = 0; counter < placedObjects.Count; counter++)
        {
            GameObject obj = placedObjects[counter];
         
            // Store the object's transform properties
            Vector3 position = obj.transform.position;
            Quaternion rotation = obj.transform.rotation;
            Vector3 scale = obj.transform.localScale;

            // Destroy the current object
            Destroy(obj);

            // Create a new sphere at the same position, rotation, and scale
            GameObject newObj = GameObject.CreatePrimitive(objPrimitiveType);
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
            newObj.transform.localScale = scale;
            // set material shader to URP/Lit
            newObj.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
            // material
            Renderer rend = newObj.GetComponent<Renderer>();
            // 8B0000
            rend.material.color = new Color(0.5f, 0.0f, 0.0f, 1.0f);

            // Replace the old object in the list
            placedObjects[counter] = newObj;
        }
    }

    public static List<string> GetSavedFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        List<string> files = new List<string>();
        foreach (string filePath in Directory.GetFiles(directoryPath, "*.json"))
        {
            files.Add(Path.GetFileNameWithoutExtension(filePath));
        }

        return files;
    }

    // return the number of saved files
    int ShowSavedFiles()
    {
        List<string> savedObjects = GetSavedFiles(  Application.persistentDataPath);
        Debug.Log("Saved Objects: " + string.Join(", ", savedObjects));
        return savedObjects.Count;
    }

    public void saveObjects(){
        // save the objects to a file
        /* format:
        objectName
        position.x position.y position.z
        rotation.x rotation.y rotation.z rotation.w
        scale.x scale.y scale.z
        */
        int saved_files_num = ShowSavedFiles();
        Debug.Log("Number of saved files: " + saved_files_num);

        string filename = "PlacedObjects" + saved_files_num + ".json";
        string filepath = Path.Combine(Application.persistentDataPath, filename);

        // check if the file exists, if not create it
        if (!File.Exists(filepath))
        {
            Debug.Log("File not found, creating file at " + filepath);
            File.Create(filepath).Close();
        }else{
            Debug.Log("File found at " + filepath);
        }

        // System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath);

        TransformDataList dataList = new TransformDataList();

        foreach (GameObject obj in placedObjects)
        {
            // writer.WriteLine(obj.name);
            // writer.WriteLine(obj.transform.position.x + " " + obj.transform.position.y + " " + obj.transform.position.z);
            // writer.WriteLine(obj.transform.rotation.x + " " + obj.transform.rotation.y + " " + obj.transform.rotation.z + " " + obj.transform.rotation.w);
            // writer.WriteLine(obj.transform.localScale.x + " " + obj.transform.localScale.y + " " + obj.transform.localScale.z);

            TransformData data = new TransformData(obj.transform);
            dataList.transforms.Add(data);
            
        }
        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(filepath, json);

        Debug.Log("Saving objects done!");
    }

    // paramter: List<GameObject> objList
    private void clearObjLists( List<GameObject> objList){
        // Destroy all the objects in the list
        // copy the objList
        List<GameObject> objListCopy = new List<GameObject>(objList);
        int objListSize = objListCopy.Count;
        Debug.Log("Clearing objects with " + objListSize + " objects");
        objListCopy.ForEach(DestroyImmediate);
        objListCopy.Clear();
    }

    public void clearCurrentObjects(){
        // clear all the objects
          
        // foreach (GameObject obj in placedObjects)
        // {
        //     Destroy(obj);
        // }

        clearObjLists(placedObjects);
        clearObjLists(firedSpheres);
      
        placedObjects.Clear();
        firedSpheres.Clear();
        Debug.Log("Clearing objects done!");

    }

    public void clearObjects(){
        // clearCurrentObjects();
        // remove all files
        List<string> savedObjects = GetSavedFiles(Application.persistentDataPath);
        foreach (string filename in savedObjects)
        {
            string filepath = Path.Combine(Application.persistentDataPath, filename + ".json");
            File.Delete(filepath);
        }
    
    }


    public void loadObjects(){
        int saved_files_num = ShowSavedFiles();
        Debug.Log("Number of saved files: " + saved_files_num);

        // pick a random file to load, from 0, ... saved_files_num-1
        int randomIndex = Random.Range(0, saved_files_num);
        string filename = "PlacedObjects" + randomIndex + ".json";

        string filepath = Path.Combine(Application.persistentDataPath, filename);

        if (!File.Exists(filepath))
        {
            Debug.Log("File not found at " + filepath);
            return;
        }else{
            Debug.Log("File found at " + filepath);
        }

        // System.IO.StreamReader reader = new System.IO.StreamReader(filepath);


        // int successLoadNum = 0;
        // while (!reader.EndOfStream)
        // {
        //     string objName = reader.ReadLine();
        //     string[] position = reader.ReadLine().Split(' ');
        //     string[] rotation = reader.ReadLine().Split(' ');
        //     string[] scale = reader.ReadLine().Split(' ');

        //     Debug.Log("Loading object with" + position.Length + " " + rotation.Length + " " + scale.Length);

        //     // if (position.Length != 3 || rotation.Length != 4 || scale.Length != 3)
        //     // {
        //     //     Debug.Log("Invalid format");
        //     //     continue;
        //     // }

        //     successLoadNum++;

        //     Vector3 pos = new Vector3(float.Parse(position[0]), float.Parse(position[1]), float.Parse(position[2]));
        //     Quaternion rot = new Quaternion(float.Parse(rotation[0]), float.Parse(rotation[1]), float.Parse(rotation[2]), float.Parse(rotation[3]));
        //     Vector3 sca = new Vector3(float.Parse(scale[0]), float.Parse(scale[1]), float.Parse(scale[2]));

        //     GameObject obj = GameObject.Find(objName);
        //     if (obj == null)
        //     {
        //         obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //         obj.name = objName;
        //     }

        //     obj.transform.position = pos;
        //     obj.transform.rotation = rot;
        //     obj.transform.localScale = sca;

        //     placedObjects.Add(obj);
        // }
        // Debug.Log("Loading objects done with " + successLoadNum + " objects loaded!");

        string json = File.ReadAllText(filepath);
        TransformDataList dataList = JsonUtility.FromJson<TransformDataList>(json);
        Debug.Log("Loading objects done with " + dataList.transforms.Count + " objects loaded!");
        for (int i = 0; i < dataList.transforms.Count; i++)
        {
            TransformData data = dataList.transforms[i];
            // GameObject obj = GameObject.Find(data.objectName);
            // if (obj == null)
            // {
            //     obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //     obj.name = data.objectName;
            // }

            Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
            Quaternion rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
            Vector3 localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);

            // add the object to random place in the scene
            GameObject newObject = Instantiate(objectToPlace, position, rotation);
        
            placedObjects.Add(newObject);

        }
    }
    public void FireSphere(){
        FireSphereAnywhere(arCamera.transform.position);
    }

    public void FireSphereAnywhere(Vector3 start_position){
        // fire sphere from the camera at any angle

        // create a sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // random scale from 0.1 to 0.5
        float randomScale = Random.Range(0.1f, 0.5f);
        sphere.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        // set the sphere position to the camera position
    
        sphere.transform.position = start_position;
        // store arCamera position to new variable a

        // set the sphere rotation to the camera rotation
        sphere.transform.rotation = arCamera.transform.rotation;

        // add rigidbody to the sphere
        Rigidbody rb = sphere.AddComponent<Rigidbody>();

        // add force to the sphere
        rb.AddForce(arCamera.transform.forward * 500f);

        // add shader to the sphere
        sphere.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
        // random color
        sphere.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value, 1.0f);

        // add the sphere to the placedObjects list
        firedSpheres.Add(sphere);

        // ShowFiredSpheresNum();
    }

    public int ShowFiredSpheresNum(){
        // Debug.Log("Number of fired spheres: " + firedSpheres.Count);
        return firedSpheres.Count;  
    }

    public int ShowPlacedObjectsNum(){
        // Debug.Log("Number of placed objects: " + placedObjects.Count);
        return placedObjects.Count;
    }
} 

