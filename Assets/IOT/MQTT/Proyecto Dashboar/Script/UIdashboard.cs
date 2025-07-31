using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIdashboard : MonoBehaviour
{
    public Text leerText;
    public Image grafica;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(){
            
        // }
        float valorConvetido = float.Parse(leerText.text);
        grafica.fillAmount = valorConvetido / 300 * 1;
    }
}
