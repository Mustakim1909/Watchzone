using System.Text.RegularExpressions;
using Watchzone.Interfaces;
using Watchzone.Services;

namespace Watchzone.Views;

public partial class SignUpPage : ContentPage
{
    private IWoocommerceServices _woocommerceServices;
	public SignUpPage(IWoocommerceServices woocommerceServices)
	{
		InitializeComponent();
        _woocommerceServices = woocommerceServices;
	}
    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateAllFields();
    }

    private void OnTermsCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        ValidateAllFields();
    }

    private void ValidateAllFields()
    {
        // Clear previous errors
        ClearErrors();

        bool isValid = true;

        // Validate first name
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text))
        {
            ShowError(FirstNameError, "First name is required");
            isValid = false;
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(LastNameEntry.Text))
        {
            ShowError(LastNameError, "Last name is required");
            isValid = false;
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ShowError(EmailError, "Email is required");
            isValid = false;
        }
        else if (!IsValidEmail(EmailEntry.Text))
        {
            ShowError(EmailError, "Please enter a valid email address");
            isValid = false;
        }

        // Validate username
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text))
        {
            ShowError(UsernameError, "Username is required");
            isValid = false;
        }
        else if (UsernameEntry.Text.Length < 3)
        {
            ShowError(UsernameError, "Username must be at least 3 characters");
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

        // Validate confirm password
        if (string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            ShowError(ConfirmPasswordError, "Please confirm your password");
            isValid = false;
        }
        else if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ShowError(ConfirmPasswordError, "Passwords do not match");
            isValid = false;
        }

        // Validate terms
        if (!TermsCheckBox.IsChecked)
        {
            ShowError(TermsError, "You must accept the terms and conditions");
            isValid = false;
        }

        SignUpButton.IsEnabled = isValid;
    }

    private void ClearErrors()
    {
        FirstNameError.IsVisible = false;
        LastNameError.IsVisible = false;
        EmailError.IsVisible = false;
        UsernameError.IsVisible = false;
        PasswordError.IsVisible = false;
        ConfirmPasswordError.IsVisible = false;
        TermsError.IsVisible = false;
    }

    private void ShowError(Label errorLabel, string message)
    {
        errorLabel.Text = message;
        errorLabel.IsVisible = true;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Regular expression for basic email validation
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        // Show loading indicator
        SignUpButton.IsEnabled = false;
        SignUpButton.Text = "CREATING ACCOUNT...";

        try
        {
            // Create customer data object
            var customerData = new
            {
                email = EmailEntry.Text.Trim(),
                first_name = FirstNameEntry.Text.Trim(),
                last_name = LastNameEntry.Text.Trim(),
                username = UsernameEntry.Text.Trim(),
                password = PasswordEntry.Text
            };

            // Call your WooCommerce integration service
            bool success = await _woocommerceServices.RegisterCustomer(customerData);

            if (success)
            {
                await DisplayAlert("Success", "Your account has been created successfully!", "OK");
                // Navigate to the main app page or login page
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "Failed to create account. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            // Reset button state
            SignUpButton.IsEnabled = true;
            SignUpButton.Text = "CREATE ACCOUNT";
        }
    }

    private async void OnTermsTapped(object sender, EventArgs e)
    {
        // Navigate to terms and conditions page
        await DisplayAlert("Terms and Conditions", "Please read our terms and conditions carefully.", "OK");
    }

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        // Navigate to sign in page
        await Navigation.PopAsync();
    }
}