using System;
using System.Diagnostics;
using System.IO;



namespace OrbitNet.Services.Visualizacion
{
    public static class GraphvizCompilador
    {
        public static string CompilarDotASvg(string dotSourceCode)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = ObtenerRutaDot();
                    process.StartInfo.Arguments = "-Tsvg";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    // Escribir el código DOT en la entrada estándar
                    using (StreamWriter writer = process.StandardInput)
                    {
                        writer.Write(dotSourceCode);
                        writer.Flush();
                    } // Al salir del bloque using se cierra el StandardInput, indicando fin de transmisión a dot.exe

                    // Leer los resultados de salida y de error
                    string svgOutput = process.StandardOutput.ReadToEnd();
                    string errorOutput = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Graphviz falló con código de salida {process.ExitCode}. Detalle: {errorOutput}");
                    }

                    // Extraer únicamente el marcado XML del SVG (omitir posibles avisos iniciales)
                    int svgStartIdx = svgOutput.IndexOf("<svg");
                    if (svgStartIdx >= 0)
                    {
                        return svgOutput.Substring(svgStartIdx);
                    }

                    return svgOutput;
                }
            }
            catch (Exception ex)
            {
                // Retorna un SVG de error en caso de que Graphviz no esté instalado o falle
                return $@"<svg width=""500"" height=""80"" xmlns=""http://www.w3.org/2000/svg"">
                            <rect width=""100%"" height=""100%"" fill=""#FADBD8"" rx=""10""/>
                            <text x=""20"" y=""45"" font-family=""monospace"" font-size=""12"" fill=""#78281F"">
                                Error al renderizar: {ex.Message}
                            </text>
                        </svg>";
            }
        }

        private static string ObtenerRutaDot()
        {
            string path1 = @"C:\Program Files\Graphviz\bin\dot.exe";
            if (File.Exists(path1)) return path1;

            string path2 = @"C:\Program Files (x86)\Graphviz\bin\dot.exe";
            if (File.Exists(path2)) return path2;

            return "dot";
        }
    }
}






