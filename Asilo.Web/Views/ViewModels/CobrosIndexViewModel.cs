using Asilo.Web.Models;

namespace Asilo.Web.ViewModels
{
    public class CobrosIndexViewModel
    {
        public List<CobroPendienteViewModel> Pendientes { get; set; } = new();

        public List<Cobro> Generados { get; set; } = new();
    }

    public class CobroPendienteViewModel
    {
        public int PacienteId { get; set; }

        public string Paciente { get; set; } = "";

        public string FamiliarResponsable { get; set; } = "";

        public int CantidadAtenciones { get; set; }

        public decimal CostoConsulta { get; set; }

        public decimal CostoLaboratorio { get; set; }

        public decimal CostoMedicamentos { get; set; }

        public decimal Subtotal =>
            CostoConsulta +
            CostoLaboratorio +
            CostoMedicamentos;
    }
}