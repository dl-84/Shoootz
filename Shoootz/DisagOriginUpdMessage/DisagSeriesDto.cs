namespace Shoootz.DisagOriginUpdMessage;

public class DisagSeriesDto : DisagStructureBaseDto
{
    public int ID { get; set; }

    public int FullValue { get; set; }

    public decimal DecimalValue { get; set; }

    public DisagShooterDto? Shooter { get; set; }

    public DisagCompetitionDto? Competition { get; set; }
}
