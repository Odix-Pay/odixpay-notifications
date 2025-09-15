namespace OdixPay.Notifications.Contracts.Constants;

public static class HubPrefixes
{
    public const string Notifications = "notifications";

    public const string Users = "users";
    public const string Orders = "orders";
    public const string Transactions = "transactions";

    public const string Admins = "admins";

    public const string Analytics = "analytics";

    public static string GetGroup(string prefix, string[]? id = null)
    {
        if (id == null || id.Length == 0 || string.IsNullOrWhiteSpace(prefix))
        {
            return prefix;
        }

        return $"{prefix}:{string.Join(":", id)}";
    }
}
