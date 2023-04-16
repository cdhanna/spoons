using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintBehaviour : MonoBehaviour
{
    [Header("scene references")]
    public RectTransform canvas;
    public Camera drawCamera;

    public RawImage paintArea;

    [Header("Asset references")]
    public RenderTexture renderTex;
    public Material paintBrushMaterial;

    [Header("Runtime data")] 
    public Texture2D tex;

    public Vector2 brush, canvasData;
    public Vector2 prevBrush;
    
    // Start is called before the first frame update
    void Start()
    {
        tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGB24, false);
    }


    // Update is called once per frame
    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,Input.mousePosition,drawCamera,out var uv );
        uv.x /= canvas.rect.width;
        uv.y /= canvas.rect.height;
        uv += Vector2.one * .5f;
        
        // Debug.Log(uv);

        canvasData = new Vector4(canvas.rect.width, canvas.rect.height, 0, 0);

        prevBrush = brush;
        brush = new Vector4(uv.x, uv.y, 10, 1);
        if (Input.GetMouseButton(0))
        {
            // Debug.Log("------- DRAW ----");

            Draw();
        }

        

        if (Input.GetMouseButtonDown(1))
        {
            // clear
            Debug.Log("----- ERASE -----");
            // renderTex.DiscardContents();
            // tex.Reinitialize(tex.width, tex.height);//
            var c = tex.width * tex.height;
            var blanks = new Color[c];
            for (var i = 0; i < blanks.Length; i++)
            {
                blanks[i] = Color.black;
            }
            tex.SetPixels(blanks);
            tex.Apply();
            // Graphics.Texu
          
            brush.x = -1000;
            prevBrush = brush;
            Draw();
        }
    }

    void Draw()
    {
        paintBrushMaterial.SetVector("_BrushOld", prevBrush);
        paintBrushMaterial.SetVector("_Brush", brush);
        paintBrushMaterial.SetVector("_Canvas", canvasData);
        // var temp = RenderTexture.GetTemporary(renderTex.descriptor);
        // Graphics.CopyTexture(renderTex, tex);
        Graphics.Blit(tex, renderTex, paintBrushMaterial);
        Graphics.CopyTexture(renderTex, tex);

        // RenderTexture.ReleaseTemporary(temp);
        // RenderTexture.active = renderTex;
        // tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        // tex.Apply();
    }
}
