// ref: https://www.patreon.com/posts/89865318
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TakePhotos : MonoBehaviour
{
    // Call this in the button.
    public void TakePhoto()
    {
        StartCoroutine(TakeAPhoto());
    }

    IEnumerator TakeAPhoto()
    {
        // Wait until rendering is complete, before take the photo.
        yield return new WaitForEndOfFrame();

        Camera camera = Camera.main;
        int width = Screen.width;
        int height = Screen.height;
        // Create a new render texture the size of the screen.
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;

        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(width, height);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        // Change back the camera target texture.
        camera.targetTexture = null;

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;

        // Save to an image file.
        // Encode the image texture into PNG. Can be change to another image file.
        byte[] bytes = image.EncodeToPNG();
        // Sample file name: 20230925_192218.png
        string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, bytes);

        // Share the image file.
        new NativeShare().AddFile( filePath ).SetSubject( "Subject goes here" ).SetText( "Hello world!" ).Share();

        // Free up memory.
        Destroy(rt);
        Destroy(image);
    }
}
