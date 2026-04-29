namespace Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;

/// <summary>Stable message tokens returned in <c>Result.Messages</c> for clients to branch on.</summary>
public static class HtmlPocConflictCodes
{
    /// <summary>A POC of this kind already exists; client must confirm replacement and retry with <c>replaceConfirmed=true</c>.</summary>
    public const string PocKindAlreadyExists = "poc_kind_already_exists";
}
