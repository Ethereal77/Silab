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

        public static bool EsVocal(this char letra, out TipoDeVocal tipoDeVocal)
        {
            if(letra != NoVálida)
            {
                tipoDeVocal = TipoDeVocal(letra);
                return EsVocal(letra);
            }

            tipoDeVocal = Textuo.TipoDeVocal.Ninguna;
            return false;
        }

        public static bool EsVocal(this char letra) => "aáeéiíoóuúü".IndexOf(letra) >= 0;

        public static TipoDeVocal TipoDeVocal(this char letra) =>
            "iíuúü".IndexOf(letra) >= 0
                ? Textuo.TipoDeVocal.Cerrada
                : EsVocal(letra)
                     ? Textuo.TipoDeVocal.Abierta
                     : Textuo.TipoDeVocal.Ninguna;

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
                    // Grupos br, cr, kr, dr, fr, gr, pr y tr
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'f':
                    case 'g':
                    case 'k':
                    case 'p':
                    case 't':
                        return true;
                    
                    // Grupos ll y rr
                    case 'l':
                    case 'r':
                        return true;
                }
            }

            else if(letras == ('c','h') ||
                    letras == ('q','u'))
                return true;
            
            return false;
        }
    }

    public struct InfoLetra
    {
        // Última letra leída, o bien 'Letra.NoVálida' si no se ha podido leer.
        public char Letra;
        // Indica si la última letra leída es una vocal.
        public bool EsVocal;
        // Indica si la última letra leída es una vocal acentuada.
        public bool EstáAcentuada;
        // Tipo de la última vocal leída, o bien 'Vocal.Ninguna' si no se ha podido leer o no es una vocal.
        public TipoDeVocal TipoDeVocal;

        public InfoLetra(char letra)
        {
            Letra = letra;

            EsVocal = letra.EsVocal(out TipoDeVocal);
            EstáAcentuada = letra.TieneTilde();
        }
    }

    public enum TipoDeVocal
    {
        Ninguna,
        Cerrada,
        Abierta,
    }
}