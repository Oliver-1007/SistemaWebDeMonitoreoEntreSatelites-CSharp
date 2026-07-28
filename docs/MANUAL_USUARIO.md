# Manual de Usuario — OrbitNet-NetCore
### Sistema Web de Monitoreo, Enrutamiento y Simulación de Conexiones Satelitales Distribuidas en Red Local

**Universidad de San Carlos de Guatemala**
**Facultad de Ingeniería — Escuela de Ciencias y Sistemas (ECYS)**
**Curso:** Introducción a la Programación y Computación II
**Vigencia:** Primer Semestre / Escuela de Vacaciones 2026
**Catedrático:** Ing. Jaime Francisco Yumán Ramírez
**Auxiliar de Cátedra:** Fernando José Vicente Velásquez

---

## Tabla de Contenidos

1. [Introducción](#1-introducción)
2. [Requisitos del Sistema](#2-requisitos-del-sistema)
3. [Inicio del Sistema — Levantamiento de las Instancias](#3-inicio-del-sistema--levantamiento-de-las-instancias)
4. [Panel Principal (Dashboard)](#4-panel-principal-dashboard)
5. [Carga Masiva de Configuración XML](#5-carga-masiva-de-configuración-xml)
6. [Gestión Manual de Nodos Satelitales](#6-gestión-manual-de-nodos-satelitales)
7. [Enrutamiento entre Satélites](#7-enrutamiento-entre-satélites)
8. [Comunicación Distribuida Inter-Puerto (Norte ↔ Sur)](#8-comunicación-distribuida-inter-puerto-norte--sur)
9. [Reportes de Visualización SVG](#9-reportes-de-visualización-svg)
10. [Bitácora de Auditoría](#10-bitácora-de-auditoría)
11. [Consulta de la API REST](#11-consulta-de-la-api-rest)
12. [Mensajes del Sistema — Guía de Interpretación](#12-mensajes-del-sistema--guía-de-interpretación)
13. [Archivos de Configuración de Prueba](#13-archivos-de-configuración-de-prueba)

---

## 1. Introducción

El presente manual tiene como propósito guiar al usuario final en el uso correcto y ordenado del sistema **OrbitNet-NetCore**, un simulador web de redes satelitales distribuidas desarrollado sobre la plataforma .NET 8.0 y el lenguaje C#.

El sistema permite modelar la topología de una constelación satelital activa mediante la carga de archivos de configuración XML, la gestión manual de nodos en un plano espacial cartesiano, el trazado de rutas de enrutamiento entre satélites, y la visualización en tiempo real del estado de la memoria del servidor a través de reportes gráficos vectoriales SVG.

Una característica distintiva de OrbitNet-NetCore es que opera bajo una arquitectura **distribuida en red local**: el sistema se despliega como dos instancias simultáneas del servidor, cada una representando una constelación hemisférica diferente. Esto permite simular el cruce de paquetes de datos entre el Hemisferio Norte (puerto `5000`) y el Hemisferio Sur (puerto `5001`) mediante comunicaciones HTTP autenticadas.

Toda la información gestionada por el sistema se almacena **exclusivamente en la memoria RAM del servidor**. Esto significa que, al detener cualquiera de las instancias, el estado del plano satelital se pierde y debe reconfigurarse en el siguiente arranque.

---

## 2. Requisitos del Sistema

Antes de ejecutar OrbitNet-NetCore, el equipo de trabajo debe verificar que el entorno de desarrollo cumpla con los siguientes requisitos:

| Componente | Requisito mínimo |
|---|---|
| Sistema Operativo | Windows 10/11, macOS 12+, o Linux (Ubuntu 20.04+) |
| Plataforma .NET | .NET 8.0 SDK instalado |
| Graphviz | Versión 2.50 o superior instalada y disponible en el PATH del sistema |
| Navegador web | Google Chrome 110+, Mozilla Firefox 110+, o Microsoft Edge 110+ |
| Puertos disponibles | `5000` y `5001` libres en la máquina local |
| Memoria RAM | Mínimo 4 GB disponibles para las dos instancias simultáneas |

### Verificación de Graphviz

Para confirmar que Graphviz está correctamente instalado y accesible desde el sistema, abra una terminal y ejecute el siguiente comando:

```bash
dot -V
```

Si la instalación es correcta, el sistema responderá con algo similar a `dot - graphviz version 9.0.0`. Si el comando no es reconocido, deberá instalar Graphviz desde [https://graphviz.org/download/](https://graphviz.org/download/) y asegurarse de que el directorio `bin` de Graphviz esté incluido en la variable de entorno `PATH`.

---

## 3. Inicio del Sistema — Levantamiento de las Instancias

OrbitNet-NetCore requiere que **dos instancias del servidor** estén activas de forma simultánea para que la simulación distribuida funcione correctamente. Cada instancia debe ejecutarse en una terminal independiente.

### Paso 1: Levantar la Instancia del Hemisferio Norte (Puerto 5000)

Abra una terminal, navegue al directorio del proyecto `OrbitNet.WebNorte` y ejecute:

```bash
cd OrbitNet.WebNorte
dotnet run --urls "http://localhost:5000"
```

Cuando el servidor esté listo, la terminal mostrará un mensaje similar a:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Paso 2: Levantar la Instancia del Hemisferio Sur (Puerto 5001)

Sin cerrar la primera terminal, abra una **segunda terminal**, navegue al directorio `OrbitNet.WebSur` y ejecute:

```bash
cd OrbitNet.WebSur
dotnet run --urls "http://localhost:5001"
```

### Paso 3: Acceder al sistema desde el navegador

Con ambas instancias activas, acceda a cada constelación desde el navegador web:

| Constelación | URL de acceso |
|---|---|
| Hemisferio Norte | [http://localhost:5000](http://localhost:5000) |
| Hemisferio Sur | [http://localhost:5001](http://localhost:5001) |

Se recomienda abrir cada instancia en una pestaña o ventana diferente del navegador para trabajar con ambas constelaciones de forma simultánea.

---

## 4. Panel Principal (Dashboard)

Al acceder a cualquiera de las dos instancias desde el navegador, el usuario es recibido por el **Panel Principal** o Dashboard, que constituye el centro de control de toda la operación del simulador.

### Elementos del Dashboard

El Dashboard está organizado en las siguientes secciones:

**Barra de navegación superior:** Contiene los accesos directos a todas las secciones funcionales del sistema: gestión de satélites, carga XML, enrutamiento, comunicación HTTP y visualización de reportes. También incluye el selector de idioma para alternar entre Español e Inglés (i18n).

**Panel de visualización SVG central:** Muestra en tiempo real el diagrama vectorial del estado actual del plano satelital, generado directamente desde la memoria RAM del servidor mediante Graphviz. El diagrama se actualiza automáticamente cada vez que se realiza una operación sobre la Matriz Dispersa.

**Panel de bitácora de auditoría:** Ubicado en la parte inferior o lateral del dashboard, muestra en orden cronológico todos los eventos registrados por el sistema: inserciones, eliminaciones, errores de validación, intentos de acceso y operaciones de enrutamiento.

**Panel de estado de TDAs:** Muestra contadores en tiempo real del número de nodos activos en la Matriz Dispersa, la cantidad de satélites en el Catálogo AVL y el total de eventos en la bitácora.

---

<p align="center">
  <img src="./Imagenes/Cap2.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

## 5. Carga Masiva de Configuración XML

La forma principal de poblar el plano satelital es mediante la carga de un archivo XML estructurado. Este proceso carga simultáneamente satélites ecuatoriales, satélites polares y antenas terrestres de forma **transaccional**: si cualquier elemento del archivo presenta un error de formato, la carga completa se cancela y el sistema permanece en su estado anterior sin modificaciones.

### Estructura del archivo XML esperado

El archivo XML debe respetar la siguiente estructura jerárquica para ser aceptado por el sistema:

```xml
<?xml version="1.0" encoding="utf-8"?>
<orbitnet>
  <constelaciones_ecuatoriales>
    <satelite id="SAT-ECU-0001">
      <nombre>Starlink-Norte-A</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
  </constelaciones_ecuatoriales>

  <orbitas_polares>
    <polar id="POLAR-NORTE-S">
      <satelite id="SAT-POL-1001">
        <nombre>Polar-Norte-1</nombre>
        <frecuencia>450.15</frecuencia>
      </satelite>
    </polar>
  </orbitas_polares>

  <antenas_terrestres>
    <antena id="ANT-GTM-401">
      <nombre>Estación Central USAC</nombre>
      <coordenadas>14.5891,-90.5514</coordenadas>
      <ip_nodo>10.0.0.50</ip_nodo>
    </antena>
  </antenas_terrestres>
</orbitnet>
```

### Formatos válidos para cada campo

Es fundamental que los datos del archivo cumplan con los formatos requeridos por el sistema. Un solo campo inválido en cualquier elemento provocará el rechazo de la carga completa.

| Campo | Formato requerido | Ejemplo válido | Ejemplo inválido |
|---|---|---|---|
| ID de Satélite | `SAT-(ECU\|POL)-XXXX` (4 dígitos) | `SAT-ECU-0012` | `SAT-ECU-99` |
| ID de Antena | `ANT-[3 letras mayúsculas]-[3 o 4 dígitos]` | `ANT-GTM-401` | `ANT-gt-40` |
| Dirección IPv4 | Cuatro octetos entre 0 y 255 separados por puntos | `10.0.0.50` | `172.300.4.1` |
| Coordenadas | `Latitud,Longitud` con 4 a 6 decimales | `14.5891,-90.5514` | `14.5891,-90.XYZ` |
| Frecuencia | Número decimal positivo | `450.15` | `-20.0` o `abc` |

### Pasos para realizar la carga masiva

**Paso 1:** Desde el Dashboard, localice el panel o botón de carga de archivo XML. En la barra de navegación superior, busque la sección etiquetada como **"Carga XML"** o **"Configuración"**.

---

<p align="center">
  <img src="./Imagenes/Cap3.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

**Paso 2:** Haga clic en el botón **"Seleccionar archivo"** o **"Examinar"** y navegue hasta el archivo XML de configuración que desea cargar.

**Paso 3:** Haga clic en el botón **"Cargar"** o **"Procesar XML"**. El sistema comenzará a leer y validar el archivo.

**Paso 4:** Observe el resultado en la parte superior del Dashboard. El sistema mostrará uno de los siguientes mensajes:

- **Mensaje verde (éxito):** La carga fue procesada correctamente. Indica cuántos nodos fueron insertados en la Matriz Dispersa y cuántos satélites en el Catálogo AVL. Ejemplo: `"Carga masiva completada con éxito (Commit). Se insertaron 3 nodos en la Matriz Dispersa y 2 satélites en el Catálogo AVL."`

- **Mensaje rojo (error):** La transacción fue abortada. Se indica la causa exacta del fallo, incluyendo el campo y el valor que no superó la validación. Ejemplo: `"Transacción de carga abortada (Rollback). Causa: El ID de Satélite 'SAT-ECU-99' no cumple con el patrón sintáctico requerido."`

---

<p align="center">
  <img src="./Imagenes/Cap4.5.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

### ¿Qué ocurre internamente durante la carga?

Para mayor comprensión del comportamiento del sistema, a continuación se describe lo que ocurre de forma invisible durante el proceso:

1. Los satélites ecuatoriales se insertan en la **Matriz Dispersa Ortogonal** en la fila de coordenada 0, usando los últimos 4 dígitos de su ID como columna.
2. Los satélites polares se insertan en el **Árbol AVL** como catálogo de referencia, ordenados alfabéticamente por ID.
3. Las antenas terrestres se insertan en la **Matriz Dispersa Ortogonal** convirtiendo sus coordenadas geográficas (latitud y longitud) a coordenadas enteras mediante redondeo matemático.
4. Si todos los elementos pasan las validaciones, se ejecuta el **commit**: se recorre la lista temporal interna y cada elemento se escribe definitivamente en los TDAs en RAM.
5. Si algún elemento falla, se ejecuta el **rollback**: la lista temporal se descarta sin modificar las estructuras existentes.

---

## 6. Gestión Manual de Nodos Satelitales

Además de la carga masiva por archivo XML, el sistema permite insertar y eliminar nodos de satélite de forma individual desde la interfaz web. Esta funcionalidad es útil para pruebas puntuales o para ajustar la topología del plano sin necesidad de cargar un archivo completo.

### 6.1 Insertar un Nodo Satelital

Para añadir un satélite o antena de forma individual al plano, localice el formulario de inserción manual en el Dashboard o en la sección de gestión de satélites.

---

<p align="center">
  <img src="./Imagenes/Cap6.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

Complete los campos requeridos con la siguiente información:

| Campo | Descripción | Restricciones |
|---|---|---|
| **Fila** | Coordenada de latitud entera del nodo en el plano | Número entero, puede ser negativo |
| **Columna** | Coordenada de longitud entera del nodo en el plano | Número entero, puede ser negativo |
| **ID** | Identificador único del satélite o antena | Debe seguir el formato `SAT-(ECU\|POL)-XXXX` |
| **Nombre** | Nombre descriptivo del nodo | Texto libre, no puede estar vacío |
| **Dirección IP** | IP del nodo | Formato IPv4 válido |

> **Importante:** Si ya existe un nodo en las coordenadas `(Fila, Columna)` indicadas, o si el ID ya está registrado en el plano, el sistema rechazará la inserción y mostrará un mensaje de error. No se admiten colisiones de coordenadas ni IDs duplicados.

Haga clic en **"Insertar"** y observe el mensaje de confirmación. El diagrama SVG del Dashboard se actualizará automáticamente para reflejar el nuevo nodo en el plano.

---

### 6.2 Eliminar un Nodo Satelital

Para eliminar un nodo existente, localice el formulario de eliminación en la interfaz. Deberá proporcionar únicamente las coordenadas `(Fila, Columna)` del nodo que desea remover.

---

<p align="center">
  <img src="./Imagenes/Eliminar.png" alt="Arquitectura OrbitNet" width="700"/>
</p>
---

El sistema verificará que exista un nodo en la posición indicada. Si se encuentra, lo eliminará de la Matriz Dispersa y reconectará automáticamente los punteros de los cuatro nodos vecinos (arriba, abajo, izquierda y derecha) para mantener la integridad del plano. Si no existe ningún nodo en esas coordenadas, se mostrará un mensaje de advertencia.

### 6.3 Limpiar el Plano Completo

Si desea reiniciar el plano satelital desde cero, utilice el botón **"Limpiar Plano"** o **"Purgar Todo"**. Esta acción eliminará todos los nodos de la Matriz Dispersa y vaciará el Catálogo AVL de satélites polares de forma simultánea.

> **Advertencia:** Esta operación es irreversible. Al no existir persistencia en disco, los datos eliminados no podrán recuperarse a menos que se realice una nueva carga de archivo XML.

---

## 7. Enrutamiento entre Satélites

El módulo de enrutamiento permite calcular y visualizar la ruta de saltos ortogonales entre dos nodos del plano satelital. La ruta se determina recorriendo los punteros físicos de la Matriz Dispersa, por lo que únicamente es posible trazar conexiones entre nodos que estén enlazados de forma directa o indirecta en el plano.

### Pasos para trazar una ruta

**Paso 1:** Localice el panel de enrutamiento en el Dashboard o en la sección **"Enrutamiento"** de la barra de navegación.

**Paso 2:** Seleccione el **satélite de origen** y el **satélite de destino** de las listas desplegables disponibles. Estas listas se pueblan automáticamente con todos los nodos activos en el plano.

---

<p align="center">
  <img src="./Imagenes/Rutaa.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

**Paso 3:** Haga clic en el botón **"Trazar Ruta"**. El sistema calculará el camino de menor número de saltos entre ambos nodos siguiendo los punteros ortogonales de la Matriz.

**Paso 4:** Observe el resultado:

- **Si la ruta existe:** El diagrama SVG del plano se actualiza, resaltando con un color diferenciado los nodos que conforman la ruta calculada y las aristas que los conectan. Se mostrará un mensaje indicando el número total de saltos realizados. Ejemplo: `"Ruta trazada con éxito. Saltos totales: 3."`

- **Si no existe ruta:** El sistema mostrará un mensaje de alerta indicando que no existe un camino físico de conexión entre los nodos seleccionados. Esto ocurre cuando la malla del plano está fragmentada y los nodos no están enlazados entre sí.

---

<p align="center">
  <img src="./Imagenes/Trazo.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

**Paso 5:** Para limpiar el trazado y restaurar la visualización normal del plano sin destacar ninguna ruta, haga clic en el botón **"Limpiar Ruta"**.

---

## 8. Comunicación Distribuida Inter-Puerto (Norte ↔ Sur)

Esta funcionalidad permite enviar peticiones HTTP autenticadas entre las dos instancias del simulador, simulando el salto de un paquete de datos entre las constelaciones del Hemisferio Norte y del Hemisferio Sur.

Para que esta funcionalidad opere correctamente, **ambas instancias del servidor deben estar activas** de forma simultánea (ver Sección 3).

### Cómo realizar una petición al cliente HTTP

**Paso 1:** Desde la instancia del Hemisferio Norte (`http://localhost:5000`), localice el panel de **"Cliente HTTP"** o **"Comunicación Inter-Puerto"** en el Dashboard o la barra de navegación.

---

<p align="center">
  <img src="./Imagenes/Consulta.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

**Paso 2:** Complete el formulario con los siguientes datos:

| Campo | Descripción | Valor de ejemplo |
|---|---|---|
| **URL de destino** | Dirección completa del endpoint a consultar | `http://localhost:5001/api/v1/space/relay` |
| **Usuario** | Nombre de usuario para la autenticación | `orbitnet_admin` |
| **Contraseña** | Contraseña de acceso | `USAC_ECYS_2026` |

> **Nota:** Los campos de usuario y contraseña son opcionales. Si se dejan en blanco, la petición se enviará sin cabecera de autenticación. Si el endpoint de destino requiere autenticación y no se proporcionan credenciales, la instancia receptora responderá con un error `401 Unauthorized`.

**Paso 3:** Haga clic en el botón **"Enviar Petición"** o **"Consultar"**. La petición se enviará de forma asíncrona a la URL indicada.

**Paso 4:** El resultado de la petición se mostrará en el Dashboard de la instancia emisora:

- **Respuesta exitosa:** Se muestra el JSON retornado por la instancia receptora en un panel de resultados, acompañado de un mensaje verde de éxito.
- **Error de autenticación (401):** Se muestra el payload de error retornado por la instancia Sur y un mensaje rojo indicando que las credenciales fueron rechazadas.
- **Error de conexión:** Si la instancia de destino no está activa, se mostrará un mensaje de error de red en la bitácora y en la interfaz.

---

<p align="center">
  <img src="./Imagenes/ConsolaAuditoria.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

### Credenciales de autenticación del sistema

Las credenciales establecidas para la comunicación inter-instancia son fijas y están definidas en el sistema:

| Parámetro | Valor |
|---|---|
| Usuario | `orbitnet_admin` |
| Contraseña | `USAC_ECYS_2026` |
| Token Base64 generado | `b3JiaXRuZXRfYWRtaW46VVNBQ19FQ1lTXzIwMjY=` |
| Formato de cabecera HTTP | `Authorization: Basic b3JiaXRuZXRfYWRtaW46VVNBQ19FQ1lTXzIwMjY=` |

---

## 9. Reportes de Visualización SVG

OrbitNet-NetCore genera tres tipos de reportes visuales en formato SVG directamente desde la memoria del servidor. Estos reportes reflejan el estado actual de las estructuras de datos en tiempo real y se renderizan dentro del navegador sin necesidad de descargar ningún archivo.

### 9.1 Reporte: Plano Satelital Activo (Dashboard Principal)

Este es el reporte principal visible en el Dashboard. Muestra el plano cartesiano completo con todos los nodos activos en la Matriz Dispersa, sus enlaces ortogonales y, cuando se ha trazado una ruta, el camino resaltado entre los nodos de origen y destino.

El diagrama se actualiza automáticamente al realizar cualquier operación sobre el plano (inserción, eliminación, carga XML o trazado de ruta).

---

<p align="center">
  <img src="./Imagenes/Trazo.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

### 9.2 Reporte: Mapa de Memoria de los TDAs (Memory Layout Map)

Este reporte genera una representación visual de la disposición física de los punteros lógicos en la memoria RAM del servidor para las estructuras de datos manuales. Cada nodo se dibuja como un registro con campos explícitos para el puntero anterior, el dato almacenado y el puntero siguiente.

Para acceder a este reporte, localice la opción **"Reporte de Memoria"** o **"Memory Layout"** en la sección de visualización de la barra de navegación.

---

<p align="center">
  <img src="./Imagenes/Matriz.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

### 9.3 Reporte: Trazabilidad de Ruta de Retransmisión (Relay Route Tracer)

Al completarse el enrutamiento de un paquete a través del plano, este reporte genera un grafo dirigido que muestra el histórico de saltos realizados por el paquete desde su nodo de origen hasta su destino. Los nodos que formaron parte de la ruta exitosa se colorean en verde, mientras que los nodos inactivos o fuera de cobertura se representan en rojo con bordes discontinuos.

---

<p align="center">
  <img src="./Imagenes/Trazo.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

### 9.4 Reporte: Matriz de Capacidad y Estado del Buffer Satelital

Este reporte presenta un mapa bidimensional que muestra el porcentaje de ocupación de la cola de prioridad (Buffer de Mensajes) de cada satélite activo en el plano. Cada celda del mapa corresponde a un satélite y contiene su identificador, su capacidad total y un indicador visual de saturación coloreado dinámicamente según el nivel de ocupación.

---

## 10. Bitácora de Auditoría

La bitácora de auditoría registra de forma automática y cronológica cada evento significativo que ocurre en el sistema. Funciona como un historial completo de todas las operaciones realizadas y es una herramienta fundamental para el diagnóstico de problemas y la verificación del comportamiento del simulador.

### Tipos de eventos registrados

| Severidad | Ícono / Color | Descripción |
|---|---|---|
| `INFO` | 🔵 Azul | Operaciones exitosas: inserciones, eliminaciones, cargas XML, peticiones HTTP completadas |
| `ALERT` | 🟡 Amarillo | Advertencias: intentos fallidos por campos vacíos, nodos no encontrados, peticiones sin credenciales |
| `ERROR` | 🔴 Rojo | Errores críticos: fallos de validación RegEx, colisiones de coordenadas, rollbacks de transacciones, errores de red |

### Dónde encontrar la bitácora

La bitácora se muestra en tiempo real dentro del Dashboard principal, generalmente en la parte inferior o en un panel lateral. Cada registro muestra la fecha y hora exacta del evento, su nivel de severidad y una descripción detallada.

---

<p align="center">
  <img src="./Imagenes/ConsolaAuditoria.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

### Cómo limpiar la bitácora

Si desea reiniciar el historial de eventos, haga clic en el botón **"Limpiar Bitácora"** o **"Purgar Logs"** disponible en el panel de auditoría. El sistema eliminará todos los registros existentes e insertará inmediatamente un nuevo evento de tipo `INFO` confirmando que la bitácora fue reiniciada.

> **Nota:** Limpiar la bitácora no afecta el estado del plano satelital ni los datos de la Matriz Dispersa o el Catálogo AVL. Es una operación exclusiva sobre el TDA de auditoría.

### Consultar la bitácora mediante la API REST

También es posible consultar el historial completo de logs en formato JSON realizando una petición GET al endpoint `/api/logs`. Consulte la Sección 11 para más detalles sobre el uso de la API.

---

## 11. Consulta de la API REST

OrbitNet-NetCore expone varios endpoints REST que permiten interactuar con el sistema de forma programática, sin necesidad de utilizar la interfaz web. Estos endpoints son especialmente útiles para pruebas automatizadas e integración entre las dos instancias del simulador.

### Endpoints disponibles

A continuación se describen los endpoints públicos más relevantes para el usuario:

#### Obtener todos los satélites del plano (Público)

```
GET http://localhost:5000/api/satelites
```

No requiere autenticación. Retorna un arreglo JSON con todos los nodos activos de la Matriz Dispersa, incluyendo su identificador, nombre, dirección IP y coordenadas.

---

<p align="center">
  <img src="./Imagenes/Consulta.png" alt="Arquitectura OrbitNet" width="600"/>
</p>

---

#### Obtener satélites con autenticación (Protegido)

```
GET http://localhost:5000/api/seguro/satelites
```

Requiere la cabecera de autenticación Basic Auth. Para probarla desde el cliente HTTP integrado en el Dashboard, use las credenciales `orbitnet_admin` / `USAC_ECYS_2026` y la URL `http://localhost:5000/api/seguro/satelites`.

#### Consultar la bitácora de auditoría

```
GET http://localhost:5000/api/logs
```

Retorna el historial completo de eventos registrados en formato JSON. No requiere autenticación.

#### Enrutamiento inter-satelital (Protegido)

```
POST http://localhost:5001/api/v1/space/relay
```

Requiere autenticación Basic Auth. Recibe el JSON del paquete a enrutar e inserta el mensaje en el buffer del satélite receptor de la instancia Sur.

---

## 12. Mensajes del Sistema — Guía de Interpretación

A lo largo del uso del sistema, el usuario encontrará mensajes de retroalimentación en la parte superior del Dashboard. La siguiente tabla facilita la interpretación de los mensajes más comunes:

| Mensaje | Tipo | Causa | Acción recomendada |
|---|---|---|---|
| `"Carga masiva completada con éxito (Commit)."` | ✅ Éxito | El archivo XML fue válido y todos los nodos se insertaron correctamente | Ninguna, continuar operando |
| `"Transacción de carga abortada (Rollback). Causa: El ID de Satélite 'X' no cumple con el patrón."` | ❌ Error | Un ID de satélite en el XML tiene formato incorrecto | Corregir el ID en el archivo XML y volver a cargar |
| `"Error sintáctico en IP 'X': Debe ser una dirección IPv4 válida."` | ❌ Error | Una dirección IP no cumple el formato de cuatro octetos | Verificar y corregir la IP en el archivo XML o formulario |
| `"Colisión detectada: ya existe un nodo en las coordenadas (X, Y)."` | ❌ Error | Se intentó insertar un nodo en coordenadas ya ocupadas | Usar coordenadas diferentes o eliminar el nodo existente primero |
| `"El identificador de satélite 'X' ya existe en el plano."` | ❌ Error | Se intentó insertar un ID ya registrado | Usar un ID diferente o verificar el archivo XML por duplicados |
| `"Ruta trazada con éxito. Saltos totales: N."` | ✅ Éxito | Se encontró un camino físico entre los nodos seleccionados | El plano SVG mostrará la ruta resaltada |
| `"No se pudo trazar una ruta física entre los satélites seleccionados."` | ⚠️ Alerta | No hay conexión física (punteros) entre los nodos seleccionados | Verificar que el plano tenga suficientes nodos enlazados |
| `"Petición Rechazada: 401 No Autorizado."` | ⚠️ Alerta | Las credenciales enviadas al endpoint protegido son incorrectas o están ausentes | Verificar usuario y contraseña en el formulario HTTP |
| `"Bitácora de auditoría reiniciada."` | ✅ Éxito | Se limpió correctamente la bitácora de logs | Ninguna |
| `"Se ha limpiado el plano espacial (Matriz y AVL)."` | ✅ Éxito | Se purgaron todos los nodos de ambas estructuras en RAM | Realizar una nueva carga XML para repoblar el plano |

---

## 13. Archivos de Configuración de Prueba

El sistema incluye tres archivos XML de prueba oficiales que permiten verificar el comportamiento correcto del motor de ingesta y las validaciones del sistema.

### Archivo 1: Carga Exitosa — Hemisferio Norte (Puerto 5000)

Utilice este archivo en la instancia del Hemisferio Norte. Contiene dos satélites ecuatoriales, dos satélites polares y una antena terrestre ubicada en Guatemala, todos con datos válidos.

```xml
<?xml version="1.0" encoding="utf-8"?>
<orbitnet>
  <constelaciones_ecuatoriales>
    <satelite id="SAT-ECU-0001">
      <nombre>Starlink-Norte-A</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
    <satelite id="SAT-ECU-0002">
      <nombre>Starlink-Norte-B</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
  </constelaciones_ecuatoriales>
  <orbitas_polares>
    <polar id="POLAR-NORTE-S">
      <satelite id="SAT-POL-1001">
        <nombre>Polar-Norte-1</nombre>
        <frecuencia>450.15</frecuencia>
      </satelite>
      <satelite id="SAT-POL-1002">
        <nombre>Polar-Norte-2</nombre>
        <frecuencia>450.30</frecuencia>
      </satelite>
    </polar>
  </orbitas_polares>
  <antenas_terrestres>
    <antena id="ANT-GTM-401">
      <nombre>Estación Central USAC Edificio T3</nombre>
      <coordenadas>14.5891,-90.5514</coordenadas>
      <ip_nodo>10.0.0.50</ip_nodo>
    </antena>
  </antenas_terrestres>
</orbitnet>
```

**Resultado esperado:** Carga exitosa. Se insertan 3 nodos en la Matriz Dispersa (2 satélites ecuatoriales + 1 antena) y 2 satélites en el Catálogo AVL. La bitácora mostrará un evento `INFO` de confirmación.

---

### Archivo 2: Carga Exitosa — Hemisferio Sur (Puerto 5001)

Utilice este archivo en la instancia del Hemisferio Sur. Cubre las operaciones de telecomunicaciones del Hemisferio Sur con una antena ubicada en Argentina.

```xml
<?xml version="1.0" encoding="utf-8"?>
<orbitnet>
  <constelaciones_ecuatoriales>
    <satelite id="SAT-ECU-0101">
      <nombre>Starlink-Sur-A</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
    <satelite id="SAT-ECU-0102">
      <nombre>Starlink-Sur-B</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
  </constelaciones_ecuatoriales>
  <orbitas_polares>
    <polar id="POLAR-SUR-S">
      <satelite id="SAT-POL-2001">
        <nombre>Polar-Sur-1</nombre>
        <frecuencia>480.15</frecuencia>
      </satelite>
      <satelite id="SAT-POL-2002">
        <nombre>Polar-Sur-2</nombre>
        <frecuencia>480.30</frecuencia>
      </satelite>
    </polar>
  </orbitas_polares>
  <antenas_terrestres>
    <antena id="ANT-ARG-501">
      <nombre>Subestación Regional Argentina</nombre>
      <coordenadas>-34.6037,-58.3816</coordenadas>
      <ip_nodo>10.0.0.90</ip_nodo>
    </antena>
  </antenas_terrestres>
</orbitnet>
```

**Resultado esperado:** Carga exitosa. Se insertan 3 nodos en la Matriz Dispersa y 2 satélites en el Catálogo AVL de la instancia Sur.

---

### Archivo 3: Escenario de Errores Controlados

Este archivo contiene fallos sintácticos deliberados para verificar que el sistema rechace correctamente la carga y ejecute el rollback sin corromper el estado de la RAM.

```xml
<?xml version="1.0" encoding="utf-8"?>
<orbitnet>
  <constelaciones_ecuatoriales>
    <satelite id="SAT-ECU-9001">
      <nombre>Satélite Guardián S1</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
    <!-- ❌ ID inválido: SAT-ECU-99 no tiene 4 dígitos -->
    <satelite id="SAT-ECU-99">
      <nombre>Satélite Defectuoso ID</nombre>
      <enlace_ip>127.0.0.1</enlace_ip>
    </satelite>
    <!-- ❌ IP inválida: 172.300.4.1 supera el valor máximo de octeto (255) -->
    <satelite id="SAT-ECU-9002">
      <nombre>Satélite Defectuoso IP</nombre>
      <enlace_ip>172.300.4.1</enlace_ip>
    </satelite>
  </constelaciones_ecuatoriales>
  <antenas_terrestres>
    <!-- ❌ Coordenadas inválidas: contienen caracteres no numéricos -->
    <antena id="ANT-GTM-405">
      <nombre>Antena Falla Coordenadas</nombre>
      <coordenadas>14.5891,-90.XYZ55</coordenadas>
      <ip_nodo>10.0.0.1</ip_nodo>
    </antena>
    <!-- ✅ Este elemento es válido, pero no se insertará por el rollback -->
    <antena id="ANT-GTM-406">
      <nombre>Antena Respaldo OK</nombre>
      <coordenadas>14.2012,-90.1114</coordenadas>
      <ip_nodo>10.0.0.2</ip_nodo>
    </antena>
  </antenas_terrestres>
</orbitnet>
```

**Resultado esperado:** La carga debe ser rechazada en cuanto el sistema encuentre el primer elemento inválido (`SAT-ECU-99`). El sistema mostrará un mensaje rojo de error indicando la causa exacta del fallo. El plano satelital debe permanecer exactamente igual que antes de intentar la carga.


---

*Fin del Manual de Usuario — OrbitNet-NetCore*
*Documento generado para el Proyecto Único del curso IPC2, Primer Semestre / Escuela de Vacaciones 2026.*
*Universidad de San Carlos de Guatemala — Facultad de Ingeniería — ECYS*
