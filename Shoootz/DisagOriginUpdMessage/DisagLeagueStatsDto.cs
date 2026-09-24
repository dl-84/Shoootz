namespace Shoootz.DisagOriginUpdMessage;

public class DisagLeagueStatsDto : DisagStructureBaseDto
{
    public int CompetitionId { get; set; }

    public string? CompetitionUUID { get; set; }

    public bool IsDataSource { get; set; }

    public int HomeTeamId { get; set; }

    public int GuestTeamId { get; set; }

    public string? LeagueType { get; set; }

    public string? HomeTeamName { get; set; }

    public string? GuestTeamName { get; set; }

    public decimal HomeTeamTotalScore { get; set; }

    public decimal GuestTeamTotalScore { get; set; }

    public decimal HomeTeamPrediction { get; set; }

    public decimal GuestTeamPrediction { get; set; }
}
