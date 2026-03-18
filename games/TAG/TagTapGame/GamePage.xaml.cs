using Plugin.NFC;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace TagTapGame;

public partial class GamePage : ContentPage
{
    //TAG definitiuons
    private static string[] BonusTags { get; } = new string[] { "b1", "b2", "b3" };
    private static string[] JokerTags { get; } = new string[] { "j1", "j2" };
    private string[] Tags { get; set; }

    //Game variables
    private ConcurrentDictionary<string, string> JokerUsage { get; } = new();
    private string BonusTag { get; set; }
    private int FoundTags { get; set; }
    private bool FoundBonus { get; set; }
    private DateTime StartTime { get; set; } = default;
    private DateTime EndTime { get; set; } = default;
    private TimeSpan BonusTime => TimeSpan.FromSeconds(FoundBonus ? 30 : 0);
    
    private IDispatcherTimer UpdateTimer { get; }
    private SemaphoreSlim TagLock { get; } = new SemaphoreSlim(1);

    public GamePage() : this("g", Colors.DarkSeaGreen)
	{
	}


    public GamePage(string tagPrefix, Color background)
    {
        //Generate the tag order
        Tags = new string[10];
        for (int i = 0; i < Tags.Length; i++)
            Tags[i] = $"{tagPrefix}{(i + 1)}";

        //Setup UI
        BackgroundColor = background;
        InitializeComponent();

        //Setup update timer
        UpdateTimer = Dispatcher.CreateTimer();
        UpdateTimer.Tick += (_, _) => UpdateUI();
        UpdateTimer.IsRepeating = true;
        UpdateTimer.Interval = TimeSpan.FromMilliseconds(50);
    }

    private void Start()
    {
        FoundTags = 0;
        StartTime = DateTime.Now;
        EndTime = default;
        Tags = Tags.OrderBy((t) => Random.Shared.Next()).ToArray();
        FoundBonus= false;
        BonusTag = BonusTags.OrderBy((t) => Random.Shared.Next()).First();
        JokerUsage.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        //game in progress
        if (StartTime == default)
        {
            NextTag.Text = $"Klik op start";
            Stopwatch.Text = $"om te beginnen";
            StartKnop.Text = "Start";
            StartKnop.IsVisible = true;
        }
        else if (EndTime == default)
        {
            NextTag.Text = $"Scan tag nummer: {FoundTags + 1}";
            Stopwatch.Text = $"{(DateTime.Now - StartTime) - BonusTime}";
            StartKnop.IsVisible = false;
        }
        //finished
        else
        {
            NextTag.Text = $"Klaar!";
            Stopwatch.Text = $"{(EndTime - StartTime) - BonusTime}";
            StartKnop.Text = "Herstart";
            StartKnop.IsVisible = true;
        }
    }

    private void StartKnop_Clicked(object sender, EventArgs e) => Start();

    protected override async void OnAppearing()
    {
        UpdateUI();

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

            //Start UI updates
            UpdateTimer.Start();

            await StartNfcAsync().ConfigureAwait(false);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        UpdateTimer.Stop();

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

    private async Task ShowMessage(string message)
    {
        await Dispatcher.DispatchAsync(async () =>
        {
            NextTag.IsVisible = false;
            MessageLabel.Text = message;
            MessageLabel.IsVisible= true;
            await Task.Delay(1000);
            MessageLabel.IsVisible = false;
            NextTag.IsVisible = true;
        });
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
        });
    }

    private void StopNfc()
    {
        // Some delay because else it does not work or something?
        MainThread.BeginInvokeOnMainThread(() =>
        {
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
        if (StartTime == default || EndTime != default)
        {
            return;
        }

        if (await TagLock.WaitAsync(0))
        {
            if (!tagInfo.IsEmpty)
            {
                var tagMsg = tagInfo.Records.First().Message;

                //joker tags
                if(JokerTags.Contains(tagMsg))
                {
                    if (JokerUsage.TryGetValue(tagMsg, out var joker))
                    {
                        tagMsg = joker;
                    }
                    else
                    {
                        JokerUsage[tagMsg] = Tags[FoundTags];
                        tagMsg = Tags[FoundTags];
                    }
                    await ShowMessage("Joker!");
                }
                //right tag
                if (tagMsg == Tags[FoundTags])
                {
                    //Last tag
                    if (FoundTags + 1 == Tags.Length)
                    {
                        if(EndTime == default)
                            EndTime= DateTime.Now;
                    }
                    else
                    {
                        FoundTags = Math.Clamp(FoundTags + 1, 0, 9);
                        await ShowMessage("Goed!");
                    }
                }
                //Bonus tag
                else if(tagMsg == BonusTag)
                {
                    FoundBonus = true;
                    await ShowMessage("Bonus!");
                }
                //Wrong Tag
                else
                {
                    if(Random.Shared.Next(Tags.Length) < FoundTags * 2)
                        FoundTags = Math.Clamp(FoundTags - 1, 0, 9);
                    await ShowMessage("Fout!");
                }

                TagLock.Release();
            }
        }
    }

    private void Current_OnMessagePublished(ITagInfo tagInfo)
    {

    }

    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {

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