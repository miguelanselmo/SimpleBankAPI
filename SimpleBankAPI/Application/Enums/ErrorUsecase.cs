using System.ComponentModel;

namespace SimpleBankAPI.Application.Enums;

public enum ErrorTypeUsecase
{
    [Description("System error.")]
    System,
    [Description("Business error.")]
    Business,
}
public enum ErrorUsecase
{
    //Account
    [Description("Account not created. Please try again.")]
    AccountNotCreated,
    [Description("Error creating account. Please try again.")]
    AccountCreatError, 
    [Description("Accounts not found.")]
    AccountNotFound,
    [Description("Error getting accounts.")]
    AccountReadError,
    [Description("Error getting account movements.")]
    AccountMovementReadError,
    //Session
    [Description("Error creating session.")]
    SessionCreateError,
    [Description("Invalid authentication.")]
    SessionInvalidAuthentication,
    [Description("User not found.")]
    SessionUserNotFound,
    [Description("Token refresh invalid or expired.")]
    SessionTokenRefreshInvalid,
    [Description("Error renewing session.")]
    SessionRenewError,
    [Description("Session already closed.")]
    SessionAlreadyClosed,
    [Description("Error closing session.")]
    SessionCloseError,
    [Description("Session not found.")]
    SessionNotFound,
    [Description("Error checking session.")]
    SessionCheckError,
    //Transfer
    [Description("The accounts are the same.")]
    TransferSameAccount,
    [Description("Account not found.")]
    TransferAccountNotFound,
    [Description("Balance below amount.")]
    TransferBalanceBelowAmount,
    [Description("Destination account not found.")]
    TransferDestinationAccountNotFound,
    [Description("Account with different currencies.")]
    TransferDifferentCurrencies,
    [Description("Error on transfer.")]
    TransferError,
    //User
    [Description("User not created. Please try again.")]
    UserNotCreated,
    [Description("Username already exists.")]
    UserUsernameExists,
    [Description("Error creating user.")]
    UserCreateError,
}

