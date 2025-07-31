using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using System.Linq;

namespace M2MqttUnity.Examples
{
    //son las variables que conforman el JSON que recibimos 
    [System.Serializable]
    public class infoMaquinaGsn// variables secuandarias que estan adentro de llaves y corchetes
    {
        public string id;
        public double v;  // Cambiado a double para valores numéricos
        public bool q;    // Cambiado a bool para valores booleanos
        public long t;    // Cambiado a long para timestamps
    }
    //esta variable sirve como la principal del json ya que despues de esta variable siguen las que estan adentro de llaves y corchetes
    [System.Serializable]
    public class ListItemGsn {
        public long timestamp;  // Campo añadido para el timestamp raíz
        public infoMaquinaGsn[] values;//variable principal del json que recibimos de Kepware
    }

    [System.Serializable]
    public class ListTopics
    {
        public string NombreTopic;
        public string Topic;
        public int PocisionDeValor;//si hay un valor en el array del valor "values" en ocaciones puede haber en el mismo topic n cantidad de vairables y se pocisionan empezando de 0 en el caso de que sea uno
        public string JSON_topic;
        public TextMeshProUGUI v;
    }
    //terminan las variables del JSON
    public class MQTT_GSN : M2MqttUnityClient
    {
        public bool autoTest = false;
        private bool updateUI = false;
        public InputField consoleInputField;
        
        private List<string> eventMessages = new List<string>();

        // variables de la clase
        public ListTopics[] listaTopics;
        
        // Dictionary to store values from each station
        private Dictionary<string, string> stationValues = new Dictionary<string, string>();
        
        // <echoPorTadeo>
        //public Text[] datosRamdom;
        
        // public string[] Topics;
        // public string[] valoresTopics;

        // ///objetos del JSON

        // public Text[] v;//variable creada con el mismo nombre del objeto que esta dentro del JSON

       // string[] value = new string[10];
        //List<int> lista = new List<int>();
        bool bandera = true;

        public void TestPublish()
        {
            for (int i = 0; i < listaTopics.Length; i++)
            {
                client.Publish(listaTopics[i].Topic, System.Text.Encoding.UTF8.GetBytes("Test message"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                Debug.Log("Test message published");
                AddUiMessage("Test message published.");
            }
        }
        
        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }
        
        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";
                updateUI = true;
            }
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SetUiMessage("Connected to broker on " + brokerAddress + "\n");

            if (autoTest)
            {
                TestPublish();
            }
        }
        
        protected override void SubscribeTopics()
        {
            // Suscribimos UNA SOLA VEZ al topic con el comodín.
            client.Subscribe(new string[] { "SLC504/+" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Debug.Log("Suscrito a SLC504/+ para el display combinado.");
        }
        
        protected override void UnsubscribeTopics()
        {
            for (int i = 0; i < listaTopics.Length; i++)
            {
                client.Unsubscribe(new string[] { listaTopics[i].Topic });
            }
        }
        
        protected override void OnConnectionFailed(string errorMessage)
        {
            AddUiMessage("CONNECTION FAILED! " + errorMessage);
        }

        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
        }

        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);

            try
            {
                ListItemGsn pd = JsonUtility.FromJson<ListItemGsn>(msg);
                if (pd != null && pd.values != null)
                {
                    // Elige qué valor quieres mostrar. 
                    // 0=encendido, 1=flujo, 2=potencia, 3=presion, 4=velocidad
                    int pocisionDelValor = 1; // Mostraremos el flujo

                    if (pocisionDelValor < pd.values.Length)
                    {
                        // Guarda el valor para este topic específico
                        string valor = pd.values[pocisionDelValor].v.ToString("F2"); // "F2" formatea el número a 2 decimales
                        stationValues[topic] = valor;
                        
                        // Llama a la función para actualizar el texto
                        UpdateCombinedText();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error procesando el mensaje de {topic}: {e.Message}");
            }
        }

        void UpdateCombinedText()
        {
            // Revisa si el objeto de texto está asignado en el inspector
            if (listaTopics == null || listaTopics.Length == 0 || listaTopics[0].v == null)
            {
                return;
            }

            string textoFinal = "";
            
            // Ordena los topics alfabéticamente para que el texto no cambie de orden
            var topicsOrdenados = stationValues.Keys.ToList();
            topicsOrdenados.Sort();

            // Crea una línea de texto para cada estación que haya enviado datos
            foreach (string topic in topicsOrdenados)
            {
                string nombreEstacion = topic.Replace("SLC504/", ""); // Extrae "estacion1", "estacion2", etc.
                textoFinal += $"{nombreEstacion}: {stationValues[topic]}\n";
            }
            
            // Asigna el texto combinado al TextMeshPro
            listaTopics[0].v.SetText(textoFinal);
        }

        private void ProcessMessage(string msg)
        {
            AddUiMessage("Received: " + msg);
        }

        private void StoreMessage(string eventMsg)
        {
            for (int i = 0; i < eventMsg.Length; i++)
            {
                //eventMessages.Add(eventMsg);
                //Debug.Log(eventMessages[i] + "  " + i);
            }
        }
        
        // Método Update para procesar actualizaciones de UI
        protected override void Update()
        {
            // Esto llama al Update() de M2MqttUnityClient,
            // que procesa la cola y dispara DecodeMessage()
            base.Update();

            // Luego tu lógica de UI
            if (updateUI)
            {
                Canvas.ForceUpdateCanvases();
                updateUI = false;
            }
        }
        
        // Método para verificar la configuración
        public void VerifyConfiguration()
        {
            Debug.Log("=== VERIFICACIÓN DE CONFIGURACIÓN ===");
            Debug.Log($"Broker Address: {brokerAddress}");
            Debug.Log($"Broker Port: {brokerPort}");
            Debug.Log($"Auto Connect: {autoConnect}");
            Debug.Log($"Lista Topics Length: {listaTopics?.Length ?? 0}");
            
            if (listaTopics != null)
            {
                for (int i = 0; i < listaTopics.Length; i++)
                {
                    Debug.Log($"Topic {i}:");
                    Debug.Log($"  Nombre: {listaTopics[i].NombreTopic}");
                    Debug.Log($"  Topic: {listaTopics[i].Topic}");
                    Debug.Log($"  Posición: {listaTopics[i].PocisionDeValor}");
                    Debug.Log($"  TextMeshPro: {(listaTopics[i].v != null ? "CONFIGURADO" : "NO CONFIGURADO")}");
                    if (listaTopics[i].v != null)
                    {
                        Debug.Log($"  TextMeshPro Text: '{listaTopics[i].v.text}'");
                        Debug.Log($"  GameObject Active: {listaTopics[i].v.gameObject.activeInHierarchy}");
                        Debug.Log($"  Hierarchy: {GetHierarchyPath(listaTopics[i].v.transform)}");
                        
                                                 // Verificar Canvas
                         Canvas canvas = listaTopics[i].v.GetComponentInParent<Canvas>();
                         if (canvas != null)
                         {
                             Debug.Log($"  Canvas: {canvas.name} (Active: {canvas.gameObject.activeInHierarchy})");
                             Debug.Log($"  Canvas Render Mode: {canvas.renderMode}");
                             Debug.Log($"  Canvas Sorting Layer: {canvas.sortingLayerName}");
                             Debug.Log($"  Canvas Order in Layer: {canvas.sortingOrder}");
                             
                             // Verificar Canvas Group
                             CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
                             if (canvasGroup != null)
                             {
                                 Debug.Log($"  Canvas Group Alpha: {canvasGroup.alpha}");
                                 Debug.Log($"  Canvas Group Interactable: {canvasGroup.interactable}");
                             }
                         }
                         else
                         {
                             Debug.LogWarning($"  No Canvas found in hierarchy!");
                         }
                         
                         // Verificar RectTransform
                         RectTransform rectTransform = listaTopics[i].v.GetComponent<RectTransform>();
                         if (rectTransform != null)
                         {
                             Debug.Log($"  RectTransform Position: {rectTransform.position}");
                             Debug.Log($"  RectTransform Scale: {rectTransform.localScale}");
                             Debug.Log($"  RectTransform Size: {rectTransform.sizeDelta}");
                             Debug.Log($"  RectTransform Anchors: {rectTransform.anchorMin} - {rectTransform.anchorMax}");
                         }
                         
                         // Verificar configuración del TextMeshPro
                         Debug.Log($"  TMP Font Size: {listaTopics[i].v.fontSize}");
                         Debug.Log($"  TMP Color: {listaTopics[i].v.color}");
                         Debug.Log($"  TMP Enabled: {listaTopics[i].v.enabled}");
                         Debug.Log($"  TMP Raycast Target: {listaTopics[i].v.raycastTarget}");
                    }
                }
            }
            Debug.Log("=== FIN VERIFICACIÓN ===");
        }
        
        private string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
        
        // Método para probar el parsing de JSON
        public void TestJSONParsing()
        {
            Debug.Log("=== PRUEBA DE PARSING JSON ===");
            string testJSON = @"{
                ""timestamp"": 1753811948840,
                ""values"": [
                    {
                        ""id"": ""Example_variables.estacion1.encendido"",
                        ""v"": 0,
                        ""q"": true,
                        ""t"": 1753805189302
                    },
                    {
                        ""id"": ""Example_variables.estacion1.flujo"",
                        ""v"": 545.5,
                        ""q"": true,
                        ""t"": 1753811287294
                    }
                ]
            }";
            
            try
            {
                ListItemGsn pd = JsonUtility.FromJson<ListItemGsn>(testJSON);
                Debug.Log($"JSON de prueba parseado exitosamente. Timestamp: {pd.timestamp}, Valores: {pd.values.Length}");
                
                for (int i = 0; i < pd.values.Length; i++)
                {
                    Debug.Log($"Valor {i}: {pd.values[i].v} (ID: {pd.values[i].id}, Quality: {pd.values[i].q})");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parseando JSON de prueba: {e.Message}");
            }
            Debug.Log("=== FIN PRUEBA JSON ===");
        }
        
        // Método para forzar actualización de textos (prueba de renderizado)
        public void ForceUpdateTexts()
        {
            Debug.Log("=== FORZANDO ACTUALIZACIÓN DE TEXTOS ===");
            for(int i = 0; i < listaTopics.Length; i++)
            {
                if(listaTopics[i].v != null)
                {
                    string testText = "TEST " + Time.time.ToString("F2");
                    listaTopics[i].v.SetText(testText);
                    listaTopics[i].v.ForceMeshUpdate();
                    Debug.Log($"Topic {i}: Texto actualizado a '{testText}'");
                    
                    // Forzar actualización del Canvas
                    Canvas.ForceUpdateCanvases();
                }
                else
                {
                    Debug.LogWarning($"Topic {i}: TextMeshPro es null");
                }
            }
            Debug.Log("=== FIN ACTUALIZACIÓN FORZADA ===");
        }
        
        // Método para limpiar todos los textos
        public void ClearAllTexts()
        {
            Debug.Log("=== LIMPIANDO TODOS LOS TEXTOS ===");
            for(int i = 0; i < listaTopics.Length; i++)
            {
                if(listaTopics[i].v != null)
                {
                    listaTopics[i].v.SetText("");
                    listaTopics[i].v.ForceMeshUpdate();
                    Debug.Log($"Topic {i}: Texto limpiado");
                }
            }
            Canvas.ForceUpdateCanvases();
            Debug.Log("=== FIN LIMPIEZA ===");
        }
        
        // Método para prueba visual automática
        public void StartVisualTest()
        {
            Debug.Log("=== INICIANDO PRUEBA VISUAL AUTOMÁTICA ===");
            StartCoroutine(VisualTestCoroutine());
        }
        
        private IEnumerator VisualTestCoroutine()
        {
            Debug.Log("Esperando 1 segundo antes de la prueba visual...");
            yield return new WaitForSeconds(1f);
            
            if (listaTopics != null && listaTopics.Length > 0 && listaTopics[0].v != null)
            {
                Debug.Log("Aplicando prueba visual al primer TextMeshPro...");
                listaTopics[0].v.SetText("¡Hola Mundo! " + Time.time.ToString("F2"));
                listaTopics[0].v.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                Debug.Log("Prueba visual aplicada. ¿Puedes ver el texto '¡Hola Mundo!'?");
            }
            else
            {
                Debug.LogError("No se puede realizar la prueba visual: listaTopics vacía o TextMeshPro no configurado");
            }
        }
        
        // Método para cambiar color de fondo temporalmente (para debug visual)
        public void ToggleBackgroundColor()
        {
            Debug.Log("=== CAMBIANDO COLOR DE FONDO PARA DEBUG ===");
            for(int i = 0; i < listaTopics.Length; i++)
            {
                if(listaTopics[i].v != null)
                {
                    // Cambiar entre rojo y blanco para hacer el texto más visible
                    if (listaTopics[i].v.color == Color.white)
                    {
                        listaTopics[i].v.color = Color.red;
                        Debug.Log($"Topic {i}: Color cambiado a ROJO");
                    }
                    else
                    {
                        listaTopics[i].v.color = Color.white;
                        Debug.Log($"Topic {i}: Color cambiado a BLANCO");
                    }
                    listaTopics[i].v.ForceMeshUpdate();
                }
            }
            Canvas.ForceUpdateCanvases();
        }
        
        public void DebugTMPVisibility()
        {
            if (listaTopics != null)
            {
                for (int i = 0; i < listaTopics.Length; i++)
                {
                    if (listaTopics[i].v != null)
                    {
                        listaTopics[i].v.SetText("VISIBLE " + Time.time.ToString("F2"));
                        listaTopics[i].v.color = Color.red;
                        listaTopics[i].v.fontSize = 48;
                        listaTopics[i].v.gameObject.SetActive(true);
                        listaTopics[i].v.ForceMeshUpdate();
                        Debug.Log($"TMP {i} visibilidad forzada");
                    }
                    else
                    {
                        Debug.LogWarning($"TMP {i} es null");
                    }
                }
                Canvas.ForceUpdateCanvases();
            }
        }
        
        protected override void Start()
        {
            base.Start();
            //DebugTMPVisibility(); // Ejecuta la función al iniciar
        }
    }
}
