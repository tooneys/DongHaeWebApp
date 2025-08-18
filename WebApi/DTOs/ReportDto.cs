namespace WebApi.DTOs
{
    public class UserPlanTargetReportDto
    {
        public string cd_emp { get; set; } = string.Empty; // 사원코드
        public decimal am_target { get; set; } // 목표금액
        public decimal am_total { get; set; } // 매출금액
        public decimal progress { get; set; } // 달성율
        public decimal am_order_st { get; set; } // 주문금액(ST)
        public decimal am_add_st { get; set; } // 주문금액(ST)
        public decimal am_etc_st { get; set; } // 주문금액(ST)
        public decimal am_total_st { get; set; } // 주문금액(ST)
        public decimal am_order_rx { get; set; } // 주문금액(RX)
        public decimal am_add_rx { get; set; } // 주문금액(Rx)
        public decimal am_etc_rx { get; set; } // 주문금액(RX)
        public decimal am_total_rx { get; set; } // 주문금액(RX)
    } 
}
