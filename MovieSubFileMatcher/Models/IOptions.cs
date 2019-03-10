namespace MovieSubFileMatcher
{
    interface IOptions
    {
        bool DeleteOriginal { get; set; }
        bool MatchSubtitle { get; set; }
        string Target { get; set; }
    }
}