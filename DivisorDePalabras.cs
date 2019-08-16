using System;
using System.IO;
using System.Collections.Generic;

namespace Textuo
{
    public sealed class DivisorDePalabras
    {
        private readonly List<string> listaSílabas = new List<string>();

        public string[] DividirEnSílabas(string palabra)
        {
            if (string.IsNullOrWhiteSpace(palabra))
                return Array.Empty<string>();

            palabra = palabra.Trim();
            palabra = palabra.ToLower();

            listaSílabas.Clear();
            BuscarSílabas(palabra);

            listaSílabas.Reverse();
            return listaSílabas.ToArray();
        }

        private void BuscarSílabas(string palabra)
        {
            char letra;
            char letraSig = Letra.NoVálida;

            int numVocales = 0;
            string sílaba = "";

            for (int i = palabra.Length - 1; i >= 0; --i)
            {
                bool terminada = false;

                letra = palabra[i];

                bool esVocal = letra.EsVocal(out Vocal tipoVocal);
                bool esVocalSig = letraSig.EsVocal(out Vocal tipoVocalSig);

                var letraCombi = (letra, letraSig);
                string letraCombi2 = letra + (letraSig != Letra.NoVálida ? letraSig.ToString() : "");

                if (esVocal && !esVocalSig)
                    numVocales++;

                if (numVocales > 1 && !esVocalSig)
                    terminada = true;
                else if (!esVocal && !esVocalSig && !letraCombi.EsGrupoConsonántico() && (numVocales == 1 && i > 1))
                    terminada = true;
                else if ((tipoVocal == Vocal.Abierta && tipoVocalSig == Vocal.Abierta) ||
                        (tipoVocal == Vocal.Cerrada && letra.TieneTilde() && esVocalSig) ||
                        (tipoVocal == Vocal.Abierta && (tipoVocalSig == Vocal.Cerrada && letraSig.TieneTilde())) ||
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
        }
    }
}