namespace Pawlio.IsyerimPos;

#pragma warning disable CS8618

public class InstallmentInfo
{
    public int Installment { get; set; }
    public decimal Amount { get; set; } = 0m;
    public decimal WithdrawAmount { get; set; } = 0m;
    public decimal CostRate { get; set; } = 0m;
    public decimal CostAmount { get; set; } = 0m;
    public decimal NetAmount { get; set; } = 0m;
}

public class CardData
{
    public string CardOwner { get; set; }
    public string CardNo { get; set; }
    public string Month { get; set; }
    public string Year { get; set; }
    public string Cvv { get; set; }
}

public class CardInfo
{
    public int BankId { get; set; }
    public CardType CardType { get; set; }
    public CardSchema CardSchema { get; set; }
    public CardBrand CardBrand { get; set; }
    public bool IsBusinessCard { get; set; }
    public bool IsCreditCard { get; set; }
}

public class GetInstallmentsResponse
{
    public List<InstallmentInfo> Installments { get; set; }

    public CardInfo CardInfo { get; set; }
}

public class CustomerInfo
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Description { get; set; }
}

public class Product
{
    public string Name { get; set; }
    public int Count { get; set; }
    public decimal UnitPrice { get; set; } = 0m;
}

public class Payment
{
    public string AccountOwner { get; set; }
    public string IBAN { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; } = 0m;
    public int SubMerchantId { get; set; }
    public string ExtCustomerId { get; set; }
}

public class PayRequest3dResponse
{
    public string Uid { get; set; }
    public string PaymentLink { get; set; }
    public string ResponseAsHtml { get; set; }
}

public class PayResponse
{
    public string ReturnUrl { get; set; }
    public int Id { get; set; }
    public string Uid { get; set; }
    public decimal NetAmount { get; set; } = 0m;
    public decimal WithdrawnAmount { get; set; } = 0m;
    public decimal FmCostRate { get; set; } = 0m;
    public decimal FmCostAmount { get; set; } = 0m;
    public ProcessStatus Status { get; set; }
    public string AuthCode { get; set; }
    public DateTime CreationTime { get; set; }
    public string ClientIp { get; set; }
    public string OrderId { get; set; }
    public CardData CardInfo { get; set; }
    public CustomerInfo CustomerInfo { get; set; }
    public List<Product> Products { get; set; }
    public int Installment { get; set; }
    public decimal Amount { get; set; } = 0m;
    public bool ReflectCost { get; set; }
}

public enum CardType
{
    Tanimsiz = 0,
    KrediKarti = 1,
    DebitKart = 2,
    Acquiring = 3,
    Onyuklemeli = 4
}

public enum CardSchema
{
    Tanimsiz = 0,
    Visa = 1,
    MasterCard = 2,
    Amex = 3,
    DinersClub = 4,
    Jcb = 5,
    Troy = 6,
    UnionPay = 7,
    ProprietaryDomestic = 8
}

public enum CardBrand
{
    Tanimsiz = 0,
    Advantage = 1,
    Axess = 2,
    BankkartCombo = 3,
    Bonus = 4,
    CardFinans = 5,
    Maximum = 6,
    MilesSmiles = 7,
    Paraf = 8,
    SaglamKart = 9,
    World = 10,
    Bank24 = 11,
    Param = 12,
    HasatKart = 13,
    UretenKart = 14,
    ShopFly = 15,
    Wings = 16,
    Neo = 17,
    Tosla = 18
}

public enum ProcessStatus
{
    Tanimsiz = 0,
    Bekliyor = 1,
    Bildirimde = 2,
    Basarili = 4,
    Basarisiz = 5,
    Iptal = 6,
    IptalSurecinde = 7,
    IadeSurecinde = 8,
    ChargeBackSurecinde = 9
}
#pragma warning restore CS8618
