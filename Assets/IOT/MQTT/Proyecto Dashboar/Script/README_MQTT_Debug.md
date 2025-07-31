# Guía de Debugging MQTT en Unity

## Problemas Comunes y Soluciones

### 1. Verificación de Configuración

**Problema**: No se reciben mensajes MQTT aunque Node-RED funciona correctamente.

**Pasos para diagnosticar**:

1. **Verificar configuración del broker**:
   - Broker Address: Asegúrate de que sea la IP correcta (no "localhost" si el broker está en otra máquina)
   - Broker Port: Verifica que sea el puerto correcto (1883 para MQTT, 8883 para MQTT over SSL)
   - Client ID: Debe ser único

2. **Verificar topics**:
   - Los topics deben coincidir exactamente con los que publica Node-RED
   - Verificar mayúsculas/minúsculas
   - Verificar espacios o caracteres especiales

### 2. Uso del Script de Debugging

El script `MQTT_Debugger.cs` te ayudará a diagnosticar problemas:

1. **Agregar el script a tu escena**:
   - Crea un GameObject vacío
   - Agrega el componente `MQTT_Debugger`
   - Asigna tu `MQTT_GSN` en el campo correspondiente

2. **Botones disponibles**:
   - **Validar Configuración**: Muestra toda la configuración actual
   - **Forzar Reconexión**: Desconecta y reconecta al broker
   - **Test Publish**: Envía un mensaje de prueba
   - **Limpiar Logs**: Limpia los mensajes de debug
   - **Probar Configuración Default**: Usa configuración de prueba
   - **Verificar Red**: Comprueba conectividad de red

### 3. Verificación en Unity Console

Busca estos mensajes en la consola de Unity:

**Mensajes de éxito**:
```
¡CONECTADO EXITOSAMENTE al broker MQTT!
Suscrito al topic: [nombre_del_topic]
Mensaje recibido: [topic] -> [contenido]
```

**Mensajes de error**:
```
FALLA DE CONEXIÓN MQTT: [error]
No hay topics configurados para suscribirse
Error procesando JSON: [error]
```

### 4. Checklist de Verificación

#### Configuración del Broker:
- [ ] Broker Address es correcto (IP o hostname)
- [ ] Broker Port es correcto (1883, 8883, etc.)
- [ ] Client ID es único
- [ ] El broker está ejecutándose y es accesible

#### Configuración de Topics:
- [ ] Topics están configurados en el array `listaTopics`
- [ ] Los nombres de topics coinciden exactamente con Node-RED
- [ ] Los elementos UI (`TextMeshProUGUI`) están asignados
- [ ] `PocisionDeValor` es correcto (empieza en 0)

#### Configuración de UI:
- [ ] `consoleInputField` está asignado (para ver mensajes)
- [ ] Los elementos `TextMeshProUGUI` en `listaTopics` están asignados
- [ ] El script `MQTT_GSN` está en un GameObject activo

### 5. Pruebas Paso a Paso

1. **Prueba básica de conectividad**:
   ```
   Broker: localhost
   Puerto: 1883
   Topic: test/topic
   ```

2. **Verificar que Node-RED está publicando**:
   - Usa un cliente MQTT como MQTT Explorer para verificar
   - Confirma que los mensajes llegan al broker

3. **Verificar firewall y red**:
   - Asegúrate de que el puerto MQTT esté abierto
   - Verifica que Unity pueda acceder a la red

### 6. Configuración de Node-RED

Asegúrate de que Node-RED esté configurado correctamente:

```javascript
// Ejemplo de configuración MQTT en Node-RED
{
  "broker": "localhost",
  "port": 1883,
  "topic": "tu/topic/aqui",
  "qos": 1,
  "retain": false
}
```

### 7. Formato JSON Esperado

El script espera este formato JSON de Node-RED:

```json
{
  "values": [
    {
      "q": "quality",
      "t": "timestamp",
      "v": "valor",
      "id": "id",
      "timestamp": "timestamp"
    }
  ]
}
```

### 8. Solución de Problemas Específicos

#### No se conecta al broker:
- Verifica IP y puerto
- Comprueba que el broker esté ejecutándose
- Verifica firewall

#### Se conecta pero no recibe mensajes:
- Verifica nombres de topics
- Comprueba que Node-RED esté publicando
- Verifica QoS settings

#### Recibe mensajes pero no actualiza UI:
- Verifica que los elementos UI estén asignados
- Comprueba `PocisionDeValor`
- Verifica formato JSON

### 9. Comandos Útiles

En la consola de Unity, puedes usar estos métodos:

```csharp
// Obtener estado de conexión
Debug.Log(mqttClient.GetConnectionStatus());

// Validar configuración
mqttClient.ValidateConfiguration();

// Forzar reconexión
mqttClient.ForceReconnect();

// Limpiar logs
mqttClient.ClearDebugMessages();
```

### 10. Contacto y Soporte

Si sigues teniendo problemas:
1. Revisa la consola de Unity para errores específicos
2. Verifica que Node-RED esté funcionando correctamente
3. Usa el script de debugging para obtener más información
4. Comprueba la conectividad de red

---

**Nota**: Este script ha sido mejorado con validaciones adicionales y mejor manejo de errores para facilitar el debugging de problemas de conexión MQTT. 