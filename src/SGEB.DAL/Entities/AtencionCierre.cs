using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEB.DAL.Entities
{
    public class AtencionCierre
    {
        public int IdCierre { get; set; }
        public int IdIncidente { get; set; }
        public DateTime FechaHoraCierre { get; set; }

        public string ResultadoCierre { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public int? PersonalInvolucrado { get; set; }
    }
}
