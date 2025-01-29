using System.Security.Principal;
using Anonymous.Database;
using Anonymous.Debug;
using Anonymous.Global;
using Anonymous.State;

namespace Anonymous
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            InitializeFunctionality();
            ApplicationState.StateUpdated += UpdateState;
        }

        private void UpdateState(object? sender, EventArgs eventArgs)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ApplicationState.StateInfo state = ApplicationState.GetState();

                if (state.Percentage == 100)
                {
                    StateLabel.Text = "Halt";
                    return;
                }

                string stateText = "(" + state.Percentage.ToString() + "%) " + state.MainTask;

                if (state.ProcessesLeft > 1)
                {
                    stateText += " (And " + (state.ProcessesLeft - 1).ToString() + " more)";
                }

                StateLabel.Text = stateText;
            });
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
                AccountDataManager.EncryptedAccount? encryptedAccount = null;
                await Task.Run(() =>
                {
                    DatabaseManager.InitializeTables();
                    encryptedAccount = AccountDataManager.GetEncryptedAccount();
                });

                if (encryptedAccount == null)
                {
                    TitleLabel.Text = "Welcome!";
                    PasswordLabel.Text = "Please enter a new master password";
                }
                else
                {
                    ApplicationProperties.encryptedAccount = encryptedAccount;
                    TitleLabel.Text = "Welcome back!";
                    PasswordLabel.Text = "Please enter your master password";
                }
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
                AccountDataManager.EncryptedAccount? encryptedAccount = ApplicationProperties.encryptedAccount;
                if (encryptedAccount != null)
                {
                    string? masterPassword = PasswordEntry.Text;

                    if (masterPassword == null || masterPassword == "" || masterPassword.Length < 4)
                    {
                        await DisplayAlert("Not acceptable!", "The password you entered is not acceptable.", "OK");
                        ToggleUIElements(true);
                        return;
                    }
                    bool isPasswordCorrect = false;
                    await Task.Run(() =>
                    {
                        isPasswordCorrect = AccountDataManager.IsMasterPasswordCorrect(encryptedAccount, masterPassword);
                    });

                    if (!isPasswordCorrect)
                    {
                        await DisplayAlert("Unauthorized!", "The password you have entered is wrong.", "OK");
                        ToggleUIElements(true);
                        return;
                    }

                    AccountDataManager.DefinedAccount? account = null;
                    await Task.Run(() => {
                        account = AccountDataManager.GetAccountWithoutKeyring(encryptedAccount, masterPassword);
                    });

                    if (account == null)
                    {
                        await DisplayAlert("Error!", "Unexpected error.", "OK");
                        ToggleUIElements(true);
                        return;
                    }

                    ApplicationProperties.account = account;
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

                    AccountDataManager.DefinedAccount? definedAccount = null;
                    await Task.Run(() =>
                    {
                        definedAccount = AccountDataManager.MakeNewAccount(masterPassword);
                    });
                    if(definedAccount != null)
                    {
                        ApplicationProperties.account = definedAccount;
                        ApplicationProperties.masterPassword = masterPassword;
                    }
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
