using System;
using System.IO;
using System.Collections.Generic;

namespace Textuo
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
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
            }
            else if(args.Length > 0)
            {
                var arg = args[0].ToLower();

                if(arg.StartsWith("--") && arg.Length > "--".Length)
                {
                    if(arg == "--file")
                    {
                        if(args.Length > 1)
                            LeerArchivo(args[1]);
                        else
                            ErrorArchivoNoEspecificado();
                    }
                    else if(arg == "--in")
                        LeerEntrada();
                    else
                        ErrorArgumentoNoVálido(args[0]);
                }
                else if(arg.StartsWith("-") && arg.Length > "-".Length)
                {
                    if(arg == "-f")
                    {
                        if(args.Length > 1)
                            LeerArchivo(args[1]);
                        else
                            ErrorArchivoNoEspecificado();
                    }
                    else if(arg == "-i")
                        LeerEntrada();
                    else
                        ErrorArgumentoNoVálido(args[0]);
                }
                else
                {
                    // Mostrar las sílabas de una sola palabra
                    var divisor = new DivisorDePalabras();
                    MostrarSílabas(arg, divisor);
                }
            }
        }

        #region Interfaz de usuario

        //
        // Divide una palabra en sus sílabas y las muestra.
        //
        private static void MostrarSílabas(string palabra, DivisorDePalabras divisorDePalabras)
        {
            Console.Write($"{palabra} --> ");

            var sílabas = divisorDePalabras.DividirEnSílabas(palabra);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine( string.Join("-", sílabas) );
            Console.ResetColor();
        }

        //
        // Lee un archivo y divide en sílabas cada palabra que aparezca. El archivo se lee línea a línea.
        //
        private static void LeerArchivo(string archivo)
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
                    LeerEntrada();
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
        private static void LeerEntrada()
        {
            var divisor = new DivisorDePalabras();

            string línea;
            while ((línea = Console.ReadLine()) != null)
            {
                MostrarSílabas(línea, divisor);
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
    }
}