using System.Security;

namespace WebApi.Models
{
    // <summary>
    // 안경원별 카드정보 DTO
    public class PartnerCardDto
    {
        public string Id { get; set; } = string.Empty;
        public Optician Optician { get; set; } = new Optician();
        public PartnerCardDetail PartnerCardDetail { get; set; } = new PartnerCardDetail();
        public List<CustNote> CustNotes { get; set; } = new List<CustNote>();
        public List<OpticianPromotion> Promotions { get; set; } = new List<OpticianPromotion>();
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public List<SalesOrderDto> SalesOrders { get; set; } = new List<SalesOrderDto>();
        public List<ReturnOrderDto> ReturnOrders { get; set; } = new List<ReturnOrderDto>();
        public List<OpticianHistoryDto> OpticianHistories { get; set; } = new List<OpticianHistoryDto>();
        public List<OpticianClaimDto> Claims { get; set; } = new List<OpticianClaimDto>();
    }

    public class PartnerCardDetail
    {
        public string Id { get; set; } = string.Empty;
        public double SalesAmount { get; set; }
        public string Mind { get; set; } = string.Empty;
        public string Payment { get; set; } = string.Empty;
        public string EHCOpt { get; set; } = string.Empty;
        public string BusinessArea { get; set; } = string.Empty;
        public string BusinessAreaDetail { get; set; } = string.Empty;
        public double TokaiAverageAmount { get; set; }
        public double TokaiGoalAmount { get; set; }
        public double H_Amount { get; set; }
        public double E_Amount { get; set; }
        public double Z_Amount { get; set; }
    }

    // <summary>
    // 안경원별 메모 DTO
    public class CustNote
    {
        public string CustCode { get; set; } = string.Empty;
        public string RegDate { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    // <summary>
    // 안경원별 판촉물 DTO
    public class OpticianPromotion 
    { 
        public int Id { get; set; }
        public string CustCode { get; set; } = string.Empty;
        public DateTime? RegDate { get; set; }
        public string Promotion { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    // <summary>
    // 안경원별 매출정보 DTO
    public class OrderDto
    {
        public string NM_GUBUN { get; set; } = string.Empty;
        public int SEQ { get; set; }
        public decimal AM_MON01 { get; set; }
        public decimal AM_MON02 { get; set; }
        public decimal AM_MON03 { get; set; }
        public decimal AM_MON04 { get; set; }
        public decimal AM_MON05 { get; set; }
        public decimal AM_MON06 { get; set; }
        public decimal AM_MON07 { get; set; }
        public decimal AM_MON08 { get; set; }
        public decimal AM_MON09 { get; set; }
        public decimal AM_MON10 { get; set; }
        public decimal AM_MON11 { get; set; }
        public decimal AM_MON12 { get; set; }
        public decimal AM_TOTAL { get; set; }
    }

    // <summary>
    // 안경원별 판매정보 DTO
    public class SalesOrderDto
    {
        public string CD_ITEMDIV { get; set; } = string.Empty;
        public string NO_ORDER { get; set; } = string.Empty;
        public string CD_ORDER { get; set; } = string.Empty;
        public string DT_COMP { get; set; } = string.Empty;
        public string CD_ITEM { get; set; } = string.Empty;
        public string NM_ITEM { get; set; } = string.Empty;
        public decimal QT_ORDER { get; set; }
        public string CD_COATING { get; set; } = string.Empty;
        public string CD_RL { get; set; } = string.Empty;
        public string AM_SPH { get; set; } = string.Empty;
        public string AM_CYL { get; set; } = string.Empty;
        public decimal AM_AXIS { get; set; }
        public decimal AM_ADD { get; set; }
        public string CD_COLORING { get; set; } = string.Empty;
        public string CD_STATE { get; set; } = string.Empty;
        public string CD_ITEMGROUP { get; set; } = string.Empty;
        public string TX_FUNC { get; set; } = string.Empty;
        public string RT_REFRACTIVE { get; set; } = string.Empty;
        public string CD_DETAIL { get; set; } = string.Empty;
        public string TX_DISCOLOR { get; set; } = string.Empty;
        //Name
        public string NM_ITEMDIV { get; set; } = string.Empty;
        public string NM_ITEMGROUP { get; set; } = string.Empty;
        public string NM_ORDER { get; set; } = string.Empty;
        public string NM_DETAIL { get; set; } = string.Empty;
        public string NM_FUNC { get; set; } = string.Empty;
        public string NM_REFRACTIVE { get; set; } = string.Empty;
        public string NM_DISCOLOR { get; set; } = string.Empty;
        public string NM_COATING { get; set; } = string.Empty;
        public string NM_RL { get; set; } = string.Empty;
        public string NM_COLORING { get; set; } = string.Empty;
    }

    // <summary>
    // 안경원별 반품정보 DTO
    public class ReturnOrderDto
    {
        public string NO_ORDER { get; set; } = string.Empty;
        public string NO_SMCR { get; set; } = string.Empty;
        public string NO_SEQ { get; set; } = string.Empty;
        public string NM_CUST { get; set; } = string.Empty;
        public string CD_ITEM { get; set; } = string.Empty;
        public string NM_ITEM { get; set; } = string.Empty;
        public decimal QT_RETURN { get; set; }
        public string CD_REASON { get; set; } = string.Empty;
        public string TX_REASONDESC { get; set; } = string.Empty;
        public decimal RT_RETURN { get; set; }
        public decimal AM_RETURN { get; set; }
        public string NM_EMP { get; set; } = string.Empty;
    }

    // <summary>
    // 안경원별 이력 DTO
    public class OpticianHistoryDto
    {
        public int ID { get; set; }
        public string CD_CUST { get; set; } = string.Empty;
        public string DT_COMP { get; set; } = string.Empty;
        public string TX_REASON { get; set; } = string.Empty;
        public string TX_PURPOSE { get; set; } = string.Empty;
        public string TX_NOTE { get; set; } = string.Empty;
    }

    public class OpticianClaimDto
    {
        public string CD_CUST { get; set; } = string.Empty;
        public string NO_SMCL { get; set; } = string.Empty;
        public string DT_CLAIM { get; set; } = string.Empty;
        public string NO_ORDER { get; set; } = string.Empty;
        public string NM_INQUIRY { get; set; } = string.Empty;
        public string NM_CLAIM { get; set; } = string.Empty;
        public string NM_RESULT { get; set; } = string.Empty;
        public string TX_CLAIM { get; set; } = string.Empty;
        public string TX_CONTENTS { get; set; } = string.Empty;
    }

    public class OpticianStoreImage
    {
        public string OpticianId { get; set; } = string.Empty;
        public int ImageSlot { get; set; }
        public string? ImageUrl { get; set; } = string.Empty;
    }
}
