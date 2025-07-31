using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class infoMaquina
{
    public string q;
    public string t;
    public string v;
    public string id;
    public string timestamp;
}
[System.Serializable]
public class ListItem {
  public infoMaquina[] values;
}

public class JSON : MonoBehaviour
{
    public Text jsonParse;// es public por que la clase de mqtt_dashboard no pertence al MonoBehaviour
    // Start is called before the first frame update
    //private Json json;
    
    void Start()
    {

      
    }
    void Update()
    {
    
     ListItem pd = JsonUtility.FromJson<ListItem>(jsonParse.text);
      for(int i=0;i<pd.values.Length;i++)
        {
          Debug.Log("count = " + pd.values[i].v);
        }
    }
      

     
}