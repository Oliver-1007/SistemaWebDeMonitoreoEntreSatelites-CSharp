using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrbitNet.Services.Data;

namespace OrbitNet.WebSur.Attributes
{
    /// <summary>
    /// Atributo de autorización personalizado que implementa IAuthorizationFilter.
    /// Intercepta las solicitudes HTTP, extrae y decodifica la cabecera 'Authorization' de tipo Basic,
    /// y valida que las credenciales coincidan con las oficiales (orbitnet_admin / USAC_ECYS_2026).
    /// Si la validación falla, interrumpe el flujo retornando un estado 401 (Unauthorized) con un JSON descriptivo.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Método interceptor que se ejecuta antes de ingresar a la acción del controlador decorado.
        /// </summary>
        /// <param name="context">El contexto de la solicitud y el filtro de autorización.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 1. Obtener la cabecera 'Authorization' desde los headers de la solicitud HTTP.
            string? authHeader = context.HttpContext.Request.Headers["Authorization"];

            // 2. Verificar si la cabecera existe y tiene el formato correcto (empieza con "Basic ").
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                // Registrar el incidente de acceso fallido sin cabeceras en la bitácora de auditoría.
                MemoriaPlano.Logs.Registrar("ALERT", "Acceso denegado a API segura: Cabecera 'Authorization' ausente o de formato inválido.");

                // Interrumpir la ejecución de la petición y retornar un error JSON estructurado con estado HTTP 401.
                context.Result = new JsonResult(new
                {
                    status = "Unauthorized",
                    mensaje = "Falta la cabecera de autorizacion o no es del tipo Basic."
                })
                {
                    StatusCode = 401
                };
                return;
            }

            try
            {
                // 3. Extraer el token Base64 omitiendo el prefijo "Basic " (que tiene 6 caracteres de longitud).
                string base64Token = authHeader.Substring(6).Trim();

                // 4. Decodificar los bytes del token Base64 a su correspondiente representación en bytes.
                byte[] credencialesBytes = Convert.FromBase64String(base64Token);

                // 5. Convertir los bytes decodificados a una cadena UTF-8 (el formato esperado es 'usuario:contrasena').
                string credencialesDecodificadas = Encoding.UTF8.GetString(credencialesBytes);

                // 6. Localizar el delimitador de dos puntos ':' para separar el usuario y la contraseña.
                int separadorIndex = credencialesDecodificadas.IndexOf(':');
                if (separadorIndex == -1)
                {
                    // Si no contiene el delimitador, las credenciales no son válidas.
                    MemoriaPlano.Logs.Registrar("ALERT", "Acceso denegado a API segura: Formato de credenciales Basic inválido (sin delimitador ':').");

                    context.Result = new JsonResult(new
                    {
                        status = "Unauthorized",
                        mensaje = "El formato de las credenciales es incorrecto (debe ser usuario:contrasena codificado)."
                    })
                    {
                        StatusCode = 401
                    };
                    return;
                }

                // 7. Extraer el usuario y contraseña por separado utilizando subcadenas basadas en el índice del delimitador.
                string usuario = credencialesDecodificadas.Substring(0, separadorIndex);
                string contrasena = credencialesDecodificadas.Substring(separadorIndex + 1);

                // 8. Comparar con las credenciales oficiales obligatorias del plano OrbitNet.
                if (usuario == "orbitnet_admin" && contrasena == "USAC_ECYS_2026")
                {
                    // Si son válidas, registrar el éxito de la autenticación en la bitácora de auditoría.
                    MemoriaPlano.Logs.Registrar("INFO", $"Autenticacion HTTP Basic exitosa para el usuario: {usuario}");

                    // Al no modificar context.Result, la petición fluye normalmente hacia la acción del controlador.
                    return;
                }
                else
                {
                    // Si las credenciales no coinciden, registrar el intento fallido en la bitácora.
                    MemoriaPlano.Logs.Registrar("ALERT", $"Acceso denegado a API segura: Intento fallido con usuario '{usuario}' y contraseña incorrecta.");

                    // Responder con un estado HTTP 401 indicando credenciales inválidas.
                    context.Result = new JsonResult(new
                    {
                        status = "Unauthorized",
                        mensaje = "Credenciales de acceso incorrectas."
                    })
                    {
                        StatusCode = 401
                    };
                    return;
                }
            }
            catch (Exception ex)
            {
                // Capturar cualquier fallo durante la decodificación Base64 o conversión de texto.
                MemoriaPlano.Logs.Registrar("ERROR", $"Error interno al decodificar cabeceras de seguridad: {ex.Message}");

                context.Result = new JsonResult(new
                {
                    status = "Unauthorized",
                    mensaje = "Error al procesar la autenticacion de red."
                })
                {
                    StatusCode = 401
                };
            }
        }
    }
}

