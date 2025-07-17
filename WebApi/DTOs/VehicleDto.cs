using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class VehicleDto
    {
        public int SEQ { get; set; }
        public DateTime? DT_COMP { get; set; }
        public string CD_EMP { get; set; }
        public string NO_CAR { get; set; }
        public string TX_AREA { get; set; }
        public decimal VL_BEFORE { get; set; }
        public decimal VL_AFTER { get; set; }
        public decimal VL_DISTANCE { get; set; }
        public decimal VL_FUEL { get; set; }
    }
}
