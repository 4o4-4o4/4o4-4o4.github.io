using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System.Diagnostics;

namespace TagTapGame;

public partial class MainPage : ContentPage
{
    private GamePage redGame = new GamePage("r", Colors.Tomato);
    private GamePage yellowGame = new GamePage("y", Color.FromArgb("#FEDD00"));
    private GamePage greenGame = new GamePage("g", Colors.DarkSeaGreen);
    private ReadTagPage readPage = new ReadTagPage();
    private WriteTagPage writePage = new WriteTagPage();
    private EraseTagPage erasePage = new EraseTagPage();

    public MainPage()
	{
        App.Current
            .On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Pan);
		InitializeComponent();

#if !DEBUG
        MainGrid.RowDefinitions.Remove(ReadTagsRow);
        MainGrid.RowDefinitions.Remove(WriteTagsRow);
        MainGrid.RowDefinitions.Remove(EraseTagsRow);
        ReadTags.IsVisible = false;
        WriteTags.IsVisible = false;
        EraseTags.IsVisible = false;
#endif
    }

    private void Red_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(redGame, true);
    }

    private void Yellow_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(yellowGame, true);
    }

    private void Green_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(greenGame, true);
    }

    private void WriteTags_Clicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(writePage, true);
    }

    private void ReadTags_Clicked(object sender, EventArgs e)
    {
        var devPage = new ReadTagPage();
        Navigation.PushModalAsync(readPage, true);
    }

    private void EraseTags_Clicked(object sender, EventArgs e)
    {
        var devPage = new EraseTagPage();
        Navigation.PushModalAsync(erasePage, true);
    }
}

