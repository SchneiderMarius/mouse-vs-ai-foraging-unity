using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cameracontrol : MonoBehaviour
{

    Texture2D frame;
    RenderTexture renderframe;
    Camera cam;
    Color[] px;
    public GameObject imgobj;
    public RawImage img;
    
    void Start()
    {
        cam = this.GetComponent<Camera>();
        img = imgobj.GetComponent<RawImage>();

        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    void Update()
    {
        
    }

    //renders a quick frame from the camera and returns its mean gray value
    public float getMeanGray()
    {
    
        //temp structures
        frame = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24,false);
        renderframe = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
    
        //tell camera to output a frame to renderframe
        cam.targetTexture = renderframe;
        cam.Render();
        
        //grab the texture2D from the renderframe- requires momentarily setting it as the active output
        RenderTexture.active = renderframe;
        frame.ReadPixels(new Rect(0,0,cam.pixelWidth, cam.pixelHeight),0,0); 
        RenderTexture.active = null;
        
        //point camera back toward the screen 
        cam.targetTexture = null;
    
        //get the pixel data from the Texture2D
        frame.Apply();
        //img.texture = frame; //for debugging- assign this to a rawimage to check the texture is good before pixel measurement
        px = frame.GetPixels();
    
        //average the pixels and return it
        float sum = 0;
        for (int i = 0; i < px.Length; i++) //ew
        {
            float gray = (px[i].r + px[i].g + px[i].b) / 3;
            sum += gray;
        }
    
        return sum / px.Length;
    }
}
