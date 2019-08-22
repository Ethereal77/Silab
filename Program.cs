using System;
using System.IO;
using System.Collections.Generic;

namespace Textuo
{
    class Program
    {
        //
        // Punto de inicio de la aplicación.
        //
        public static int Main(string[] args)
        {
            if(args.Length == 0)
            {
                MostrarAyudaDeUso();
                return 0;
            }
            
            var modoPresentación = Modo.Dash;
            var listaEntradas = new List<(Entrada Tipo, string)>();
            bool entradaStdin = false;

            int i = 0;
            while(i < args.Length)
            {
                var arg = args[i].ToLower();

                switch(arg)
                {
                    // === Entrada: Archivo de palabras
                    case "-f":
                    case "--file":
                        if(i+1 < args.Length)
                            listaEntradas.Add( (Entrada.File, args[++i]) );
                        else
                            ErrorArchivoNoEspecificado();
                        break;

                    // === Entrada: Emtrada estándar (stdin)
                    case "-i":
                    case "--in":
                        entradaStdin = true;
                        break;

                    // === Especificador de Modo de Presentación
                    case "-m":
                    case "--mode":
                        if(i+1 < args.Length)
                        {
                            var modo = args[++i].ToLower();
                            switch(modo)
                            {
                                case "dash": modoPresentación = Modo.Dash; break;
                                case "space": modoPresentación = Modo.Space; break;
                                case "line": modoPresentación = Modo.Line; break;

                                default:
                                    ErrorModoNoVálido(modo);
                                    break;
                            }
                        }
                        else
                            ErrorModoNoEspecificado();
                        break;

                    // === Entrada: Palabra suelta
                    default:
                        if(arg.StartsWith("--") || arg.StartsWith("-"))
                            ErrorArgumentoNoVálido(arg);
                        else
                            listaEntradas.Add( (Entrada.Palabra, arg) );
                        break;
                }

                i++;    // Siguiente argumento
            }

            // Sin entradas a procesar: Mostramos la ayuda
            if(listaEntradas.Count == 0 && !entradaStdin)
            {
                MostrarAyudaDeUso();
                return 0;
            }

            var divisor = new DivisorDePalabras();

            var separador = "";
            switch(modoPresentación)
            {
                case Modo.Dash: separador="-"; break;
                case Modo.Space: separador=" "; break;
            }

            // Procesamos las entradas en orden
            foreach(var entrada in listaEntradas)
            {
                switch(entrada.Tipo)
                {
                    case Entrada.Palabra:
                        MostrarSílabas(entrada.Item2, divisor, modoPresentación, separador);
                        break;

                    case Entrada.File:
                        LeerArchivo(entrada.Item2, divisor, modoPresentación, separador);
                        break;
                }
            }

            if(entradaStdin)
            {
                LeerEntrada(divisor, modoPresentación, separador);
            }

            return 0;   // Todo bien
        }

        #region Interfaz de usuario

        //
        // Divide una palabra en sus sílabas y las muestra.
        //
        private static void MostrarAyudaDeUso()
        {
            Console.WriteLine();

            Console.WriteLine("Silab (Proyecto 'Textuo') v1.0");
            Console.WriteLine("  Divide una palabra en las sílabas que la forman, siguiendo las reglas");
            Console.WriteLine("  ortográficas del Español.");

            Console.WriteLine();
            Console.WriteLine("  Uso:");
            Console.WriteLine("    Silab <Palabra> : Muestra las sílabas que componen <Palabra>.");
            Console.WriteLine();
            Console.WriteLine("    Silab -f <Archivo>");
            Console.WriteLine("    Silab --file <Archivo>");
            Console.WriteLine("        Muestra las sílabas de cada palabra que se encuentre en el");
            Console.WriteLine("        <Archivo>. Debe aparecer una palabra por línea.");
            Console.WriteLine();
            Console.WriteLine("    Silab -i");
            Console.WriteLine("    Silab --in");
            Console.WriteLine("        Muestra las sílabas de cada palabra que se encuentre en la");
            Console.WriteLine("        entrada estándar (stdin).");
            Console.WriteLine();
            Console.WriteLine("    Silab -m <Modo>");
            Console.WriteLine("    Silab --mode <Modo>");
            Console.WriteLine("        Especifica el modo de presentación de las sílabas. Debe ser");
            Console.WriteLine("        uno de los valores siguientes:");
            Console.WriteLine();
            Console.WriteLine("          Dash : Muestra la palabra en una sola línea, con las sílabas");
            Console.WriteLine("                 separadas mediante guiones.");
            Console.WriteLine();  
            Console.WriteLine("          Space : Muestra la palabra en una sola línea, con las sílabas");
            Console.WriteLine("                  separadas mediante espacios.");
            Console.WriteLine();  
            Console.WriteLine("          Line : Muestra la palabra en varias líneas, cada sílaba en una");
            Console.WriteLine("                 línea diferente.");
            Console.WriteLine();
            Console.WriteLine("        Sólo tendrá efecto el último que se especifique en el caso de");
            Console.WriteLine("        que se especifique más de un modo de presentación.");
            Console.WriteLine();
        }

        //
        // Divide una palabra en sus sílabas y las muestra.
        //
        private static void MostrarSílabas(string palabra, DivisorDePalabras divisorDePalabras,
                                            Modo modoDePresentación, string separador)
        {
            //Console.Write($"{palabra} --> ");

            var sílabas = divisorDePalabras.DividirEnSílabas(palabra);

            if(modoDePresentación == Modo.Line)
            {
                foreach(var sílaba in sílabas)
                    Console.WriteLine(sílaba);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine( string.Join(separador, sílabas) );
                Console.ResetColor();
            }
        }

        //
        // Lee un archivo y divide en sílabas cada palabra que aparezca. El archivo se lee línea a línea.
        //
        private static void LeerArchivo(string archivo, DivisorDePalabras divisorDePalabras,
                                        Modo modoDePresentación, string separador)
        {
            if(!File.Exists(archivo))
                ErrorArchivoNoEncontrado(archivo);
            else
            {
                try
                {
                    // Redirigimos a la entrada estándar el contenido del archivo
                    Console.SetIn(new StreamReader(archivo));

                    // Leemos por la entrada estándar normalmente
                    LeerEntrada(divisorDePalabras, modoDePresentación, separador);
                }
                catch(IOException)
                {
                    // Ante cualquier error asumimos que el archivo no se puede leer
                    ErrorArchivoNoSePuedeAbrir(archivo);
                }
            }
        }

        //
        // Divide en sílabas cada palabra que aparezca en la entrada estándar.
        //
        private static void LeerEntrada(DivisorDePalabras divisorDePalabras, Modo modoDePresentación, string separador)
        {
            string línea;
            while ((línea = Console.ReadLine()) != null)
            {
                MostrarSílabas(línea, divisorDePalabras, modoDePresentación, separador);
            }
        }

        // === Mensajes de error === === === === === === === === === === === === === === //

        //
        // Error: Argumento no válido.
        //
        private static void ErrorArgumentoNoVálido(string arg)
        {
            MostrarError($"El argumento '{arg}' no es válido");
        }

        //
        // Error: Archivo no especificado.
        //
        private static void ErrorArchivoNoEspecificado()
        {
            MostrarError("No se ha especificado el nombre de archivo a leer", 2);
        }

        //
        // Error: Archivo no encontrado.
        //
        private static void ErrorArchivoNoEncontrado(string archivo)
        {
            MostrarError($"El archivo '{archivo}' no existe", 3);
        }

        //
        // Error: No se puede abrir el archivo.
        //
        private static void ErrorArchivoNoSePuedeAbrir(string archivo)
        {
            MostrarError($"El archivo '{archivo}' no se puede leer", 4);
        }

        //
        // Error: Modo de presentación no válido.
        //
        private static void ErrorModoNoVálido(string arg)
        {
            MostrarError($"El modo de presentación '{arg}' no es válido", 5);
        }

        //
        // Error: Modo de presentación no especificado.
        //
        private static void ErrorModoNoEspecificado()
        {
            MostrarError("No se ha especificado el modo de presentación", 6);
        }

        //
        // Muestra un mensaje de error y sale con un código de error.
        //
        private static void MostrarError(string mensaje, int código = 1)
        {
            TextWriter errorWriter = Console.Error;

            Console.ForegroundColor = ConsoleColor.Red;

            errorWriter.WriteLine();
            errorWriter.WriteLine($"  ERROR: {mensaje}");
            errorWriter.WriteLine();

            Console.ResetColor();
            Environment.Exit(exitCode: código);
        }

        #endregion

        #region Configuración

        private enum Entrada
        {
            Palabra,
            File,
            Stdin
        }

        private enum Modo
        {
            Dash,
            Space,
            Line
        }

        #endregion
    }
}