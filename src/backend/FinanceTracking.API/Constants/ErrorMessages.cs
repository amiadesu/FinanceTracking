namespace FinanceTracking.API.Constants;

public static class ErrorMessages
{
    // General
    public const string UserNotFound = "User not found.";
    public const string UnauthorizedAccess = "You do not have permission to perform this action.";

    // Groups
    public const string InvalidGroupId = "Invalid group ID.";
    public const string GroupNotFound = "Group not found.";

    // Invitations
    public const string InvitationNotFound = "Invitation not found.";
    public const string UserAlreadyActiveMember = "User is already an active member of this group.";
    public const string PendingInvitationExists = "A pending invitation already exists for this user.";
    public const string InvitationNotPending = "This action can only be performed on pending invitations.";
    public const string CannotCancelInvitation = "You do not have permission to cancel this invitation.";
    public const string InvitationNotFoundOrUnauthorized = "Invitation not found or you are not authorized to view it.";

    // Budget goals
    public const string BudgetGoalNotFound = "Budget goal not found.";
    public const string InvalidBudgetPeriod = "End date must be after start date.";

    // Categories
    public const string CategoryNotFound = "Category not found.";
    public const string CategoryNameRequired = "Category name is required.";
    public const string CategoryColorRequired = "Category color code is required.";
    public const string InvalidColorFormat = "Invalid color format. Please provide a valid hex color code.";
    public const string SystemCategoryCannotBeModified = "System categories cannot be modified.";
    public const string SystemCategoryCannotBeDeleted = "System categories cannot be deleted.";

    // Receipts
    public const string ReceiptNotFound = "Receipt not found.";
    public const string ReceiptProductNameRequired = "Product name is required.";
    public const string ReceiptProductPriceRequired = "Product price is required.";
    public const string ReceiptProductQuantityRequired = "Product quantity is required.";
    public const string TooManyProductCategories = "A product cannot have more than 5 categories.";
    public const string TooManyProductsOnReceipt = "A receipt cannot contain more than 50 products.";

    // Sellers
    public const string SellerIdRequired = "Seller ID is required.";
}