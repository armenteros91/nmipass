using System.Xml.Serialization;
using ThreeTP.Payment.Application.Helpers;

namespace ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
/// <summary>
/// Modelo de respuesta estatica para consulta de transacciones
/// <para>
/// transaction_type : CC = A credit card transaction. 
/// action_type :  sale
/// transaction_id : identificador de la transaccion
/// </para>
/// </summary>
public class QueryResponseDto
{
    [XmlRoot("nm_response")]
    public class NmResponse
    {
        [XmlElement("transaction")] public Transaction Transaction { get; set; }
    }

    public class Transaction
    {
        [XmlElement("transaction_id")] public string? TransactionId { get; set; }

        [XmlElement("partial_payment_id")] public string? PartialPaymentId { get; set; }

        [XmlElement("partial_payment_balance")]
        public string? PartialPaymentBalance { get; set; }

        [XmlElement("platform_id")] public string? PlatformId { get; set; }

        [XmlElement("transaction_type")] public string? TransactionType { get; set; }

        [XmlElement("condition")] public string? Condition { get; set; }

        [XmlElement("order_id")] public string? OrderId { get; set; }

        [XmlElement("authorization_code")] public string? AuthorizationCode { get; set; }

        [XmlElement("ponumber")] public string? PoNumber { get; set; }

        [XmlElement("order_description")] public string? OrderDescription { get; set; }

        [XmlElement("first_name")] public string? FirstName { get; set; }

        [XmlElement("last_name")] public string? LastName { get; set; }

        [XmlElement("address_1")] public string? Address1 { get; set; }

        [XmlElement("address_2")] public string? Address2 { get; set; }

        [XmlElement("company")] public string? Company { get; set; }

        [XmlElement("city")] public string? City { get; set; }

        [XmlElement("state")] public string? State { get; set; }

        [XmlElement("postal_code")] public string? PostalCode { get; set; }

        [XmlElement("country")] public string? Country { get; set; }

        [XmlElement("email")] public string? Email { get; set; }

        [XmlElement("phone")] public string? Phone { get; set; }

        [XmlElement("fax")] public string? Fax { get; set; }

        [XmlElement("cell_phone")] public string? CellPhone { get; set; }

        [XmlElement("customertaxid")] public string? CustomerTaxId { get; set; }

        [XmlElement("customerid")] public string? CustomerId { get; set; }

        [XmlElement("website")] public string? Website { get; set; }

        [XmlElement("shipping_first_name")] public string? ShippingFirstName { get; set; }

        [XmlElement("shipping_last_name")] public string? ShippingLastName { get; set; }

        [XmlElement("shipping_address_1")] public string? ShippingAddress1 { get; set; }

        [XmlElement("shipping_address_2")] public string? ShippingAddress2 { get; set; }

        [XmlElement("shipping_company")] public string? ShippingCompany { get; set; }

        [XmlElement("shipping_city")] public string? ShippingCity { get; set; }

        [XmlElement("shipping_state")] public string? ShippingState { get; set; }

        [XmlElement("shipping_postal_code")] public string? ShippingPostalCode { get; set; }

        [XmlElement("shipping_country")] public string? ShippingCountry { get; set; }

        [XmlElement("shipping_email")] public string? ShippingEmail { get; set; }

        [XmlElement("shipping_carrier")] public string? ShippingCarrier { get; set; }

        [XmlElement("tracking_number")] public string? TrackingNumber { get; set; }

        [XmlElement("shipping_date")] public string? ShippingDate { get; set; }

        [XmlElement("shipping")] public string? Shipping { get; set; }

        [XmlElement("shipping_phone")] public string? ShippingPhone { get; set; }

        [XmlElement("cc_number")] public string? CcNumber { get; set; }//todo: enmascarar en logs :fcano 

        [XmlElement("cc_hash")] public string? CcHash { get; set; }

        [XmlElement("cc_exp")] public string? CcExp { get; set; }

        [XmlElement("cavv")] 
        [SensitiveData] // decorador para ignorar en logs 
        public string? Cavv { get; set; }

        [XmlElement("cavv_result")] public string? CavvResult { get; set; }

        [XmlElement("xid")] public string? Xid { get; set; }

        [XmlElement("eci")] public string? Eci { get; set; }

        [XmlElement("directory_server_id")] public string? DirectoryServerId { get; set; }

        [XmlElement("three_ds_version")] public string? ThreeDsVersion { get; set; }

        [XmlElement("avs_response")] public string? AvsResponse { get; set; }

        [XmlElement("csc_response")] public string? CscResponse { get; set; }

        [XmlElement("cardholder_auth")] public string? CardholderAuth { get; set; }

        [XmlElement("cc_start_date")] public string? CcStartDate { get; set; }

        [XmlElement("cc_issue_number")] public string? CcIssueNumber { get; set; }

        [XmlElement("check_account")] public string? CheckAccount { get; set; }

        [XmlElement("check_hash")] public string? CheckHash { get; set; }

        [XmlElement("check_aba")] public string? CheckAba { get; set; }

        [XmlElement("check_name")] public string? CheckName { get; set; }

        [XmlElement("account_holder_type")] public string? AccountHolderType { get; set; }

        [XmlElement("account_type")] public string? AccountType { get; set; }

        [XmlElement("sec_code")] public string? SecCode { get; set; }

        [XmlElement("drivers_license_number")] public string? DriversLicenseNumber { get; set; }

        [XmlElement("drivers_license_state")] public string?DriversLicenseState { get; set; }

        [XmlElement("drivers_license_dob")] public string? DriversLicenseDob { get; set; }

        [XmlElement("social_security_number")] public string? SocialSecurityNumber { get; set; }

        [XmlElement("processor_id")] public string? ProcessorId { get; set; }

        [XmlElement("tax")] public string? Tax { get; set; }

        [XmlElement("currency")] public string? Currency { get; set; }

        [XmlElement("surcharge")] public string? Surcharge { get; set; }

        [XmlElement("convenience_fee")] public string? ConvenienceFee { get; set; }

        [XmlElement("misc_fee")] public string? MiscFee { get; set; }

        [XmlElement("misc_fee_name")] public string? MiscFeeName { get; set; }

        [XmlElement("cash_discount")] public string? CashDiscount { get; set; }

        [XmlElement("tip")] public string? Tip { get; set; }

        [XmlElement("card_balance")] public string? CardBalance { get; set; }

        [XmlElement("card_available_balance")] public string? CardAvailableBalance { get; set; }

        [XmlElement("entry_mode")] public string? EntryMode { get; set; }

        [XmlElement("cc_bin")] public string? CcBin { get; set; }

        [XmlElement("cc_type")] public string? CcType { get; set; }

        [XmlElement("signature_image")] public string? SignatureImage { get; set; }

        [XmlElement("duty_amount")] public string? DutyAmount { get; set; }

        [XmlElement("discount_amount")] public string? DiscountAmount { get; set; }

        [XmlElement("national_tax_amount")] public string? NationalTaxAmount { get; set; }

        [XmlElement("summary_commodity_code")] public string? SummaryCommodityCode { get; set; }

        [XmlElement("vat_tax_amount")] public string? VatTaxAmount { get; set; }

        [XmlElement("vat_tax_rate")] public string? VatTaxRate { get; set; }

        [XmlElement("alternate_tax_amount")] public string? AlternateTaxAmount { get; set; }

        [XmlElement("action")] public Action? Action { get; set; }
    }

    public class Action
    {
        [XmlElement("amount")] public string? Amount { get; set; }

        [XmlElement("action_type")] public string? ActionType { get; set; }

        [XmlElement("date")] public string? Date { get; set; }

        [XmlElement("success")] public string? Success { get; set; }

        [XmlElement("ip_address")] public string? IpAddress { get; set; }

        [XmlElement("source")] public string? Source { get; set; }

        [XmlElement("api_method")] public string? ApiMethod { get; set; }

        [XmlElement("tap_to_mobile")] public string? TapToMobile { get; set; }

        [XmlElement("username")] public string? Username { get; set; }

        [XmlElement("response_text")] public string? ResponseText { get; set; }

        [XmlElement("batch_id")] public string? BatchId { get; set; }

        [XmlElement("processor_batch_id")] public string? ProcessorBatchId { get; set; }

        [XmlElement("response_code")] public string? ResponseCode { get; set; }

        [XmlElement("processor_response_text")]
        public string? ProcessorResponseText { get; set; }

        [XmlElement("processor_response_code")]
        public string? ProcessorResponseCode { get; set; }

        [XmlElement("requested_amount")] public string? RequestedAmount { get; set; }

        [XmlElement("device_license_number")] public string? DeviceLicenseNumber { get; set; }

        [XmlElement("device_nickname")] public string? DeviceNickname { get; set; }
    }
}