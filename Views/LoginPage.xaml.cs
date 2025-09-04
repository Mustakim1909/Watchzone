using Watchzone.Services;

namespace Watchzone.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        LoadSavedCredentials();
    }

        private void LoadSavedCredentials()
    {
        // Check if we have saved credentials
        if (Preferences.ContainsKey("RememberMe") && Preferences.Get("RememberMe", false))
        {
            UsernameEntry.Text = Preferences.Get("SavedUsername", "");
            PasswordEntry.Text = Preferences.Get("SavedPassword", "");
            RememberMeCheckBox.IsChecked = true;

            // Auto validate if we have values
            ValidateAllFields();
        }
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateAllFields();
    }

    private void ValidateAllFields()
    {
        // Clear previous errors
        ClearErrors();

        bool isValid = true;

        // Validate username/email
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            ShowError(UsernameError, "Username or email is required");
            isValid = false;
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError(PasswordError, "Password is required");
            isValid = false;
        }
        else if (PasswordEntry.Text.Length < 6)
        {
            ShowError(PasswordError, "Password must be at least 6 characters");
            isValid = false;
        }

        LoginButton.IsEnabled = isValid;
    }

    private void ClearErrors()
    {
        UsernameError.IsVisible = false;
        PasswordError.IsVisible = false;
    }

    private void ShowError(Label errorLabel, string message)
    {
        errorLabel.Text = message;
        errorLabel.IsVisible = true;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Show loading indicator
        LoginButton.IsEnabled = false;
        LoginButton.Text = "SIGNING IN...";

        try
        {
            // Save credentials if "Remember me" is checked
            if (RememberMeCheckBox.IsChecked)
            {
                Preferences.Set("RememberMe", true);
                Preferences.Set("SavedUsername", UsernameEntry.Text);
                Preferences.Set("SavedPassword", PasswordEntry.Text);
            }
            else
            {
                Preferences.Set("RememberMe", false);
                Preferences.Remove("SavedUsername");
                Preferences.Remove("SavedPassword");
            }

            // Call WooCommerce authentication service
            bool isAuthenticated = await WoocommerceServices.AuthenticateUser(
                UsernameEntry.Text.Trim(),
                PasswordEntry.Text
            );

            if (isAuthenticated)
            {
                // Navigate to the main application page
                await DisplayAlert("Success", "You have successfully logged in!", "OK");
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Error", "Invalid username or password. Please try again.", "OK");
                // Shake animation for wrong credentials
                await AnimateWrongCredentials();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            // Reset button state
            LoginButton.IsEnabled = true;
            LoginButton.Text = "SIGN IN";
        }
    }

    private async Task AnimateWrongCredentials()
    {
        uint timeout = 50;
        await LoginFrame.TranslateTo(-15, 0, timeout);
        await LoginFrame.TranslateTo(15, 0, timeout);
        await LoginFrame.TranslateTo(-10, 0, timeout);
        await LoginFrame.TranslateTo(10, 0, timeout);
        await LoginFrame.TranslateTo(-5, 0, timeout);
        await LoginFrame.TranslateTo(5, 0, timeout);
        LoginFrame.TranslationX = 0;
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        string email = await DisplayPromptAsync("Forgot Password",
            "Please enter your email address to reset your password:",
            "Reset Password", "Cancel", "Email", maxLength: 100, keyboard: Keyboard.Email);

        if (!string.IsNullOrWhiteSpace(email))
        {
            bool success = await WoocommerceServices.ResetPassword(email);
            if (success)
            {
                await DisplayAlert("Success", "Password reset instructions have been sent to your email.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to send reset instructions. Please check your email address.", "OK");
            }
        }
    }

    private async void OnSignUpTapped(object sender, EventArgs e)
    {
        // Navigate to sign up page
        await Navigation.PushAsync(new SignUpPage());
    }
}