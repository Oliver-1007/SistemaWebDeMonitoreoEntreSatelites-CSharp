# 🛰️ OrbitNet-NetCore

Sistema web de **monitoreo, enrutamiento y simulación de conexiones satelitales distribuidas** desarrollado en **C# / ASP.NET Core MVC (.NET 8.0)**. Simula una constelación satelital en tiempo real mediante dos instancias web independientes que se comunican entre sí vía **REST con HTTP Basic Authentication**, gestionando el tránsito de paquetes de datos a través de estructuras de datos abstractas (TDAs) construidas completamente a mano, sin colecciones nativas del framework.

Proyecto desarrollado como **Proyecto Único** del curso **Introducción a la Programación y Computación II**, Escuela de Ciencias y Sistemas, Facultad de Ingeniería, Universidad de San Carlos de Guatemala.

---

## 📋 Descripción

**OrbitNet-NetCore** modela una red de telecomunicaciones satelitales distribuidas: satélites de órbita baja y media (constelaciones polares y ecuatoriales) que enrutan paquetes de datos hacia estaciones terrestres receptoras, buscando la ruta óptima de descarga.

El sistema se despliega como **dos instancias simultáneas** de un servidor ASP.NET Core MVC en la misma máquina:

- **OrbitNet.WebNorte** — Hemisferio Norte — `puerto 5000`
- **OrbitNet.WebSur** — Hemisferio Sur — `puerto 5001`

Cuando un paquete debe salir de la red de una instancia hacia una estación terrestre configurada en la instancia hermana, el sistema realiza un salto HTTP síncrono autenticado entre ambos servidores.

Toda la persistencia vive **en memoria RAM**, mediante estructuras de datos autorreferenciadas diseñadas manualmente — sin usar `System.Collections` ni `System.Collections.Generic`.

---

## 🏗️ Arquitectura

Patrón **Modelo-Vista-Controlador (MVC)** con flujo de dependencia estrictamente unidireccional:

```
Vista (Razor) → Controlador (API/MVC) → Capa de Servicio → Capa de Persistencia (TDAs en RAM)
```

| Capa | Responsabilidad |
|---|---|
| **Presentación** | Vistas Razor con Bootstrap 5, carga masiva, simulación interactiva por *ticks*, inyección de SVG en caliente |
| **Controladores** | API REST, serialización/deserialización JSON, parseo XML vía XPath, validación de solicitudes |
| **Lógica de negocio** | Motor de simulación (Tick Processor, Orbital Rotator), servicio de enrutamiento distribuido (`HttpClient` + Basic Auth), compilador DOT → SVG con Graphviz |
| **Persistencia** | TDAs manuales autorreferenciados en memoria RAM, sin bases de datos ni colecciones dinámicas nativas |

---

## 🧠 Estructuras de Datos Abstractas (TDAs)

Todas las estructuras se implementaron manualmente con punteros de tipo objeto, cumpliendo la interfaz base `IAbstractCollection`.

| TDA | Estructura | Uso |
|---|---|---|
| **RedSatelitalPlano** | Matriz dispersa ortogonal bidireccional | Topología espacial (latitud/longitud) de satélites y antenas — inserción, búsqueda y eliminación con reconexión quirúrgica de punteros |
| **RegistroSatelites** | Árbol AVL auto-balanceado | Catálogo global de satélites, con rotaciones LL, RR, LR y RL para garantizar `O(log n)` |
| **BufferMensajes** | Árbol Binario de Búsqueda (ABB) como cola de prioridad | Buffer de mensajes por satélite, despachando primero los de prioridad 5 (crítica) |
| **LogAuditoria** | Lista enlazada simple | Bitácora cronológica de eventos, alertas y errores, con búsqueda por expresión regular |

**Complejidades logradas:**
- Búsqueda en matriz dispersa: `O(r + c)`
- Inserción/búsqueda/eliminación en AVL: `O(log n)` garantizado mediante balanceo estricto (`|FE| ≤ 1`)
- Encolamiento por prioridad en ABB: ordenado por campo `Priority` (1–5)

---

## ✨ Funcionalidades principales

### 📥 Motor de Ingesta y Carga Masiva
- Parseo de configuración vía **XML + XPath** (`XmlDocument` con `XmlResolver = null` para mitigar XXE).
- Validaciones sintácticas estrictas con **expresiones regulares** (ID de satélite, IPv4, coordenadas).
- Carga **transaccional atómica**: si un registro falla la validación, se cancela toda la carga y se revierte el estado en memoria, registrando el error en la bitácora.

### 🌐 Protocolo REST Multi-Puerto
- Comunicación entre instancias mediante `IHttpClientFactory` y peticiones POST síncronas.
- Autenticación obligatoria vía **HTTP Basic Authentication** (credenciales codificadas en Base64).
- Rechazo con `401 Unauthorized` ante credenciales inválidas o ausentes.

**Endpoints principales:**

| Método | Endpoint | Función |
|---|---|---|
| `POST` | `/api/v1/space/config` | Ingesta de configuración XML |
| `POST` | `/api/v1/space/relay` | Enrutamiento inter-satelital (requiere Basic Auth) |
| `POST` | `/api/v1/space/simulation/step` | Avance de simulación por *ticks* |

### 📊 Visualización en Memoria con Graphviz
Generación de reportes vectoriales SVG **compilados en caliente**, sin escritura de archivos temporales en disco — el proceso `dot.exe` recibe el código DOT por `StandardInput` y retorna el SVG por `StandardOutput`, evitando *deadlocks* de buffer mediante cierre explícito del canal de entrada antes de leer la salida.

| Reporte | Contenido |
|---|---|
| **Memory Layout Map** | Disposición física de punteros de los TDAs en tiempo real (`shape=record`) |
| **Relay Route Tracer** | Trazabilidad de la ruta de retransmisión de un paquete, coloreando nodos activos/inactivos |
| **Matriz de Capacidad del Buffer** | Mapa consolidado de ocupación de las colas de prioridad por satélite |

### 🔒 Seguridad (OWASP)
- Mitigación de **XXE** deshabilitando la resolución de entidades externas en el parser XML.
- Sanitización del código DOT antes de inyectarlo con `@Html.Raw()` para prevenir **XSS**.

### 🧪 Pruebas Automatizadas
Proyecto de pruebas desacoplado con **xUnit/NUnit**, cubriendo:
- Balanceo correcto del árbol AVL ante inserciones desordenadas.
- Integridad de punteros en la matriz dispersa tras eliminaciones.
- Orden correcto de despacho por prioridad en el ABB.
- Pruebas de integración simulando el tránsito de un paquete entre los puertos 5000 y 5001.

---

## 🛠️ Tecnologías

- **Lenguaje / Framework:** C# — ASP.NET Core MVC (.NET 8.0)
- **Persistencia:** Memoria RAM — TDAs manuales autorreferenciados (sin `System.Collections`)
- **Comunicación:** REST vía `HttpClient`, HTTP Basic Authentication
- **Parseo de datos:** `XmlDocument` + XPath, `System.Text.RegularExpressions`
- **Visualización:** Graphviz (compilación DOT → SVG en memoria, vía `Process`)
- **Frontend:** Vistas Razor, Bootstrap 5
- **Pruebas:** xUnit / NUnit
- **Control de versiones:** Git Flow (`main`, `develop`, `feature/*`)

---

## 🚀 Cómo ejecutar el proyecto

### Requisitos previos
- .NET 8.0 SDK
- Graphviz instalado y accesible en el `PATH` (comando `dot`)

### Levantar ambas instancias

```bash
# Terminal 1 — Hemisferio Norte
cd OrbitNet.WebNorte
dotnet run --urls=http://localhost:5000

# Terminal 2 — Hemisferio Sur
cd OrbitNet.WebSur
dotnet run --urls=http://localhost:5001
```

Ambas instancias deben estar activas simultáneamente para que el enrutamiento cross-port funcione correctamente.

### Cargar configuración inicial
Desde la interfaz web de cada instancia, cargar el archivo XML de topología correspondiente (Norte → puerto 5000, Sur → puerto 5001) mediante el módulo de carga masiva.

---

## 📂 Estructura del repositorio

```
OrbitNet-NetCore/
├── OrbitNet.WebNorte/            # Instancia MVC - Hemisferio Norte (puerto 5000)
├── OrbitNet.WebSur/               # Instancia MVC - Hemisferio Sur (puerto 5001)
├── OrbitNet.Core/                 # Biblioteca de clases: TDAs manuales
│   ├── RedSatelitalPlano.cs       # Matriz dispersa ortogonal
│   ├── RegistroSatelites.cs       # Árbol AVL
│   ├── BufferMensajes.cs          # ABB - cola de prioridad
│   └── LogAuditoria.cs            # Lista enlazada simple
├── OrbitNet.Services/              # Motor de simulación, enrutamiento y Graphviz
├── OrbitNet.Tests/                # Pruebas unitarias e integración (xUnit)
├── docs/
│   ├── MANUAL_TECNICO.md
│   ├── MANUAL_USUARIO.md
│   └── DiagramaFlujo.pdf
└── README.md
```

> *Nota: ajusta esta estructura con los nombres reales de tus proyectos/carpetas dentro de la solución.*

---

## ✅ Validaciones implementadas

- ID de satélite: `^SAT-(ECU|POL)-\d{4}$`
- Dirección IPv4 válida mediante regex estándar.
- Formato de coordenadas geográficas (latitud/longitud).
- Rechazo transaccional atómico ante cualquier dato inconsistente en la carga masiva.

---

## 🎓 Contexto académico

Proyecto grupal desarrollado para el curso **IPC2** de la Escuela de Ciencias y Sistemas (USAC), aplicando estructuras de datos no lineales, arquitectura MVC en C#/.NET, comunicación REST distribuida, generación de gráficos vectoriales en memoria y buenas prácticas de control de versiones bajo Git Flow.

---

## 👤 Autor

**Jorge** — Estudiante de Ingeniería en Sistemas, USAC
Facultad de Ingeniería, Universidad de San Carlos de Guatemala
