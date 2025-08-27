namespace OdixPay.Notifications.Domain.Utils;

public static class Utils
{
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        // Simple email validation
        return System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        // Simple phone number validation (digits only, optional leading +)
        PhoneNumbers.PhoneNumberUtil phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
        try
        {
            var number = phoneUtil.Parse(phoneNumber, null);
            return phoneUtil.IsValidNumber(number);
        }
        catch (PhoneNumbers.NumberParseException)
        {
            return false;
        }
    }

    public static bool IsValidPushNotificationToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        // Simple validation for push notification tokens (e.g., Firebase, APNs)
        // This can be adjusted based on the specific requirements of the push service
        return true; // Placeholder for actual validation logic
    }
}