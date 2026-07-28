

# 📝 Bitácora de Desarrollo (OrbitNet-NetCore)

# Josué Javier Carrera Soyós - 202300834

## 📌 FASE 1: Estructura Base e Ingesta

* **Estructura:** Inicialización de la solución `OrbitNet-NetCore` y creación de la arquitectura de carpetas base.
* **Ingesta:** Implementación del motor de lectura y procesamiento de archivos XML.

## 📌 FASE 2: Modelado, Graphviz y Refactorización

* **Modelos:** Integración de los componentes `Satelite`, `ListaSatelite` y `NodoSatelite`.
* **Componentes:** Adición de `XmlController`, `MemoriaPlano` y `RedSatelitePlano`.
* **Visualización:** Integración del compilador de Graphviz para diagramas de relaciones.
* **Optimización:** Limpieza de arquitectura, validaciones de datos y enrutamiento.

## 📌 FASE 3: Conectividad y Cierre

* **API:** Integración de controladores restantes y clientes HTTP para comunicación entre instancias de OrbitNet.
* **Calidad:** Depuración final de la arquitectura, validaciones oficiales y enrutamiento de archivos.

---

# 📝 Bitácora de Desarrollo (Estructuras y Árboles)

## Byron Rodolfo de Leon - 201404117

## 📌 FASE 1: Clases Base y Organización

-   **Implementación:** Creación de las clases `ListaSatelite` y `RedSatelitalPlano`.
    
-   **Organización:** Creación de las carpetas `estructuras` y `bitacora` para documentar el desarrollo individual.
    

## 📌 FASE 2: Integración y Visualización

-   **Modelos:** Integración de `ListaSatelite`, `RedSatelitalPlano`, `BufferMensajesAbb`, `BufferMensajes` y `ArbolSateliteAvl`.
    
-   **Operaciones:** Visualización gráfica de los árboles, gestión de inserciones/eliminaciones y reporte de errores del sistema.
    

## 📌 FASE 3: Árboles Avanzados y Optimización

-   **Cola de Prioridad (ABB):** Integración nativa de `BufferMensajesAbb` como Árbol Binario de Búsqueda en memoria (sin colecciones genéricas de .NET).
    
-   **Árbol AVL:** Implementación de estructura autobalanceada para almacenar y listar satélites con alta eficiencia y búsquedas en **O(logn)**.

---

# 📝 Bitácora de Desarrollo (Servicios y Pruebas)

# José Esaú Jiatas Cruz - 202303332

## 📌 FASE 1: Arquitectura y Reportes Base

* **Arquitectura:** Configuración de `OrbitNet.Services`, inyección de dependencias y definición de interfaces (`IReportService`) bajo el patrón MVC.
* **Reportes Planos:** Desarrollo de la lógica inicial para extraer y formatear datos de la Lista Simple de Logs en texto estructurado.

## 📌 FASE 2: Motor Gráfico y Traducción Visual

* **Motor Graphviz:** Implementación asíncrona de `GraphvizEngine.cs` para ejecutar comandos `dot` en segundo plano y capturar gráficos SVG sin bloquear el servidor.
* **Generador Visual:** Creación de `ReportGenerator.cs` para mapear los punteros de la Matriz Dispersa y Listas, enviando los SVG generados a las vistas Razor.

## 📌 FASE 3: Automatización de Pruebas (xUnit)

* **Pruebas Unitarias:** Validación estructural de árboles y matrices (`AvlTests.cs`, `MatrixTests.cs`) para certificar el balanceo dinámico y la reconexión de punteros ortogonales.
* **Pruebas de Integración:** Simulación de peticiones HTTP cruzadas entre puertos (Norte/Sur) y certificación del Árbol Binario de Búsqueda (ABB) como cola de prioridad matemática.
  
---  

# 📝 Bitácora de Desarrollo

# Oliver Jorge Raxtún Morales - 202400634

## 📌 FASE 1: Estructuras Base y Auditoría

* **C1:** Creación de la interfaz `IAbstractCollection` y el nodo `LogNode`.
* **C2:** TDA `LogAuditoria` con inserción O(1) y búsqueda por expresiones regulares.

## 📌 FASE 2: Reportes y Presentación

* **C1:** `ReportGenerator` para crear código DOT en memoria (sin colecciones nativas).
* **C2:** Vistas Razor integradas para renderizar los gráficos SVG de rutas y buffers.

## 📌 FASE 3: Integración, AVL y Documentación

* **C1:** Corrección en `XmlController` y `Satelite.cs`; implementación de árboles AVL (búsqueda y balanceo).
* **C2:** Manual Técnico y diagramas de arquitectura/flujo en SVG.
* **C3:** Manual de Usuario, capturas de pantalla y diagrama de flujo.