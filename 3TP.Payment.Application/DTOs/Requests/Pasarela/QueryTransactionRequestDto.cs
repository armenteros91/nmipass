using System.Globalization;
using System.Reflection;

namespace ThreeTP.Payment.Application.DTOs.Requests.Pasarela;

/// <summary>
/// Represents a request to the NMI Transactions Query API.
/// POST URL: https://secure.nmi.com/api/query.php
/// </summary>
public class QueryTransactionRequestDto
{
    /// <summary>
    /// API Security Key assigned to a merchant account.
    /// Can be generated from the merchant control panel in Settings > Security Keys.
    /// </summary>
    public string SecurityKey { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated transaction conditions (e.g., pending, complete).
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Transactions type: cc (credit card), ck (check).
    /// </summary>
    public string? TransactionType { get; set; }

    /// <summary>
    /// action_type Retrieves only transactions with the specified action types.
    /// A combination of the values can be used and should be separated by commas. For example, to
    /// retrieve all transactions with credit or refund actions,
    /// use the following:
    /// Example: action_type=refund,credit
    /// sale: Sale transactions.
    /// refund: Refund transactions.
    /// credit: Credit transactions.
    /// auth: 'Auth Only' transactions.
    /// capture: Captured transactions.
    /// void: Voided transactions.
    /// return: Electronic Check (ACH) transactions that have returned, as well as credit card chargebacks.
    /// </summary>
    public string? ActionType { get; set; }

    /// <summary>
    /// Comma-separated transaction sources (e.g., api, recurring).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Specific Transactions ID(s), comma-separated.
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Subscription ID(s) to retrieve transactions for a subscription.
    /// </summary>
    public string? SubscriptionId { get; set; }

    /// <summary>
    /// Invoice ID, used when report_type is 'invoicing'.
    /// </summary>
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Partial payment ID.
    /// </summary>
    public string? PartialPaymentId { get; set; }

    /// <summary>
    /// Specific Order ID.
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// Billing first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Billing last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Billing address.
    /// </summary>
    public string? Address1 { get; set; }

    /// <summary>
    /// Billing city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Billing state.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Billing zip/postal code.
    /// </summary>
    public string? Zip { get; set; }

    /// <summary>
    /// Billing phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Billing fax number.
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Order description.
    /// </summary>
    public string? OrderDescription { get; set; }

    /// <summary>
    /// Driver's license number.
    /// </summary>
    public string? DriversLicenseNumber { get; set; }

    /// <summary>
    /// Driver's license date of birth.
    /// </summary>
    public string? DriversLicenseDob { get; set; }

    /// <summary>
    /// Driver's license state.
    /// </summary>
    public string? DriversLicenseState { get; set; }

    /// <summary>
    /// Billing email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Credit card number (full or last 4 digits).
    /// </summary>
    public string? CreditCardNumber { get; set; }

    /// <summary>
    /// Merchant defined field (1-20).
    /// Replace '#' with a number.
    /// </summary>
    public Dictionary<int, string>? MerchantDefinedFields { get; set; }

    /// <summary>
    /// Start date for query (Format: YYYYMMDDhhmmss).
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// End date for query (Format: YYYYMMDDhhmmss).
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Type of report to be generated (e.g., receipt, customer_vault, recurring).
    /// </summary>
    public string? ReportType { get; set; }

    /// <summary>
    /// Specific mobile device license.
    /// </summary>
    public string? MobileDeviceLicense { get; set; }

    /// <summary>
    /// Specific mobile device nickname.
    /// </summary>
    public string? MobileDeviceNickname { get; set; }

    /// <summary>
    /// Specific customer vault record ID.
    /// </summary>
    public string? CustomerVaultId { get; set; }

    /// <summary>
    /// Filter customer vault by created/updated date.
    /// </summary>
    public string? DateSearch { get; set; }

    /// <summary>
    /// Limit number of results returned.
    /// </summary>
    public int? ResultLimit { get; set; }

    /// <summary>
    /// Page number for paginated results.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Result order: 'standard' (oldest to newest) or 'reverse' (newest to oldest).
    /// </summary>
    public string? ResultOrder { get; set; }

    /// <summary>
    /// Comma-separated invoice statuses (e.g., open, closed, paid).
    /// </summary>
    public string? InvoiceStatus { get; set; }

    /// <summary>
    /// Return processor details when using profile report_type.
    /// </summary>
    public bool? ProcessorDetails { get; set; }

    /// <summary>
    /// Converts this request object into FormUrlEncodedContent for sending via HttpClient.
    /// </summary>
    public FormUrlEncodedContent ToFormContent()
    {
        var formFields = new List<KeyValuePair<string, string>>();

        foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(this);

            if (value == null)
                continue;

            if (prop.Name == nameof(MerchantDefinedFields) && MerchantDefinedFields != null)
            {
                foreach (var field in MerchantDefinedFields)
                {
                    formFields.Add(new KeyValuePair<string, string>($"merchant_defined_field_{field.Key}",
                        field.Value));
                }
            }
            else if (prop.Name == nameof(ProcessorDetails) && ProcessorDetails.HasValue)
            {
                formFields.Add(new KeyValuePair<string, string>("processor_details",
                    ProcessorDetails.Value ? "true" : "false"));
            }
            else
            {
                // Map C# property names to API expected names
                var apiFieldName = prop.Name switch
                {
                    nameof(SecurityKey) => "security_key",
                    nameof(TransactionType) => "transaction_type",
                    nameof(ActionType) => "action_type",
                    nameof(TransactionId) => "transaction_id",
                    nameof(SubscriptionId) => "subscription_id",
                    nameof(InvoiceId) => "invoice_id",
                    nameof(PartialPaymentId) => "partial_payment_id",
                    nameof(OrderId) => "order_id",
                    nameof(FirstName) => "first_name",
                    nameof(LastName) => "last_name",
                    nameof(Address1) => "address1",
                    nameof(DriversLicenseNumber) => "drivers_license_number",
                    nameof(DriversLicenseDob) => "drivers_license_dob",
                    nameof(DriversLicenseState) => "drivers_license_state",
                    nameof(CreditCardNumber) => "cc_number",
                    nameof(MobileDeviceLicense) => "mobile_device_license",
                    nameof(MobileDeviceNickname) => "mobile_device_nickname",
                    nameof(CustomerVaultId) => "customer_vault_id",
                    nameof(DateSearch) => "date_search",
                    nameof(ResultLimit) => "result_limit",
                    nameof(PageNumber) => "page_number",
                    nameof(ResultOrder) => "result_order",
                    nameof(InvoiceStatus) => "invoice_status",
                    _ => prop.Name.ToLower(CultureInfo.InvariantCulture)
                };

                formFields.Add(new KeyValuePair<string, string>(apiFieldName, value.ToString()!));
            }
        }

        return new FormUrlEncodedContent(formFields);
    }
}