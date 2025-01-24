using System.Security.Principal;
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

        private void ToggleUIElements(bool enabled)
        {
            PasswordFrame.IsEnabled = enabled;
            PasswordEntry.IsEnabled = enabled;
            AuthButton.IsEnabled = enabled;
        }

        private async void InitializeFunctionality()
        {
            try
            {
                ToggleUIElements(false);
                await Task.Run(() =>
                {
                    DatabaseManager.InitializeTables();
                    bool isAccountPresent = (AccountDataManager.GetEncryptedAccount() != null);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (isAccountPresent)
                        {
                            accountPresent = true;
                            TitleLabel.Text = "Welcome back!";
                            PasswordLabel.Text = "Please enter your master password";

                        }
                        else
                        {
                            accountPresent = false;
                            TitleLabel.Text = "Welcome!";
                            PasswordLabel.Text = "Please enter a new master password";
                        }
                    });
                });
            }
            catch (Exception exception)
            {
                DebugManager.LogException(exception);
                string error = DebugManager.MakeReadableLogs(exception);
                await DisplayAlert("Error!", error, "OK");
            }
            finally
            {
                ToggleUIElements(true);
            }
        }

        private async void OnAuthButtonClicked(object sender, EventArgs e)
        {
            try
            {
                ToggleUIElements(false);
                if (accountPresent)
                {
                    string? masterPassword = PasswordEntry.Text;

                    if (masterPassword == null || masterPassword == "" || masterPassword.Length < 4)
                    {
                        await DisplayAlert("Not acceptable!", "The password you entered is not acceptable.", "OK");
                        ToggleUIElements(true);
                        return;
                    }
                    bool isPassword = false;
                    await Task.Run(() =>
                    {
                        isPassword = ApplicationAuth.SetMasterPassword(masterPassword);
                    });

                    if (!isPassword)
                    {
                        await DisplayAlert("Unauthorized!", "The password you have entered is wrong.", "OK");
                        ToggleUIElements(true);
                        return;
                    }

                    AccountDataManager.DefinedAccount? account = null;
                    await Task.Run(() => {
                        account = ApplicationAuth.GetAccount();
                    });

                    if (account == null)
                    {
                        await DisplayAlert("Error!", "Unexpected error.", "OK");
                        ToggleUIElements(true);
                        return;
                    }
                }
                else
                {
                    bool accepted = await DisplayAlert("Attention!", "Please note that if you lose your password, there will be NO WAY to retrieve it. Make sure to store it securely. Do you want to proceed?", "YES", "NO");
                    if (!accepted)
                    {
                        ToggleUIElements(true);
                        return;
                    }

                    string? masterPassword = PasswordEntry.Text;

                    if (masterPassword == null || masterPassword == "")
                    {
                        await DisplayAlert("Not acceptable!", "Your have to enter a new master password.", "OK");
                        ToggleUIElements(true);
                        return;
                    }

                    if (masterPassword.Length < 4)
                    {
                        await DisplayAlert("Not acceptable!", "Your master password must at least be 4 characters long.", "OK");
                        ToggleUIElements(true);
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
                string error = DebugManager.MakeReadableLogs(exception);
                await DisplayAlert("Error!", error, "OK");
            } finally
            {
                ToggleUIElements(true);
            }
        }
    }
}
