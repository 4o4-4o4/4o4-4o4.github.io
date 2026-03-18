using Plugin.NFC;

namespace TagTapGame;

public partial class EraseTagPage : ContentPage
{
	public EraseTagPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // In order to support Mifare Classic 1K tags (read/write), you must set legacy mode to true.
        CrossNFC.Legacy = true;

        if (CrossNFC.IsSupported)
        {
            if (!CrossNFC.Current.IsAvailable)
                await ShowError("NFC is not available");

            if (!CrossNFC.Current.IsEnabled)
                await ShowError("NFC is disabled");

            CrossNFC.Current.SetConfiguration(new NfcConfiguration
            {
                DefaultLanguageCode = "en",
                Messages = new UserDefinedMessages
                {
                    NFCSessionInvalidated = "NFC Session Invalidated",
                    NFCSessionInvalidatedButton = "NFC Session Invalidated Button",
                    NFCWritingNotSupported = "NFC Writing Not Supported",
                    NFCDialogAlertMessage = "NFC Dialog Alert Message",
                    NFCErrorRead = "NFC Error Read",
                    NFCErrorEmptyTag = "NFC Error Empty Tag",
                    NFCErrorReadOnlyTag = "NFC Error Read Only Tag",
                    NFCErrorCapacityTag = "NFC Error Capacity Tag",
                    NFCErrorMissingTag = "NFCE rror Missing Tag",
                    NFCErrorMissingTagInfo = "NFC Error Missing TagInfo",
                    NFCErrorNotSupportedTag = "NFC Error Not Supported Tag",
                    NFCErrorNotCompliantTag = "NFC Error Not Compliant Tag",
                    NFCErrorWrite = "NFC Error Write",
                    NFCSuccessRead = "NFC Success Read",
                    NFCSuccessWrite = "NFC Success Write",
                    NFCSuccessClear = "NFC Success Clear"
                }
            });

            await StartNfcAsync().ConfigureAwait(false);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (CrossNFC.IsSupported)
        {
            StopNfc();
        }

        return base.OnBackButtonPressed();
    }

    private async Task ShowError(string message, string title = null)
    {
        await DisplayAlert(title, message, "OK");
        await Navigation.PopModalAsync();
    }

    private async Task ShowInfo(string message, string title = null)
    {
        await DisplayAlert(title, message, "OK");
    }

    private async Task StartNfcAsync()
    {
        // Some delay to prevent Java.Lang.IllegalStateException "Foreground dispatch can only be enabled when your activity is resumed" on Android
        await Task.Delay(500);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
            CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
            CrossNFC.Current.OnTagConnected += Current_OnTagConnected;
            CrossNFC.Current.OnTagDisconnected += Current_OnTagDisconnected;
            CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
            CrossNFC.Current.StartListening();
            CrossNFC.Current.StartPublishing(clearMessage: true);
        });
    }

    private void StopNfc()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CrossNFC.Current.StopPublishing();
            CrossNFC.Current.StopListening();
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
            CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
            CrossNFC.Current.OnTagConnected -= Current_OnTagConnected;
            CrossNFC.Current.OnTagDisconnected -= Current_OnTagDisconnected;
            CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;
            CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
        });
    }

    private async void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        await ShowInfo("Message received!");
    }

    private async void Current_OnMessagePublished(ITagInfo tagInfo)
    {
        await ShowInfo("Tag erased!");
    }

    private async void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        try
        {
            tagInfo.Records = Array.Empty<NFCNdefRecord>();
            CrossNFC.Current.ClearMessage(tagInfo); 
        }
        catch(Exception ex)
        {
            await ShowInfo(ex.Message);
        }
    }

    private void Current_OnTagConnected(object sender, EventArgs e)
    {

    }

    private void Current_OnTagDisconnected(object sender, EventArgs e)
    {

    }

    private void Current_OnTagListeningStatusChanged(bool isListening)
    {

    }

    private void Current_OnNfcStatusChanged(bool isEnabled)
    {

    }
}