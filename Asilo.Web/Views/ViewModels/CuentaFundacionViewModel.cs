namespace Asilo.Web.ViewModels
{
    public class CuentaFundacionViewModel
    {
        public decimal TotalConsultas { get; set; }
        public decimal TotalLaboratorio { get; set; }
        public decimal TotalMedicamentos { get; set; }

        public decimal TotalAdeudado =>
            TotalConsultas +
            TotalLaboratorio +
            TotalMedicamentos;

        public decimal TotalPagado { get; set; }

        public decimal SaldoPendiente =>
            TotalAdeudado - TotalPagado;
    }
}