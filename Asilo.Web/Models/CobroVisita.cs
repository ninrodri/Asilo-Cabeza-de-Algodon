using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class CobroVisita
    {
        public int Id { get; set; }

        public int CobroId { get; set; }

        [ForeignKey("CobroId")]
        public Cobro? Cobro { get; set; }


        public int VisitaMedicaId { get; set; }

        [ForeignKey("VisitaMedicaId")]
        public VisitaMedica? VisitaMedica { get; set; }
    }
}