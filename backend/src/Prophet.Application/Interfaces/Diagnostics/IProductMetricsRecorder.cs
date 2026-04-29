namespace Prophet.Application.Interfaces.Diagnostics;

/// <summary>Records API request metrics per product for diagnostics (requests/min, 24h counts, last activity/error).</summary>
public interface IProductMetricsRecorder
{
    void RecordRequest(Guid? productId, bool isError);
}
