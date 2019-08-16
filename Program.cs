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
                    MostrarSílabas(arg);
                }
            }
        }

        #region Interfaz de usuario

        //
        // Divide una palabra en sus sílabas y las muestra.
        //
        private static void MostrarSílabas(string palabra)
        {
            var sílabas = DividirEnSílabas(palabra);
            Array.Reverse(sílabas);

            Console.WriteLine( string.Join("-", sílabas) );
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
            string línea;
            while ((línea = Console.ReadLine()) != null)
            {
                MostrarSílabas(línea);
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

        const char LETRA_NO_VÁLIDA = '\0';

        private enum Vocal
        {
            Ninguna,
            Cerrada,
            Abierta,
        }

        private static string[] DividirEnSílabas(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return Array.Empty<string>();

            palabra = palabra.Trim();

            char letra;
            char letraSig = LETRA_NO_VÁLIDA;

            var listaSílabas = new List<string>();

            int numVocales = 0;
            string sílaba = "";

            for (int i = palabra.Length - 1; i >= 0; --i)
            {
                bool terminada = false;

                letra = palabra[i];

                bool esVocal = EsVocal(letra, out Vocal tipoVocal);
                bool esVocalSig = EsVocal(letraSig, out Vocal tipoVocalSig);

                string letraCombi = letra + (letraSig != LETRA_NO_VÁLIDA ? letraSig.ToString() : "");

                if (esVocal && !esVocalSig)
                    numVocales++;

                if (numVocales > 1 && !esVocalSig)
                    terminada = true;
                else if (!esVocal && !esVocalSig && !EsGrupoConsonántico(letraCombi) && (numVocales == 1 && i > 1))
                    terminada = true;
                else if ((tipoVocal == Vocal.Abierta && tipoVocalSig == Vocal.Abierta) ||
                        (tipoVocal == Vocal.Cerrada && TieneTilde(letra) && esVocalSig) ||
                        (tipoVocal == Vocal.Abierta && (tipoVocalSig == Vocal.Cerrada && TieneTilde(letraSig))) ||
                        ((esVocal && esVocalSig) && (letra == letraSig)))
                {
                    terminada = true;
                    numVocales++;
                }

                if (terminada)
                {
                    listaSílabas.Add(sílaba);
                    sílaba = letra.ToString();
                    numVocales--;
                }
                else
                    sílaba = letra + sílaba;

                letraSig = letra;
            }

            listaSílabas.Add(sílaba);

            return listaSílabas.ToArray();
        }

        private static bool TieneTilde(char letra)
        {
            letra = char.ToLower(letra);
        
            switch(letra)
            {
                case 'á':
                case 'é':
                case 'í':
                case 'ó':
                case 'ú':
                    return true;
                
                default:
                    return false;
            }
        }

        private static bool EsVocal(char letra, out Vocal tipoDeVocal)
        {
            if(letra != LETRA_NO_VÁLIDA)
            {
                tipoDeVocal = TipoDeVocal(letra);
                return EsVocal(letra);
            }

            tipoDeVocal = Vocal.Ninguna;
            return false;
        }

        private static bool EsVocal(char letra)
        {
            if(letra != LETRA_NO_VÁLIDA)
            {
                switch(letra)
                {
                    case 'a':
                    case 'á':
                    case 'e':
                    case 'é':
                    case 'i':
                    case 'í':
                    case 'o':
                    case 'ó':
                    case 'u':
                    case 'ú':
                    case 'ü':
                        return true;
                    
                    default:
                        return false;
                }
            }
            
            return false;
        }

        private static Vocal TipoDeVocal(char letra)
        {
            if(letra != LETRA_NO_VÁLIDA)
            {
                if(EsVocal(letra))
                {
                    switch(letra)
                    {
                        case 'i':
                        case 'í':
                        case 'u':
                        case 'ú':
                        case 'ü':
                            return Vocal.Cerrada;
                        
                        default:
                            return Vocal.Abierta;
                    }
                }
            }
            
            return Vocal.Ninguna;
        }

        private static bool EsLetra(char letra)
        {
            if(letra >= 'a' && letra <= 'z' || letra == 'ñ')
                return true;
            if(letra == 'á' || letra == 'é' || letra == 'í' || letra == 'ó' || letra == 'ú' || letra == 'ü')
                return true;
            
            return false;
        }

        private static bool EsGrupoConsonántico(string letras)
        {
            letras = letras.ToLower();

            switch(letras)
            {
                case "bl":
                case "br":
                case "cl":
                case "cr":
                case "dl":
                case "dr":
                case "fl":
                case "fr":
                case "gl":
                case "gr":
                case "pl":
                case "pr":
                case "tl":
                case "tr":
                case "kl":
                case "kr":
                case "ll":
                case "rr":
                case "ch":
                case "qu":
                    return true;

                default:
                    return false;
            }
        }
    }
}