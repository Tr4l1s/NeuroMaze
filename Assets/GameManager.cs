using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Swift’te yazacaðýmýz native fonksiyonun imzasý
    [DllImport("__Internal")]
    private static extern void StartPulseMeasurement();

    // Güvenli alana girildiðinde çaðrýlacak fonksiyon
    public void OnEnterSafeZone()
    {
        Debug.Log("Güvenli alan: nabýz ölçümü baþlatýlýyor.");
#if !UNITY_EDITOR && UNITY_IOS
        StartPulseMeasurement();
#endif
    }

    // Swift’ten gelecek nabýz sonucunu alacak fonksiyon
    public void OnPulseResult(string bpmString)
    {
        Debug.Log("Swift’ten nabýz sonucu geldi: " + bpmString);
        if (int.TryParse(bpmString, out int bpm))
        {
            if (bpm > 90)
            {
                // Labirenti karart
                DarkenLabyrinth();
            }
            else
            {
                // Normal / aydýnlýk
                LightenLabyrinth();
            }
        }
    }

    private void DarkenLabyrinth()
    {
        // Senin ortam ýþýðý / post processing kodlarýn
    }

    private void LightenLabyrinth()
    {
        // Eski haline getir
    }
}
