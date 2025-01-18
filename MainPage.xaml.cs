using Anonymous.Database;
using Anonymous.Debug;
using Anonymous.Security;

namespace Anonymous
{
    public partial class MainPage : ContentPage
    {
        private bool accountPresent = false;
        public MainPage()
        {
            InitializeComponent();
            InitializeFunctionality();
        }

        private async void InitializeFunctionality()
        {
            try
            {
                PasswordFrame.IsEnabled = false;
                PasswordEntry.IsEnabled = false;
                AuthButton.IsEnabled = false;
                await Task.Run(() =>
                {
                    DatabaseManager.InitializeTables();
                    bool isAccountPresent = AccountDataManager.GetEncryptedAccount() != null;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (isAccountPresent)
                        {
                            accountPresent = true;
                            PasswordLabel.Text = "Please enter your master password";

                        }
                        else
                        {
                            accountPresent = false;
                            PasswordLabel.Text = "Please enter a new master password";
                        }
                        PasswordEntry.IsEnabled = true;
                        AuthButton.IsEnabled = true;
                        PasswordFrame.IsEnabled = true;
                    });
                });
            }
            catch (Exception exception)
            {
                DebugManager.LogException(exception);
            }
        }

        private async void OnAuthButtonClicked(object sender, EventArgs e)
        {
            try
            {
                PasswordFrame.IsEnabled = false;
                PasswordEntry.IsEnabled = false;
                AuthButton.IsEnabled = false;
                if (accountPresent)
                {
                    return;
                }
                else
                {
                    bool accepted = await DisplayAlert("Attention!", "Please note that if you lose your password, there will be NO WAY to retrieve it. Make sure to store it securely. Do you want to proceed?", "YES", "NO");
                    if (!accepted)
                    {
                        PasswordFrame.IsEnabled = true;
                        PasswordEntry.IsEnabled = true;
                        AuthButton.IsEnabled = true;
                        return;
                    }

                    string? masterPassword = PasswordEntry.Text;

                    if (masterPassword == null || masterPassword == "")
                    {
                        await DisplayAlert("Not acceptable!", "Your have to enter a new master password.", "OK");
                        PasswordFrame.IsEnabled = true;
                        PasswordEntry.IsEnabled = true;
                        AuthButton.IsEnabled = true;
                        return;
                    }

                    if (masterPassword.Length < 4)
                    {
                        await DisplayAlert("Not acceptable!", "Your master password must at least be 4 characters long.", "OK");
                        PasswordFrame.IsEnabled = true;
                        PasswordEntry.IsEnabled = true;
                        AuthButton.IsEnabled = true;
                        return;
                    }

                    await Task.Run(() =>
                    {
                        AccountDataManager.MakeNewAccount(masterPassword);
                        ApplicationAuth.SetMasterPassword(masterPassword);
                    });
                }
            } catch (Exception exception)
            {
                DebugManager.LogException(exception);
                string error = "An error occured.\nError code: " + exception.HResult.ToString() + "\nDetails: " + exception.Message;
                await DisplayAlert("Error!", error, "OK");
            } finally
            {
                PasswordFrame.IsEnabled = true;
                PasswordEntry.IsEnabled = true;
                AuthButton.IsEnabled = true;
            }
        }
    }

}
