# Manual Técnico — OrbitNet-NetCore
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
2. [Arquitectura General del Sistema](#2-arquitectura-general-del-sistema)
3. [Estructura del Repositorio y Solución .NET](#3-estructura-del-repositorio-y-solución-net)
4. [Tipos de Datos Abstractos (TDAs) Manuales](#4-tipos-de-datos-abstractos-tdas-manuales)
   - 4.1 [RedSatelitalPlano — Matriz Dispersa Ortogonal](#41-redsatelitalplano--matriz-dispersa-ortogonal)
   - 4.2 [ArbolSatelitesAvl — Árbol AVL Auto-Balanceado](#42-arbolsatelitesavl--árbol-avl-auto-balanceado)
   - 4.3 [BufferMensajes — Árbol Binario de Búsqueda (ABB)](#43-buffermensajes--árbol-binario-de-búsqueda-abb)
   - 4.4 [LogAuditoria — Lista Enlazada Simple](#44-logauditoria--lista-enlazada-simple)
5. [Módulo de Ingesta y Carga Masiva XML](#5-módulo-de-ingesta-y-carga-masiva-xml)
6. [Validaciones con Expresiones Regulares](#6-validaciones-con-expresiones-regulares)
7. [Capa de Controladores MVC](#7-capa-de-controladores-mvc)
8. [Protocolo REST Multi-Puerto y Comunicación Inter-Instancia](#8-protocolo-rest-multi-puerto-y-comunicación-inter-instancia)
9. [Seguridad: HTTP Basic Authentication](#9-seguridad-http-basic-authentication)
10. [Motor de Visualización con Graphviz](#10-motor-de-visualización-con-graphviz)
11. [Documentación de Endpoints de la API REST](#11-documentación-de-endpoints-de-la-api-rest)
12. [Estrategias de Seguridad OWASP](#12-estrategias-de-seguridad-owasp)

---

## 1. Introducción

OrbitNet-NetCore es un simulador web distribuido desarrollado íntegramente sobre la plataforma **.NET 8.0** y el lenguaje **C#**, cuyo propósito pedagógico es modelar el tránsito de paquetes de datos a través de una constelación satelital activa. El sistema administra múltiples nodos satelitales organizados en un plano espacial cartesiano, valida la integridad estructural de los datos mediante expresiones regulares, persiste toda la información exclusivamente en memoria RAM a través de estructuras de datos abstractas implementadas manualmente, y genera reportes visuales vectoriales SVG directamente desde el servidor web sin ningún tipo de persistencia en disco.

El simulador se despliega como **dos instancias simultáneas** de un servidor ASP.NET Core MVC en la misma máquina de desarrollo: una instancia representa la constelación del Hemisferio Norte (puerto `5000`) y la otra la del Hemisferio Sur (puerto `5001`). Cuando un paquete de datos debe cruzar de una constelación a la otra, el sistema ejecuta un salto HTTP seguro y autenticado entre ambas instancias.

Una restricción técnica fundamental del proyecto es que **está terminantemente prohibido el uso de cualquier clase o interfaz perteneciente a `System.Collections` o `System.Collections.Generic`**. Todas las estructuras de almacenamiento se construyen utilizando nodos autorreferenciados enlazados manualmente mediante punteros de tipo referencia en C#.

---

## 2. Arquitectura General del Sistema

El sistema sigue una arquitectura de **N-Capas bajo el patrón Modelo-Vista-Controlador (MVC)**, con dependencia unidireccional estricta: la capa de presentación invoca a los controladores, los controladores delegan en los servicios, y los servicios interactúan con las estructuras de datos en RAM. Ninguna capa inferior puede invocar directamente a una capa superior.


<p align="center">
  <img src="./Diagramas/Arquitectura.svg" alt="Arquitectura OrbitNet" width="300"/>
</p>


### Estado Global Compartido: `MemoriaPlano`

La clase estática `MemoriaPlano` (ubicada en `OrbitNet.WebNorte/Models/MemoriaPlano.cs`) actúa como el repositorio central de estado en tiempo de ejecución. Todos los controladores acceden al estado de la aplicación a través de esta única clase, garantizando coherencia entre peticiones HTTP consecutivas.

<p align="center">
  <img src="./Diagramas/Diagrama7.svg" alt="Núcleo del Sistema" width="300"/>
</p>

```csharp
public static class MemoriaPlano
{
    public static RedSatelitalPlano Matriz   { get; } = new RedSatelitalPlano();
    public static LogAuditoria      Logs     { get; } = new LogAuditoria();
    public static ArbolSatelitesAvl Catalogo { get; } = new ArbolSatelitesAvl();
    public static MatrixNode[]?     RutaActiva { get; set; } = null;
}
```

---

## 3. Estructura del Repositorio y Solución .NET

El repositorio sigue la convención de nomenclatura exigida por la cátedra y está organizado como una solución multi-proyecto de .NET:

```
IPC2_Proyecto_2026_Grupo[N]/
│
├── OrbitNet-NetCore.sln
│
├── OrbitNet.Core/                    ← Biblioteca de clases: Nodos y Estructuras
│   ├── Estructuras/
│   │   ├── RedSatelitalPlano.cs      (Matriz Dispersa Ortogonal)
│   │   ├── ArbolSatelitesAvl.cs      (Árbol AVL)
│   │   ├── BufferMensajes.cs         (ABB de Prioridades)
│   │   └── LogAuditoria.cs           (Lista Enlazada Simple)
│   ├── Nodos/
│   │   ├── MatrixNode.cs
│   │   ├── HeaderNode.cs
│   │   ├── AvlNode.cs
│   │   ├── AbbNode.cs
│   │   └── LogNode.cs
│   ├── Modelos/
│   │   └── Satelite.cs
│   └── Interfaces/
│       └── IAbstractCollection.cs
│
├── OrbitNet.Services/                ← Biblioteca de clases: Servicios y Algoritmos
│   ├── Algoritmos/
│   │   └── EnrutadorOrtogonal.cs
│   ├── Dtos/
│   │   └── SateliteDto.cs
│   ├── Ingesta/
│   │   └── XmlProcessorService.cs
│   ├── Validaciones/
│   │   └── RegexValidator.cs
│   └── Visualizacion/
│       └── GraphvizCompilador.cs
│
├── OrbitNet.WebNorte/                ← Aplicación ASP.NET Core MVC (Puerto 5000)
│   ├── Attributes/
│   │   └── BasicAuthorizeAttribute.cs
│   ├── Controllers/
│   │   ├── ApiController.cs
│   │   ├── HomeController.cs
│   │   ├── HttpClienteController.cs
│   │   ├── LogsController.cs
│   │   ├── RouteController.cs
│   │   ├── SateliteController.cs
│   │   └── XmlController.cs
│   ├── Models/
│   │   ├── DashBoardViewModel.cs
│   │   └── MemoriaPlano.cs
│   └── Views/
│
├── OrbitNet.WebSur/                  ← Aplicación ASP.NET Core MVC (Puerto 5001)
│   └── (estructura simétrica a WebNorte)
│
└── tests/                            ← Proyecto de pruebas xUnit/NUnit
```

---

## 4. Tipos de Datos Abstractos (TDAs) Manuales

Todas las estructuras implementan la interfaz base de auditoría `IAbstractCollection`, que exige exponer el conteo de elementos, el método de limpieza y una propiedad de verificación de vacío:

```csharp
public interface IAbstractCollection
{
    int  Count   { get; }
    void Clear();
    bool IsEmpty { get; }
}
```

---

### 4.1 RedSatelitalPlano — Matriz Dispersa Ortogonal

**Ubicación:** `OrbitNet.Core/Estructuras/RedSatelitalPlano.cs`

#### Descripción

Esta estructura modela la topología espacial de la constelación como un plano cartesiano de latitudes y longitudes enteras. A diferencia de un arreglo bidimensional convencional que reserva memoria para todas las celdas posibles, la matriz dispersa ortogonal solo asigna espacio en el Heap para los nodos que contienen datos reales, enlazándolos a través de nodos cabecera de fila y columna.

#### Nodos que la conforman

**`HeaderNode`** — Nodo Cabecera de Eje

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Index` | `int` | Coordenada numérica del eje (fila o columna) |
| `Next` | `HeaderNode` | Puntero al siguiente cabecera en el mismo eje |
| `Access` | `MatrixNode` | Puntero al primer nodo de datos en esta fila/columna |

**`MatrixNode`** — Nodo de Datos (Satélite o Antena)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Row` | `int` | Coordenada de fila (latitud redondeada) |
| `Col` | `int` | Coordenada de columna (longitud redondeada) |
| `Id` | `string` | Identificador único del nodo (ej. `SAT-ECU-0001`) |
| `Name` | `string` | Nombre descriptivo del nodo |
| `IpAddress` | `string` | Dirección IPv4 del nodo |
| `Up` | `MatrixNode` | Puntero al nodo inmediatamente superior en la columna |
| `Down` | `MatrixNode` | Puntero al nodo inmediatamente inferior en la columna |
| `Left` | `MatrixNode` | Puntero al nodo inmediatamente a la izquierda en la fila |
| `Right` | `MatrixNode` | Puntero al nodo inmediatamente a la derecha en la fila |
| `Buffer` | `BufferMensajes` | Instancia del buffer de mensajes propio del satélite |

#### Representación lógica en memoria

<p align="center">
  <img src="./Diagramas/Diagrama2.svg" alt="Estructura de la Matriz Dispersa Ortogonal en RAM" width="450"/>
</p>

#### Interfaz pública de operaciones

```csharp
// Inserta un nodo creando cabeceras si no existen, en orden creciente por coordenada.
void Insert(int row, int col, string id, string nombre, string ip);

// Elimina el nodo físicamente y reconecta los cuatro punteros ortogonales adyacentes.
void Delete(int row, int col);

// Retorna el nodo ubicado en (row, col) en tiempo O(r + c).
MatrixNode? Search(int row, int col);

// Retorna el nodo cuyo Id coincida, recorriendo todas las filas.
MatrixNode? BuscarPorId(string id);

// Retorna un arreglo nativo con todos los nodos de datos activos.
MatrixNode[] ObtenerTodosLosNodos();

// Genera el código DOT de Graphviz para representar el estado actual de la matriz.
string GenerarCodigoDot(MatrixNode[]? rutaActiva);
```

#### Análisis de complejidad

| Operación | Complejidad |
|-----------|-------------|
| `Insert` | O(r + c) para encontrar la posición + O(1) para el enlace |
| `Delete` | O(r + c) para encontrar el nodo + O(1) para reconectar |
| `Search` | O(r + c) donde r = nodos en filas cabecera, c = nodos en columnas cabecera |
| `ObtenerTodosLosNodos` | O(n) donde n = total de nodos en la matriz |

---

### 4.2 ArbolSatelitesAvl — Árbol AVL Auto-Balanceado

**Ubicación:** `OrbitNet.Core/Estructuras/ArbolSatelitesAvl.cs`

#### Descripción

Funciona como el catálogo global de satélites polares activos. Al mantenerse auto-balanceado, garantiza que las operaciones de búsqueda, inserción y eliminación nunca superen O(log n) comparaciones, independientemente del orden en que se inserten los elementos.

#### Nodo AVL

**`AvlNode`**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `SatelliteId` | `string` | Clave de búsqueda única |
| `Name` | `string` | Nombre del satélite |
| `Frequency` | `double` | Frecuencia de operación en MHz |
| `Height` | `int` | Altura del subárbol enraizado en este nodo |
| `LeftChild` | `AvlNode` | Hijo izquierdo |
| `RightChild` | `AvlNode` | Hijo derecho |

#### Factor de equilibrio y rotaciones

El factor de equilibrio de un nodo se calcula como:

```
FE = H(subárbol derecho) − H(subárbol izquierdo)
```

Un nodo se considera desbalanceado cuando `|FE| >= 2`. El sistema implementa las cuatro rotaciones fundamentales para restaurar el balance:

| Caso | Tipo de Rotación | Condición de Disparo |
|------|-----------------|----------------------|
| LL | Rotación simple a la derecha | Inserción en el hijo izquierdo del subárbol izquierdo |
| RR | Rotación simple a la izquierda | Inserción en el hijo derecho del subárbol derecho |
| LR | Rotación doble Izquierda-Derecha | Inserción en el hijo derecho del subárbol izquierdo |
| RL | Rotación doble Derecha-Izquierda | Inserción en el hijo izquierdo del subárbol derecho |

```
Rotación Simple Derecha (LL):        Rotación Simple Izquierda (RR):

       C                  B                A                  B
      / \      ═══►      / \              / \      ═══►      / \
     B  T3              A   C           T1   B             A   C
    / \                / \                  / \
   A  T2             T1  T2               T2  T3
```

<p align="center">
  <img src="./Diagramas/Diagrama3.svg" alt="Árbol AVL de Satélites con Rotaciones" width="650"/>
</p>

#### Interfaz pública de operaciones

```csharp
// Inserta un satélite y ejecuta rotaciones de balanceo de ser necesario.
void Insertar(Satelite satelite);

// Busca un nodo por su SatelliteId en tiempo O(log n).
AvlNode? Buscar(string id);

// Recorre el árbol en inorden y devuelve los nodos ordenados alfabéticamente por ID.
AvlNode[] RecorridoInorden();

// Limpia el árbol poniendo la raíz en null, permitiendo la recolección de basura.
void Limpiar();
```

---

### 4.3 BufferMensajes — Árbol Binario de Búsqueda (ABB)

**Ubicación:** `OrbitNet.Core/Estructuras/BufferMensajes.cs`

#### Descripción

Cada nodo de la matriz dispersa posee una instancia de esta estructura. Implementa un Árbol Binario de Búsqueda ordenado por nivel de prioridad del paquete, simulando una cola de prioridad no lineal. Los mensajes de prioridad mayor se ubican en la rama derecha del árbol para ser despachados primero.

#### Nodo ABB

**`AbbNode`**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `HexCode` | `string` | Código hexadecimal único del paquete |
| `EmisorId` | `string` | ID del satélite de origen |
| `DestIp` | `string` | IP del nodo terrestre de destino |
| `Priority` | `int` | Nivel de prioridad (1 = mínima, 5 = Alerta Crítica) |
| `Content` | `string` | Cuerpo del mensaje |
| `Left` | `AbbNode` | Enlace al hijo de menor prioridad |
| `Right` | `AbbNode` | Enlace al hijo de mayor prioridad |

#### Interfaz pública de operaciones

```csharp
// Inserta el paquete en el ABB según su prioridad (mayor prioridad → derecha).
void Enqueue(AbbNode packet);

// Extrae el mensaje de máxima prioridad (nodo más a la derecha) y reestructura el árbol.
AbbNode? Dequeue();

// Recorrido inorden que retorna los mensajes ordenados de menor a mayor prioridad.
void TraverseInOrder(Action<AbbNode> accion);
```

#### Esquema de ordenación

<p align="center">
  <img src="./Diagramas/Diagrama4.svg" alt="Árbol Binario ABB como Cola de Prioridad" width="650"/>
</p>

```
                   [P=3]
                  /     \
               [P=1]   [P=4]
                          \
                          [P=5]  ← Dequeue() extrae este nodo primero
```

---

### 4.4 LogAuditoria — Lista Enlazada Simple

**Ubicación:** `OrbitNet.Core/Estructuras/LogAuditoria.cs`

#### Descripción

Registra de forma cronológica, lineal e inmutable cada evento del sistema: operaciones exitosas, advertencias de validación, errores de ingesta e intentos de acceso no autorizado. Utiliza un puntero de cola (`Tail`) para garantizar inserciones en tiempo O(1).

#### Nodo de Log

**`LogNode`**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Timestamp` | `DateTime` | Fecha y hora exacta del evento |
| `Severity` | `string` | Gravedad: `INFO`, `ALERT`, `ERROR` |
| `Message` | `string` | Descripción detallada del suceso |
| `Next` | `LogNode` | Puntero al siguiente registro en la lista |

#### Interfaz pública de operaciones

```csharp
// Inserta un nuevo evento al final de la lista en O(1) usando el puntero Tail.
void WriteEvent(string severity, string message);

// Filtra registros cuyo mensaje coincide con el patrón RegEx proporcionado.
string SearchLogRegex(string pattern);

// Retorna un arreglo nativo con todos los LogNode de la lista.
LogNode[] ObtenerTodosLosNodos();
```

---

## 5. Módulo de Ingesta y Carga Masiva XML

**Ubicación:** `OrbitNet.WebNorte/Controllers/XmlController.cs`

El módulo de ingesta implementa un esquema **transaccional atómico**: si cualquier elemento del archivo XML contiene datos inválidos, la operación completa se cancela y las estructuras en RAM permanecen inalteradas (rollback). Solo si la validación de todos los elementos es exitosa se procede a la escritura definitiva en los TDAs (commit).

### Flujo de procesamiento

<p align="center">
  <img src="./Diagramas/Diagrama5.svg" alt="Flujo de Ingesta Transaccional XML" width="650"/>
</p>


### Atomicidad mediante listas enlazadas temporales

Para garantizar la atomicidad sin usar colecciones nativas, el controlador utiliza **nodos enlazados temporales privados** (`SateliteTemporal` y `NodoAvlTemporal`) que acumulan los datos validados en memoria. Estos nodos internos son descartados o consolidados según el resultado de la transacción.

```csharp
// Nodo temporal para satélites ecuatoriales (Matriz Dispersa)
private class SateliteTemporal
{
    public int    Fila      { get; }
    public int    Columna   { get; }
    public string Id        { get; }
    public string Nombre    { get; }
    public string IpAddress { get; }
    public SateliteTemporal? Siguiente { get; set; }
    // ...
}

// Nodo temporal para satélites polares (Árbol AVL)
private class NodoAvlTemporal
{
    public string Id         { get; }
    public string Nombre     { get; }
    public double Frecuencia { get; }
    public NodoAvlTemporal? Siguiente { get; set; }
    // ...
}
```

### Mapeo de entidades XML a TDAs

| Elemento XML | XPath utilizado | TDA destino | Coordenada |
|---|---|---|---|
| Satélite ecuatorial | `//constelaciones_ecuatoriales/satelite` | `RedSatelitalPlano` | Fila = 0, Col = últimos 4 dígitos del ID |
| Satélite polar | `//orbitas_polares/polar/satelite` | `ArbolSatelitesAvl` | No aplica (árbol) |
| Antena terrestre | `//antenas_terrestres/antena` | `RedSatelitalPlano` | Fila = Round(Lat), Col = Round(Long) |

---

## 6. Validaciones con Expresiones Regulares

**Ubicación:** `OrbitNet.Services/Validaciones/RegexValidator.cs`

Todas las validaciones sintácticas están centralizadas en la clase estática `RegexValidator`. Esta centralización garantiza consistencia entre el `XmlController` y el `SateliteController`, ya que ambos utilizan los mismos patrones de validación.

| Atributo | Método de validación | Expresión Regular | Ejemplo válido |
|---|---|---|---|
| ID de Satélite | `ValidarSateliteId(string id)` | `^SAT-(ECU\|POL)-\d{4}$` | `SAT-ECU-0012` |
| Dirección IPv4 | `ValidarIPv4(string ip)` | `^(?:(?:25[0-5]\|2[0-4][0-9]\|[01]?\d?\d)\.){3}(?:25[0-5]\|2[0-4][0-9]\|[01]?\d?\d)$` | `10.0.0.50` |
| Coordenadas geográficas | `ValidarCoordenadas(string coords)` | `^-?\d{1,2}\.\d{4,6},-?\d{1,3}\.\d{4,6}$` | `14.5891,-90.5514` |
| ID de Antena | `ValidarAntenaId(string id)` | `^ANT-[A-Z]{3}-\d{3,4}$` | `ANT-GTM-401` |
| Frecuencia | `ValidarFrecuencia(string freq)` | Verificación numérica positiva (`double.TryParse`) | `450.15` |

### Comportamiento ante fallo de validación

Ante cualquier fallo de validación, el sistema ejecuta las siguientes acciones de forma secuencial:

1. Se establece la bandera `transaccionExitosa = false`.
2. Se registra el motivo exacto del fallo en `causaFallo`.
3. Se interrumpe el procesamiento del archivo (no se continúa con el siguiente elemento).
4. Se ejecuta el rollback implícito (las listas temporales de nodos se descartan).
5. Se escribe un evento de severidad `ERROR` en el TDA `LogAuditoria`.
6. Se informa al usuario mediante `TempData["ErrorMessage"]` con el detalle del fallo.

---

## 7. Capa de Controladores MVC

### `HomeController`

Controlador principal del dashboard. Construye el `DashBoardViewModel` que agrega el estado completo de todos los TDAs y el SVG del diagrama actual para su renderizado en la vista principal.

**Métodos:**
- `GET /` — `Index()`: Recupera mensajes de TempData, inicializa los logs si la bitácora está vacía, invoca a `GraphvizCompilador` para generar el SVG del estado actual y retorna el ViewModel al dashboard.

### `ApiController`

Expone el estado interno del sistema como endpoints REST en formato JSON.

**Métodos:**
- `GET /api/satelites` — `ObtenerSatelites()`: Retorna todos los nodos de la matriz como un arreglo de `SateliteDto[]`, construido manualmente sin usar listas genéricas.
- `GET /api/seguro/satelites` — `ObtenerSatelitesSeguro()`: Idéntico al anterior pero protegido con el atributo `[BasicAuthorize]`.
- `GET /api/logs` — `ObtenerLogs()`: Retorna el arreglo de `LogNode[]` de la bitácora de auditoría.

### `SateliteController`

Gestiona las operaciones manuales sobre nodos individuales de la Matriz Dispersa.

**Métodos:**
- `POST /Satelite/InsertarNodo` — Valida ID, IPv4 y colisiones antes de invocar `MemoriaPlano.Matriz.Insert()`.
- `POST /Satelite/EliminarNodo` — Verifica la existencia del nodo en `(row, col)` y ejecuta `MemoriaPlano.Matriz.Delete()`, reconectando los punteros ortogonales.
- `POST /Satelite/LimpiarMatriz` — Limpia tanto la Matriz Dispersa como el Catálogo AVL de forma simultánea.

### `XmlController`

Gestiona la ingesta transaccional de archivos XML de configuración.

**Métodos:**
- `POST /Xml/CargarXml` — Recibe el `IFormFile`, ejecuta el flujo de validación y la lógica de commit/rollback descrita en la sección 5.

### `RouteController`

Gestiona el cálculo y visualización de rutas de saltos ortogonales.

**Métodos:**
- `POST /Route/Trazar` — Invoca al servicio `EnrutadorOrtogonal.EncontrarRuta()` y almacena el resultado en `MemoriaPlano.RutaActiva` para su destaque visual en el SVG.
- `POST /Route/Limpiar` — Restablece `MemoriaPlano.RutaActiva = null`.

### `LogsController`

**Métodos:**
- `POST /Logs/LimpiarLogs` — Ejecuta `MemoriaPlano.Logs.Clear()` e inserta inmediatamente un evento `INFO` indicando el reinicio de la bitácora.

### `HttpClienteController`

Gestiona las peticiones HTTP salientes hacia otras instancias u APIs externas.

**Métodos:**
- `POST /HttpCliente/ConsultarApi` — Construye un `HttpRequestMessage` individual por petición (para evitar colisiones de cabeceras en el `HttpClient` estático compartido), inyecta las credenciales Basic Auth si se proporcionan, ejecuta la petición de forma asíncrona y registra cada etapa en la bitácora.

---

## 8. Protocolo REST Multi-Puerto y Comunicación Inter-Instancia

El simulador distribuido requiere el despliegue simultáneo de dos procesos de servidor ASP.NET Core en la misma máquina. El flujo de comunicación entre instancias se activa cuando un paquete destinado a una antena del Hemisferio Sur es procesado por la instancia del Norte.

```mermaid
sequenceDiagram
    title Protocolo REST Multi-Puerto - Enrutamiento Distribuido con Basic Auth

    participant U as Usuario (Navegador)
    participant N as Instancia Norte :5000
    participant S as Instancia Sur :5001

    U->>N: POST /Route/Trazar (origenId, destinoId)
    Note over N: EnrutadorOrtogonal.EncontrarRuta()<br/>Calcula saltos en Matriz Dispersa
    activate N
    N->>N: ¿Destino IP pertenece a constelación Sur?
    Note over N: Serializa paquete a JSON<br/>Codifica credenciales en Base64
    N->>S: HTTP POST /api/v1/space/relay<br/>Header: Authorization: Basic b3JiaX...
    Note over S: BasicAuthorizeAttribute<br/>Decodifica y valida credenciales
    activate S
    S->>S: BufferMensajes.Enqueue(paquete) en satélite receptor
    S-->>N: 201 Creado — {status: Routed}
    deactivate S
    N-->>U: Redirect → Dashboard con mensaje de éxito
    deactivate N
```

El `HttpClient` utilizado en `HttpClienteController` es una instancia estática (`static readonly`) compartida entre todas las peticiones, siguiendo la recomendación de Microsoft para evitar el agotamiento de sockets de red bajo cargas concurrentes.

---

## 9. Seguridad: HTTP Basic Authentication

**Ubicación:** `OrbitNet.WebNorte/Attributes/BasicAuthorizeAttribute.cs`

Todas las comunicaciones REST inter-instancia se autentican mediante el estándar HTTP Basic Authentication. El atributo personalizado `[BasicAuthorize]` puede decorar cualquier acción o controlador para protegerlo.

### Construcción de la cabecera de autenticación (lado emisor)

```csharp
string credenciales    = "orbitnet_admin:USAC_ECYS_2026";
string base64          = Convert.ToBase64String(Encoding.UTF8.GetBytes(credenciales));
// Resultado: b3JiaXRuZXRfYWRtaW46VVNBQ19FQ1lTXzIwMjY=

peticion.Headers.Authorization =
    new AuthenticationHeaderValue("Basic", base64);
// Cabecera final: Authorization: Basic b3JiaXRuZXRfYWRtaW46VVNBQ19FQ1lTXzIwMjY=
```

### Validación de credenciales (lado receptor)

El atributo `BasicAuthorizeAttribute` intercepta la petición antes de que llegue al método del controlador. Extrae el valor de la cabecera `Authorization`, decodifica el token Base64, separa las credenciales por el delimitador `:` y las compara con los valores esperados. Si las credenciales son inválidas o la cabecera está ausente, retorna inmediatamente una respuesta `401 Unauthorized`.

---

## 10. Motor de Visualización con Graphviz

**Ubicación:** `OrbitNet.Services/Visualizacion/GraphvizCompilador.cs`

### Arquitectura de renderizado sin persistencia en disco

El sistema está diseñado para generar visualizaciones SVG completamente en memoria, sin escribir ningún archivo intermedio en disco. El flujo es el siguiente:

1. El TDA `RedSatelitalPlano` genera una cadena de código DOT que describe el estado actual de la matriz.
2. `GraphvizCompilador.CompilarDotASvg()` lanza un subproceso del sistema operativo (`dot -Tsvg`).
3. El código DOT se escribe directamente al `StandardInput` del subproceso.
4. La salida SVG se lee del `StandardOutput` del subproceso como una cadena de texto.
5. El SVG resultante se inyecta en el ViewModel y se renderiza en la vista con `@Html.Raw(Model.SvgDiagrama)`.

### Prevención de deadlocks en la redirección de flujos

Un error crítico habitual al usar subprocesos con `RedirectStandardOutput` es el bloqueo mutuo (*deadlock*): si el subproceso escribe más datos de los que caben en el buffer de la tubería del sistema operativo mientras el proceso padre espera con `WaitForExit()`, ambos quedan bloqueados indefinidamente. La solución implementada es cerrar el `StandardInput` antes de leer el `StandardOutput`:

```csharp
public static string CompilarDotASvg(string dotSourceCode)
{
    using (Process process = new Process())
    {
        process.StartInfo.FileName               = "dot";
        process.StartInfo.Arguments              = "-Tsvg";
        process.StartInfo.UseShellExecute        = false;
        process.StartInfo.RedirectStandardInput  = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError  = true;
        process.StartInfo.CreateNoWindow         = true;
        process.Start();

        // Cerrar el canal de entrada indica a dot.exe que la entrada finalizó,
        // liberando el subproceso para que escriba toda su salida SVG.
        using (StreamWriter writer = process.StandardInput)
        {
            writer.Write(dotSourceCode);
            writer.Flush();
        }

        // Leer la salida ANTES de llamar WaitForExit previene el deadlock.
        string svgOutput   = process.StandardOutput.ReadToEnd();
        string errorOutput = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"Error de compilación Graphviz: {errorOutput}");

        return svgOutput;
    }
}
```

### Reportes SVG generados

| Reporte | Descripción | Técnica DOT |
|---|---|---|
| **Memory Layout Map** | Mapa físico de punteros de los TDAs en RAM | `shape=record` con campos `prev`, `data`, `next` |
| **Relay Route Tracer** | Grafo dirigido de la ruta de retransmisión del paquete | Nodos activos en verde (`#2ECC71`), inactivos en rojo (`#E74C3C`), ruta óptima con `penwidth=3.0` |
| **Buffer Capacity Matrix** | Mapa de ocupación de las colas de prioridad de todos los satélites | Etiquetas HTML con tablas (`label=<<table>...</table>>`) |

---

## 11. Documentación de Endpoints de la API REST

### Endpoint 1: Ingesta de Configuración XML

| Propiedad | Valor |
|---|---|
| **Método HTTP** | `POST` |
| **Ruta** | `/api/v1/space/config` |
| **Autenticación** | No requerida |

**Cuerpo de la petición (JSON):**
```json
{
  "xml_data": "<?xml version=\"1.0\"?><orbitnet>...</orbitnet>"
}
```

**Respuesta exitosa — 200 OK:**
```json
{
  "status": "Success",
  "message": "Configuración cargada exitosamente en RAM. Nodos procesados: 4.",
  "timestamp": "2026-06-01T17:56:00Z"
}
```

**Respuesta de error — 400 Bad Request:**
```json
{
  "status": "Error",
  "error_code": "XML_SCHEMA_VIOLATION",
  "details": "El ID de Satelite 'SAT-ECU-99' no cumple con el formato RegEx exigido."
}
```

---

### Endpoint 2: Transferencia y Enrutamiento Inter-Satelital

| Propiedad | Valor |
|---|---|
| **Método HTTP** | `POST` |
| **Ruta** | `/api/v1/space/relay` |
| **Autenticación** | **HTTP Basic Auth requerida** |

**Cuerpo de la petición (JSON):**
```json
{
  "codigo_hex": "A19F",
  "emisor_id": "SAT-ECU-0012",
  "destino_ip": "10.0.0.50",
  "prioridad": 5,
  "contenido": "Alerta de tsunami detectada en boya de superficie."
}
```

**Respuesta exitosa — 201 Created:**
```json
{
  "status": "Routed",
  "message": "Mensaje insertado con éxito en el buffer de prioridad del satélite receptor SAT-POL-1001.",
  "queue_occupancy_percentage": 40.0
}
```

**Respuesta de error de credenciales — 401 Unauthorized:**
```json
{
  "status": "Unauthorized",
  "details": "Acceso restringido. Cabecera HTTP Basic Auth inválida o ausente."
}
```

---

### Endpoint 3: Avance de Simulación por Ticks

| Propiedad | Valor |
|---|---|
| **Método HTTP** | `POST` |
| **Ruta** | `/api/v1/space/simulation/step` |
| **Autenticación** | No requerida |

**Cuerpo de la petición (JSON):**
```json
{
  "ticks": 1
}
```

**Respuesta exitosa — 200 OK:**
```json
{
  "status": "Simulated",
  "current_tick": 45,
  "events_processed": 3,
  "details": "Órbitas rotadas exitosamente. Se ejecutaron 2 saltos lógicos."
}
```

---

### Endpoint 4: Listado de Satélites (Público)

| Propiedad | Valor |
|---|---|
| **Método HTTP** | `GET` |
| **Ruta** | `/api/satelites` |
| **Autenticación** | No requerida |

**Respuesta exitosa — 200 OK (arreglo JSON):**
```json
[
  { "fila": 0, "columna": 1, "id": "SAT-ECU-0001", "nombre": "Starlink-Norte-A", "ipAddress": "127.0.0.1" },
  { "fila": 15, "columna": -91, "id": "ANT-GTM-401", "nombre": "Estación Central USAC", "ipAddress": "10.0.0.50" }
]
```

---

### Endpoint 5: Listado de Satélites (Protegido)

| Propiedad | Valor |
|---|---|
| **Método HTTP** | `GET` |
| **Ruta** | `/api/seguro/satelites` |
| **Autenticación** | **HTTP Basic Auth requerida** |

Respuesta idéntica al Endpoint 4 cuando las credenciales son válidas. Retorna `401 Unauthorized` en caso contrario.

---

### Endpoint 6: Bitácora de Auditoría

| Propiedad | Valor |
|---|---|
| **Método HTTP** | `GET` |
| **Ruta** | `/api/logs` |
| **Autenticación** | No requerida |

**Respuesta exitosa — 200 OK (arreglo JSON):**
```json
[
  { "timestamp": "2026-06-01T12:00:00Z", "severity": "INFO", "message": "Sistema inicializado." },
  { "timestamp": "2026-06-01T12:01:05Z", "severity": "ERROR", "message": "ID de satélite 'SAT-ECU-99' inválido." }
]
```

---

## 12. Estrategias de Seguridad OWASP

### Mitigación de XXE (XML External Entity Injection)

Al procesar el archivo XML de configuración, se deshabilita explícitamente la resolución de entidades externas y el procesamiento DTD. Esto previene ataques de tipo XXE que podrían permitir a un atacante leer archivos del sistema o ejecutar peticiones de red no autorizadas desde el servidor.

```csharp
// Doble mitigación: a nivel de XmlReaderSettings y a nivel de XmlDocument
XmlReaderSettings settings = new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver   = null
};

XmlDocument doc = new XmlDocument();
doc.XmlResolver = null;  // Deshabilita resolución de entidades externas
```

### Prevención de XSS en el renderizado SVG

Dado que el SVG generado por Graphviz se inyecta en bruto en las vistas Razor mediante `@Html.Raw()`, el backend sanitiza el código DOT antes de enviarlo al compilador. Se eliminan o escapan atributos HTML embebidos dentro de las etiquetas de nodos Graphviz para neutralizar la posibilidad de inyectar scripts maliciosos en el SVG renderizado.

### Autenticación de comunicaciones inter-instancia

Toda petición al endpoint `/api/v1/space/relay` debe incluir la cabecera `Authorization: Basic <token>`. El atributo `[BasicAuthorize]` rechaza sin procesar cualquier petición que no cumpla este requisito, retornando `401 Unauthorized` antes de que la lógica de negocio sea invocada.

### Validación de entradas con RegEx antes de toda operación

Ningún dato proveniente del usuario o de un archivo externo se inserta directamente en las estructuras de memoria. Todos los campos pasan por las validaciones de `RegexValidator` antes de que se instancie cualquier nodo. Esto previene que datos malformados o maliciosos corrompan la integridad de las estructuras de datos en RAM.

---

*Fin del Manual Técnico — OrbitNet-NetCore*
*Documento generado para el Proyecto Único del curso IPC2, Primer Semestre / Escuela de Vacaciones 2026.*
*Universidad de San Carlos de Guatemala — Facultad de Ingeniería — ECYS*
