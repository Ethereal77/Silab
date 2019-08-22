using System;

namespace Textuo
{
    public sealed class Configuración
    {
        // Indica si se debería tratar la pareja de consonantes TL como un grupo consonántico
        // indivisible (como en México, por ejemplo) o tratarlas de forma separada.
        public bool TratarTlComoGrupoConsonántico = false;
    }
}