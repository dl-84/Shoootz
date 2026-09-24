namespace Shoootz.DisagOriginUpdMessage;

public class DisagShooterDto
{
    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public int Birthyear { get; set; }

    public string? InternalID { get; set; }

    public string? Identification { get; set; }

    public int Startnumber { get; set; }

    public DisagTeamDto? Team { get; set; }

    public DisagClubDto? Club { get; set; }
}
