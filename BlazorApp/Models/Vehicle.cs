using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models
{
    public class Vehicle
    {
        public int SEQ { get; set; }
        [Required(ErrorMessage = "등록일자를 입력하세요.")]
        public DateTime? DT_COMP { get; set; }
        [Required(ErrorMessage = "사원코드를 입력하세요.")]
        public string CD_EMP { get; set; }
        [Required(ErrorMessage = "차량번호를 입력하세요.")]
        public string NO_CAR { get; set; }
        public string TX_AREA { get; set; }
        public decimal VL_BEFORE { get; set; }
        public decimal VL_AFTER { get; set; }
        public decimal VL_DISTANCE { get; set; }
        public decimal VL_FUEL { get; set; }
    }
}
