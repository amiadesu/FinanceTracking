namespace FinanceTracking.API.Constants;

public static class ErrorMessages
{
    // General
    public const string UserNotFound = "User not found.";
    public const string InvalidUserId = "Invalid user ID.";
    public const string UnauthorizedAccess = "You do not have permission to perform this action.";
    public const string CannotPerformActionOnYourself = "You cannot perform this action on yourself.";
    public const string CannotPerformActionOnPersonalGroups = "This action cannot be performed on personal groups.";
    public const string NoFilesUploaded = "No files were uploaded.";
    public const string InvalidFileFormat = "Invalid file format.";

    // Groups
    public const string InvalidGroupId = "Invalid group ID.";
    public const string GroupNotFound = "Group not found.";
    public const string UserAlreadyHasPersonalGroup = "User already has a personal group.";
    public const string UserReachedMaxGroups = "User has reached the maximum number of groups.";

    // Group members
    public const string GroupMaxMembersReached = "Group has reached the maximum number of members.";
    public const string UseTransferEndpoint = "Use the transfer ownership endpoint to change the owner.";

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
    public const string TooManyCategoriesInGroup = "A group cannot have more than 50 custom categories.";

    // Receipts
    public const string ReceiptNotFound = "Receipt not found.";
    public const string ReceiptProductNameRequired = "Product name is required.";
    public const string ReceiptProductPriceRequired = "Product price is required.";
    public const string ReceiptProductQuantityRequired = "Product quantity is required.";
    public const string ReceiptShouldHaveAtLeastOneProduct = "A receipt must have at least one product.";
    public const string TooManyProductCategories = "A product cannot have more than 5 categories.";
    public const string TooManyProductsOnReceipt = "A receipt cannot contain more than 50 products.";

    // Sellers
    public const string SellerIdRequired = "Seller ID is required.";
    public const string InvalidSellerId = "Invalid seller ID format.";
    public const string SellerNotOrphaned = "Seller cannot be deleted because it is associated with existing receipts.";

    // Products
    public const string ProductNotOrphaned = "Product cannot be deleted because it is associated with existing receipts.";
}