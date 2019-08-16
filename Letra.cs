using System;

namespace Textuo
{
    public static class Letra
    {
        public const char NoVálida = '\0';

        public static bool TieneTilde(this char letra)
        {
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

        public static bool EsVocal(this char letra, out Vocal tipoDeVocal)
        {
            if(letra != NoVálida)
            {
                tipoDeVocal = TipoDeVocal(letra);
                return EsVocal(letra);
            }

            tipoDeVocal = Vocal.Ninguna;
            return false;
        }

        public static bool EsVocal(this char letra)
        {
            if(letra != NoVálida)
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

        public static Vocal TipoDeVocal(this char letra)
        {
            if(letra != NoVálida)
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

        public static bool EsLetra(this char letra)
        {
            if(letra >= 'a' && letra <= 'z' || letra == 'ñ')
                return true;
            if(letra == 'á' || letra == 'é' || letra == 'í' || letra == 'ó' || letra == 'ú' || letra == 'ü')
                return true;
            
            return false;
        }

        public static bool EsGrupoConsonántico(this (char letraA, char letraB) letras)
        {
            if(letras.letraB == 'l' || letras.letraB == 'r')
            {
                switch(letras.letraA)
                {
                    case 'c':
                    case 'd':
                    case 'f':
                    case 'g':
                    case 'p':
                    case 't':
                    case 'k':
                        return true;
                }
            }

            else if(letras == ('l','l') ||
                    letras == ('r','r') ||
                    letras == ('c','h') ||
                    letras == ('q','u'))
                return true;
            
            return false;
        }
    }

    public enum Vocal
    {
        Ninguna,
        Cerrada,
        Abierta,
    }
}