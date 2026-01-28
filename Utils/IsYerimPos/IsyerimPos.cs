using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Pawlio.IsyerimPos;

public static class IsyerimPosUtils
{
    public static bool isTest { get; set; } = false;
    static RestClient client { get => new RestClient(isTest ? "https://apitest.isyerimpos.com/v1/" : "https://api.isyerimpos.com/v1/"); }
    static int merchantId { get => isTest ? 11039 : 2506; }
    static int userId { get => isTest ? 445 : 2073; }
    static string apiKey { get => isTest ? "AQAAAAEAACcQAAAAEBfEWS9XGrZta1wfYC1qeTCA2gJIbDH1+rQEdrY8k4qE8EFtWf3i0Axyb4jY2uReyw==" : "AQAAAAEAACcQAAAAEAEl5rJwuoxDYPNe7nKJcSuVsJQrIwmVZTNaeeyOq3MH0OSH2CpLqvMN1NoNIbR4qw=="; }
    static bool ReflectCost = false;

    //public static GetInstallmentsResponse GetInstallments()
    //{
    //    object cardData = new { CardNumber = "5818775818772285", ReflectCost, Amount };

    //    var request = new RestRequest("getInstallments", Method.Post);
    //    request.AddHeader("Content-Type", "application/json");
    //    request.AddHeader("MerchantId", merchantId);
    //    request.AddHeader("UserId", userId);
    //    request.AddHeader("ApiKey", apiKey);
    //    request.AddJsonBody(cardData);

    //    var res = isyerimPos.Execute(request);
    //    var installments = res.Content?.JsonTo<Result<GetInstallmentsResponse>>();

    //    if (installments == null) throw new Exception("Installments data not found!");
    //    if (!installments.IsDone) throw new Exception(installments.Message ?? "Installments data not avaible!");
    //    return installments.Content;
    //}

    public static async Task<PayRequest3dResponse> PayRequest(int payId, decimal amount, [FromBody] CardData cardData)
    {
        object payData = new
        {
            ReturnUrl = $"https://vetapp.com.tr/api/pay/payResult/{payId}",
            //ReturnUrl = $"http://192.168.1.200:5291/api/pay/payResult/{payId}",
            OrderId = payId,
            ClientIp = "127.0.0.1",
            Installment = 1,
            Amount = amount,
            ReflectCost,
            CardInfo = cardData,
            Products = new List<Product> { new Product { Name = "Hizmet Bedeli", Count = 1, UnitPrice = amount } },
        };

        var request = new RestRequest("payRequest3d", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("MerchantId", merchantId);
        request.AddHeader("UserId", userId);
        request.AddHeader("ApiKey", apiKey);
        request.AddJsonBody(payData);

        var res = await client.ExecuteAsync(request);
        var response = res.Content?.JsonTo<Result<PayRequest3dResponse>>();

        if (response == null) throw new Exception("Ödeme bilgisi bulunamadı!");
        if (!response.IsDone) throw new Exception(response.Message ?? "Ödeme bilgisi alınamadı!");
        return response.Content;
    }

    public static async Task<Result> CheckPayStatus(string orderId)
    {
        var request = new RestRequest($"payResultCheck?uid={orderId}", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("MerchantId", merchantId);
        request.AddHeader("UserId", userId);
        request.AddHeader("ApiKey", apiKey);

        var res = await client.ExecuteAsync(request);
        var response = res.Content?.JsonTo<Result<PayResponse>>();

        if (response == null) return new ResultError("Ödeme sonucu alınamadı!");
        if (!response.IsDone) return new ResultError(response.Message ?? "Ödeme alınamadı, tekrar deneyin!");
        if (response.Content.Status != ProcessStatus.Basarili) return new ResultError(response.Message ?? "Ödeme cevabı alınamadı, tekrar deneyin!");
        return new Result { IsDone = response.IsDone, Message = response.Message };
    }
}


