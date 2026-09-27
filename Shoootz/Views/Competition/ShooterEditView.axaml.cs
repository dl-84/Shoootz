using Avalonia.Controls;
using Shoootz.Models.Shooter;

namespace Shoootz.Views.Competition;

public partial class ShooterEditView : UserControl
{
    public ShooterEditView()
    {
        InitializeComponent();
    }

    internal ShooterListModel BuildShooter()
    {
        int.TryParse(BirthyearField.Text, out int birthyear);

        return new ShooterListModel
        {
            Birthyear = birthyear,
            Club = ClubField.Text,
            Firstname = FirstnameField.Text ?? string.Empty,
            Id = IdField.Text ?? string.Empty,
            Lastname = LastnameField.Text ?? string.Empty,
        };
    }

    internal void LoadShooter(ShooterListModel shooter)
    {
        IdField.Text = shooter.Id;
        IdField.IsInputEnabled = false;
        LastnameField.Text = shooter.Lastname;
        FirstnameField.Text = shooter.Firstname;
        BirthyearField.Text = shooter.Birthyear == 0 ? null : shooter.Birthyear.ToString();
        ClubField.Text = shooter.Club;
    }
}
