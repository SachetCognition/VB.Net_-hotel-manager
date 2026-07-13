using Microsoft.JSInterop;

namespace HotelManager.Web.Services.Reports;

/// <summary>Triggers a browser download of a generated PDF via a JS module (no script tag needed).</summary>
public static class PdfDownload
{
    public static async Task DownloadAsync(IJSRuntime js, string fileName, byte[] pdfBytes)
    {
        await using var module = await js.InvokeAsync<IJSObjectReference>("import", "./js/report-download.js");
        await module.InvokeVoidAsync("downloadPdf", fileName, Convert.ToBase64String(pdfBytes));
    }
}
