namespace Shoootz.DisagOriginUpdMessage;

public class DisagResultDto : DisagStructureBaseDto
{
    public int FullValue { get; set; }

    public decimal DecimalValue { get; set; }

    public DisagShooterDto? Shooter { get; set; }

    public DisagCompetitionDto? Competition { get; set; }
}
