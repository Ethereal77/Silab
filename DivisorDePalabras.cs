using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Textuo
{
    public sealed class DivisorDePalabras
    {
        private readonly List<string> listaSílabas = new List<string>();
        private readonly Contexto contexto = new Contexto();
        private readonly Configuración config;


        public DivisorDePalabras(Configuración configuración = null)
        {
            config = configuración ?? new Configuración();
        }


        public IEnumerable<string> DividirEnSílabas(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return Array.Empty<string>();

            palabra = palabra.Trim();
            palabra = palabra.ToLower();

            contexto.Palabra = palabra;
            contexto.Posición = 0;
            contexto.Cursor = 0;
            
            listaSílabas.Clear();
            LeerPalabra();

            return listaSílabas;
        }

        //
        // Lee una palabra y obtiene una lista de las sílabas que la componen.
        //
        private void LeerPalabra()
        {
            // Palabra := <Sílaba> { [ '-' ] <Sílaba> }

            bool continuar = true;
            do
            {
                // Leemos una sílaba
                if(LeerSílaba())
                {
                    var inicio = contexto.Posición;
                    var longitud = contexto.Cursor - contexto.Posición;

                    listaSílabas.Add( contexto.Palabra.Substring(inicio, longitud) );

                    contexto.Posición = contexto.Cursor;
                }
                // Si encontramos un carácter que no sea letra, lo añadimos como si una sílaba fuese
                else if(MirarLetra(out char letra) && !letra.EsLetra() )
                {
                    ConsumirLetra();
                    listaSílabas.Add( contexto.Palabra.Substring(contexto.Posición, 1) );

                    contexto.Posición = contexto.Cursor;
                }
                else
                    // Si no hay sílabas ni caracteres restantes, hemos acabado
                    continuar = false;
            
            } while(continuar);
        }

        //
        // Lee una sílaba de una palabra.
        //
        private bool LeerSílaba()
        {
            // Sílaba := <GrupoConsonantes> <ParteVocálica>
            //         | <ParteVocálica>

            // ParteVocálica := <GrupoVocales> [ <GrupoConsonantes> ]

            // GrupoVocales := <Triptongo> | <Diptongo>
            //               | <Hiato>
            //               | <Vocal>

            // GrupoConsonantes := <GrupoConsonánticoIndivisible>
            //                   | <Consonante>

            if(!MirarLetra(out Letra.Info letraActual))
                return false;
            if(!letraActual.Letra.EsLetra())
                return false;

            if(letraActual.EsVocal)
            {
                if(LeerParteVocálica(letraActual))
                    return true;
            }
            else
            {
                if(LeerGrupoConsonántico(letraActual))
                {
                    // KKA ==> (KKA)

                    // Grupo de consonantes indivisible, seguido de una parte vocálica
                    if(!MirarLetra(out letraActual))
                        return false;
                    if(LeerParteVocálica(in letraActual))
                        return true;
                }
                else
                {
                    if(!MirarLetra(+1, out letraActual))
                        return false;

                    // Consonante suelta, seguido de una parte vocálica
                    if(letraActual.EsVocal)
                    {
                        // CA ==> (CA)

                        ConsumirLetra();
                        if(LeerParteVocálica(in letraActual))
                            return true;
                        
                        return false;
                    }

                    // Dos consonantes sueltas, seguidas de una parte vocálica
                    if(!MirarLetra(+2, out letraActual))
                        return false;

                    if(letraActual.EsVocal)
                    {
                        // CCA ==> (CCA)

                        ConsumirLetras(2);
                        if(LeerParteVocálica(in letraActual))
                            return true;
                        
                        return false;
                    }
                }
            }

            // Parte no reconocida. Sílaba no válida
            return false;
        }

        //
        // Lee la parte vocálica de una sílaba, consistente en un grupo de vocales (A), seguido de una
        // o varias consonantes (C) o de un grupo consonántico (KK).
        //
        //   ParteVocálica := <GrupoVocales> [ <GrupoConsonantes> ]
        //
        //   GrupoVocales := <Triptongo> | <Diptongo>
        //                 | <Hiato>
        //                 | <Vocal>
        //
        private bool LeerParteVocálica(in Letra.Info letraActual)
        {
            // --- Grupo de Vocales ----------------------

            Debug.Assert(letraActual.EsVocal);

            if(LeerTriptongo(letraActual) ||
               LeerDiptongo(letraActual))
            {
                // Diptongo o triptongo
            }
            else if(LeerHiato(letraActual))
            {
                // Si es un hiato, significa que la siguiente letra es vocal y forma parte de
                // otra sílaba, por lo que hemos acabado ésta
                return true;
            }
            else
            {
                // Vocal suelta
                ConsumirLetra();
            }

            // Comprobamos si hay otra letra a continuación
            if(!MirarLetra(out Letra.Info siguiente))
                // Fin de la sílaba
                return true;
            
            // --- Grupo de Consonantes ----------------------

            // Si tras las vocales hay letras, que deben ser consonantes, vemos cuáles son:
            Debug.Assert(!siguiente.EsVocal);

            if(LeerGrupoConsonántico(siguiente))
            {
                // AKK ==> (AKK)

                // Vemos si hay algo a continuación del grupo
                if(!MirarLetra(out siguiente))
                    // Fin de la sílaba. El grupo forma parte de la sílaba actual
                    return true;
                
                // AKKA ==> (A) (KKA) --> Ejemplo: hA-CHa

                // Si tras el grupo hay una vocal
                if(siguiente.EsVocal)
                {
                    // Forma parte de otra sílaba, por lo que el grupo consonántico va con dicha
                    // vocal y forma parte de esa otra sílaba
                    Retroceder(2);
                    return true;
                }
                else
                {
                    // Si hay consonantes tras el grupo, esas forman parte de otra sílaba
                    Debug.WriteLine("AKKC: Consonante no esperada tras un grupo consonántico.");
                    return true;
                }
            }

            // Parece que tras las vocales hay una consonante suelta. Vemos qué hay tras ella
            if(!MirarLetra(+1, out siguiente))
            {
                // AC· ==> (AC) --> Ejemplo: aS

                // Fin de la sílaba. La consonante forma parte de la sílaba actual
                ConsumirLetra();
                return true;
            }

            // Si, tras la consonante, hay otra vocal, forma parte de otra sílaba
            if(siguiente.EsVocal)
            {
                // ACA ==> (A) (CA) --> Ejemplo: a-Sa

                return true;
            }

            // Parece que hay dos consonantes seguidas tras las vocales

            // Comprobamos si hay un grupo consonántico tras la última consonante
            ConsumirLetra();
            if(LeerGrupoConsonántico(siguiente))
            {
                // Miramos qué hay tras el grupo consonántico
                if(!MirarLetra(out siguiente))
                {
                    // ACKK ==> (ACKK)

                    // Fin de sílaba. Las tres consonantes forman parte de la sílaba actual
                    return true;
                }

                if(siguiente.EsVocal)
                {
                    // ACKKA ==> (AC) (KKA) --> Ejemplo: eS-PLé-ni-co

                    // El grupo consonántico corresponden a esta vocal en otra sílaba
                    Retroceder(2);
                    return true;
                }

                // Una consonante tras el grupo consonántico: Forma parte de otra sílaba
                Debug.WriteLine("ACKKC: Consonante no esperada tras un grupo consonántico.");
                return true;
            }

            // Comprobamos qué hay tras las dos consonantes
            if(!MirarLetra(+1, out siguiente))
            {
                // ACC· ==> (ACC)

                // Fin de sílaba. Las dos consonantes forman parte de la sílaba actual
                ConsumirLetra();
                return true;
            }

            if(siguiente.EsVocal)
            {
                // ACCA ==> (AC) (CA) --> Ejemplo: aS-Ta

                // Si tras las dos consonantes hay otra vocal, forma parte de otra sílaba
                return true;
            }

            // Parece que hay tres consonantes seguidas tras las vocales

            // Comprobamos qué hay tras las tres consonantes
            if(!MirarLetra(+2, out siguiente))
            {
                // ACCC· ==> (ACCC)

                // Fin de sílaba. Las tres consonantes forman parte de la sílaba actual
                ConsumirLetras(2);
                return true;
            }

            if(siguiente.EsVocal)
            {
                // ACCCA ==> (ACC) (CA) --> Ejemplo: 

                // Si tras las tres consonantes hay otra vocal, forma parte de otra sílaba
                ConsumirLetra();
                return true;
            }

            // Parece que hay cuatro consonantes seguidas tras las vocales

            // Comprobamos qué hay tras las cuatro consonantes
            if(!MirarLetra(+3, out siguiente))
            {
                // ACCCC ==> (ACCCC)

                // Fin de sílaba. Las cuatro consonantes forman parte de la sílaba actual
                ConsumirLetras(3);
                return true;
            }

            if(siguiente.EsVocal)
            {
                // ACCCCA ==> (ACC) (CCA) --> Ejemplo: aBS-TRac-to

                // Si tras las cuatro consonantes hay otra vocal, forma parte de otra sílaba
                ConsumirLetra();
                return true;
            }

            return false;
        }

        //
        // Lee un grupo consonántico, una unión de dos consonantes que no se pueden separar en
        // diferentes sílabas.
        //
        private bool LeerGrupoConsonántico(in Letra.Info letraActual)
        {
            Debug.Assert(!letraActual.EsVocal);

            // Miramos la letra siguiente, que debe ser también una consonante
            if(!MirarLetra(+1, out Letra.Info letraSig))
                return false;
            if(letraSig.EsVocal)
                // Es una consonante suelta
                return false;
            
            // Comprobamos si la pareja de consonantes forman un grupo consonántico
            else if((letraActual.Letra, letraSig.Letra).EsGrupoConsonántico(config))
            {
                ConsumirLetras(2);
                return true;
            }

            return false;
        }

        //
        // Lee un triptongo, una unión de tres vocales, una débil, una fuerte y otra débil, que forman parte de la
        // misma sílaba.
        //
        private bool LeerTriptongo(in Letra.Info letraActual)
        {
            Debug.Assert(letraActual.EsVocal);

            // Un triptongo debe comenzar por una letra débil
            if(letraActual.TipoDeVocal != TipoDeVocal.Cerrada)
                return false;
            // No debe estar acentuada, de lo contrario formaría un hiato
            if(letraActual.EstáAcentuada)
                return false;

            // Miramos las dos letras siguientes, que deben ser ambas vocales
            if(!MirarLetra(+1, out Letra.Info letraSig1) ||
               !MirarLetra(+2, out Letra.Info letraSig2))
               return false;
            if(!letraSig1.EsVocal || !letraSig2.EsVocal)
                return false; 
            if(letraSig1.TipoDeVocal == TipoDeVocal.Ninguna || letraSig2.TipoDeVocal == TipoDeVocal.Ninguna)
                return false;

            // La segunda vocal debe ser fuerte
            if(letraSig1.TipoDeVocal != TipoDeVocal.Abierta)
                return false;
            
            // La tercera vocal debe ser débil y, al igual que la primera, no debe tener tilde
            if(letraSig2.TipoDeVocal != TipoDeVocal.Cerrada)
                return false;
            if(letraSig2.EstáAcentuada)
                return false;

            // Consumimos las letras e indicamos que hemos encontrado un triptongo
            ConsumirLetras(3);
            return true;
        }

        //
        // Lee un diptongo, una unión de dos vocales, una fuerte y una débil, que forman parte de la misma sílaba.
        //
        private bool LeerDiptongo(in Letra.Info letraActual)
        {
            Debug.Assert(letraActual.EsVocal);

            // Miramos de qué tipo es la letra siguiente. Debe haber una vocal a continuación
            if(!MirarLetra(+1, out Letra.Info letraSig))
                return false;
            if(!letraSig.EsVocal)
                // Si la letra siguiente es una consonante, no hay diptongo
                return false;
            if(letraSig.TipoDeVocal == TipoDeVocal.Ninguna)
                return false;

            if(letraActual.TipoDeVocal == TipoDeVocal.Cerrada)
            {
                // Si la vocal está acentuada, no es diptongo, sino que se rompe en un hiato
                if(letraActual.EstáAcentuada)
                    return false;

                // Si la vocal es débil y tiene tilde, no es diptongo, sino que se rompe en un hiato
                if(letraSig.TipoDeVocal == TipoDeVocal.Cerrada && letraSig.EstáAcentuada)
                    return false;
                
                // Consumimos las letras e indicamos que hemos encontrado un diptongo
                ConsumirLetras(2);
                return true;
            }
            else if(letraActual.TipoDeVocal == TipoDeVocal.Abierta)
            {
                // Si la vocal siguiente también es fuerte, no hay diptongo
                if(letraSig.TipoDeVocal == TipoDeVocal.Abierta)
                    return false;
                // Si la vocal es débil y tiene tilde, no es diptongo, sino que se rompe en un hiato
                if(letraSig.TipoDeVocal == TipoDeVocal.Cerrada && letraSig.EstáAcentuada)
                    return false;
                
                // Consumimos la letras e indicamos que hemos encontrado un diptongo
                ConsumirLetras(2);
                return true;
            }
            else
            {
                // No debería ocurrir nunca que sea TipoDeVocal.Ninguna habiéndose comprobado que es vocal
                Debug.Fail("La letra actual es vocal, pero tiene tipo TipoDeVocal.Ninguna.");
                return false;
            }
        }

        //
        // Lee un hiato, una unión de dos vocales que forman parte de sílabas distintas.
        //
        private bool LeerHiato(in Letra.Info letraActual)
        {
            Debug.Assert(letraActual.EsVocal);

            // Miramos de qué tipo es la letra siguiente. Debe haber una vocal a continuación
            if(!MirarLetra(+1, out Letra.Info letraSig))
                return false;
            if(!letraSig.EsVocal)
                // Si la letra siguiente es una consonante, no hay hiato
                return false;
            if(letraSig.TipoDeVocal == TipoDeVocal.Ninguna)
                return false;

            if(letraActual.TipoDeVocal == TipoDeVocal.Abierta)
            {
                // Si forman dos vocales fuertes seguidas, es hiato simple
                if(letraSig.TipoDeVocal == TipoDeVocal.Abierta)
                {
                    ConsumirLetra();
                    return true;
                }
                
                // Si tras la vocal fuerte va una débil tónica (acentuada), es hiato acentual
                if(letraSig.EstáAcentuada)
                {
                    ConsumirLetra();
                    return true;
                }

                // No es hiato
                return false;
            }
            else if(letraActual.TipoDeVocal == TipoDeVocal.Cerrada)
            {
                // Si la vocal débil actual es tónica, forma hiato acentual con la vocal siguiente
                if(letraActual.EstáAcentuada)
                {
                    ConsumirLetra();
                    return true;
                }

                // Si forman dos vocales débiles seguidas, es hiato
                if(letraSig.TipoDeVocal == TipoDeVocal.Cerrada)
                {
                    ConsumirLetra();
                    return true;
                }

                // No es hiato
                return false;
            }
            else
            {
                // No debería ocurrir nunca que sea TipoDeVocal.Ninguna habiéndose comprobado que es vocal
                Debug.Fail("La letra actual es vocal, pero tiene tipo TipoDeVocal.Ninguna.");
                return false;
            }
        }


        //
        // Lee la letra en la posición indicada sin avanzar la posición de lectura.
        //
        private bool MirarLetra(int avance, out char letra)
        {
            var posición = contexto.Cursor + avance;
            if(posición < 0 || posición >= contexto.Palabra.Length)
            {
                letra = Letra.NoVálida;
                return false;
            }

            letra = contexto.Palabra[posición];

            return letra.EsLetra();
        }

        private bool MirarLetra(out char letra)
        {
            var posición = contexto.Cursor;
            if(posición < 0 || posición >= contexto.Palabra.Length)
            {
                letra = Letra.NoVálida;
                return false;
            }

            letra = contexto.Palabra[posición];

            return true;
        }

        private bool MirarLetra(int avance, out Letra.Info letraInfo)
        {
            bool éxito = MirarLetra(avance, out char letra);

            letraInfo = new Letra.Info(letra);

            return éxito;
        }

        private bool MirarLetra(out Letra.Info letraInfo)
        {
            bool éxito = MirarLetra(0, out char letra);

            letraInfo = new Letra.Info(letra);

            return éxito;
        }


        //
        // Avanza la posición de lectura, consumiendo (aceptando) la actual letra bajo el cursor de
        // lectura como parte del elemento léxico que se está analizando.
        //
        private void ConsumirLetra()
        {
            contexto.Cursor++;
        }
        private void ConsumirLetras(int numLetras)
        {
            contexto.Cursor += numLetras;
        }


        //
        // Retrocede la posición de lectura, moviendo hacia atrás el cursor de lectura.
        //
        private void Retroceder(int numLetras = 1)
        {
            contexto.Cursor -= numLetras;
        }


        // ===================================================
        // Contexto de lectura para el analizador de palabras.
        // ===================================================
        private sealed class Contexto
        {
            // Palabra que está siendo analizada
            public string Palabra;

            /// Posición actual en la palabra.
            public int Posición;
            // Posición del cursor que investiga las letras de la palabra antes de consumir elementos léxicos.
            public int Cursor;
        }
    }
}