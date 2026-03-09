using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.API.Constants;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Services;

public class GroupMemberServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task TransferOwnershipAsync_ShouldSwapRolesAndLogHistory_WhenBothUsersExist()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var mockHistoryService = new Mock<IGroupHistoryService>();
        var service = new GroupMemberService(db, mockGroupService.Object, mockHistoryService.Object);

        var currentOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();

        db.Users.AddRange(
            new AppUser { Id = currentOwnerId, UserName = "OldOwner", Email = "old@test.com" },
            new AppUser { Id = newOwnerId, UserName = "NewOwner", Email = "new@test.com" }
        );

        db.GroupMembers.AddRange(
            new GroupMember { GroupId = 1, UserId = currentOwnerId, RoleId = GroupRole.Owner, Active = true },
            new GroupMember { GroupId = 1, UserId = newOwnerId, RoleId = GroupRole.Member, Active = true }
        );
        await db.SaveChangesAsync();

        var result = await service.TransferOwnershipAsync(1, newOwnerId);

        result.Should().NotBeNull();
        result!.Role.Should().Be(GroupRole.Owner);

        var oldOwnerDb = await db.GroupMembers.FirstAsync(m => m.UserId == currentOwnerId);
        oldOwnerDb.RoleId.Should().Be(GroupRole.Admin, "because the old owner should be demoted to Admin");

        var newOwnerDb = await db.GroupMembers.FirstAsync(m => m.UserId == newOwnerId);
        newOwnerDb.RoleId.Should().Be(GroupRole.Owner, "because the new owner should be promoted");

        mockHistoryService.Verify(h => h.AddHistoryRecord(
            1, newOwnerId, currentOwnerId, HistoryNotes.OwnershipTransferred, GroupRole.Member, GroupRole.Owner, null, null, null, null), 
            Times.Once);
    }

    [Fact]
    public async Task RemoveGroupMemberAsync_ShouldDeactivateMemberAndResetRole()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var mockHistoryService = new Mock<IGroupHistoryService>();
        var service = new GroupMemberService(db, mockGroupService.Object, mockHistoryService.Object);

        var userId = Guid.NewGuid();
        db.GroupMembers.Add(new GroupMember { GroupId = 1, UserId = userId, RoleId = GroupRole.Admin, Active = true });
        await db.SaveChangesAsync();

        var result = await service.RemoveGroupMemberAsync(1, Guid.NewGuid(), userId);

        result.Should().BeTrue();
        
        var memberInDb = await db.GroupMembers.FirstAsync(m => m.UserId == userId);
        memberInDb.Active.Should().BeFalse();
        memberInDb.RoleId.Should().Be(GroupRole.Member, "because roles are reset to default for historical accuracy");
    }
}