using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeuroMaze.Pulse
{
    public class TabletGameLayout : MonoBehaviour
    {
        int width,height;
        Rect safe;
        void OnEnable() {SceneManager.sceneLoaded+=Loaded;StartCoroutine(FitNextFrame());}
        void OnDisable() {SceneManager.sceneLoaded-=Loaded;}
        void Loaded(Scene scene,LoadSceneMode mode) {StartCoroutine(FitNextFrame());}
        IEnumerator FitNextFrame() {yield return null;Fit();}
        void Update() {if(width!=Screen.width || height!=Screen.height || safe!=Screen.safeArea) Fit();}
        void Fit()
        {
            width=Screen.width;height=Screen.height;safe=Screen.safeArea;
            foreach(var player in FindObjectsByType<FPSJoystickController>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(player.moveJoystick!=null) player.moveJoystick.FitTablet(false);
                if(player.lookJoystick!=null) player.lookJoystick.FitTablet(true);
            }
            foreach(var game in FindObjectsByType<GameManager>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var label=game.measureInfoText;
                if(label==null) continue;
                var canvas=label.GetComponentInParent<Canvas>();
                if(canvas==null || canvas.renderMode==RenderMode.WorldSpace) continue;
                canvas=canvas.rootCanvas;
                float scale=Mathf.Max(0.01f,canvas.scaleFactor);
                var rect=label.rectTransform;rect.SetParent(canvas.transform,false);
                rect.localScale=Vector3.one;
                rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0.5f,0);
                rect.sizeDelta=new Vector2(safe.width*0.72f/scale,safe.height*0.14f/scale);
                rect.anchoredPosition=new Vector2((safe.center.x-Screen.width*0.5f)/scale,(safe.yMin+12)/scale);
                label.enableAutoSizing=true;label.fontSizeMin=16/scale;label.fontSizeMax=28/scale;
                label.alignment=TMPro.TextAlignmentOptions.Center;label.raycastTarget=false;
                rect.SetAsLastSibling();
            }
        }
    }
}
