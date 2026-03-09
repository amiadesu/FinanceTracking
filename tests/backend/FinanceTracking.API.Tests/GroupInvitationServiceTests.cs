using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.API.Constants;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Services;

public class GroupInvitationServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task CreateInvitationAsync_ShouldThrowConflict_WhenUserIsAlreadyActiveMember()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var mockHistoryService = new Mock<IGroupHistoryService>();
        var service = new GroupInvitationService(db, mockGroupService.Object, mockHistoryService.Object);

        var targetUserId = Guid.NewGuid();
        
        db.Users.Add(new AppUser { Id = targetUserId, UserName = "TargetUser", Email = "target@test.com" });
        db.GroupMembers.Add(new GroupMember 
        { 
            GroupId = 1, 
            UserId = targetUserId, 
            Active = true 
        });
        
        mockGroupService.Setup(s => s.GetGroupMaxMembersAsync(1)).ReturnsAsync(10);
        await db.SaveChangesAsync();

        var dto = new CreateInvitationDto { TargetUserIdentifier = "target@test.com", Note = "Join us!" };

        Func<Task> act = async () => await service.CreateInvitationAsync(1, Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ErrorMessages.UserAlreadyActiveMember);
    }

    [Fact]
    public async Task AcceptInvitationAsync_ShouldMakeUserActiveMemberAndRecordHistory()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var mockHistoryService = new Mock<IGroupHistoryService>();
        var service = new GroupInvitationService(db, mockGroupService.Object, mockHistoryService.Object);

        var inviteId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        db.GroupInvitations.Add(new GroupInvitation 
        { 
            Id = inviteId, 
            GroupId = 1,
            TargetUserId = targetUserId, 
            Note = "",
            Status = InvitationStatus.Pending 
        });
        
        mockGroupService.Setup(s => s.GetGroupMaxMembersAsync(1)).ReturnsAsync(10);
        await db.SaveChangesAsync();

        await service.AcceptInvitationAsync(inviteId, targetUserId);

        var inviteInDb = await db.GroupInvitations.FindAsync(inviteId);
        inviteInDb!.Status.Should().Be(InvitationStatus.Accepted);

        var newMember = await db.GroupMembers.FirstOrDefaultAsync(m => m.UserId == targetUserId && m.GroupId == 1);
        newMember.Should().NotBeNull();
        newMember!.Active.Should().BeTrue();
        newMember.RoleId.Should().Be(GroupRole.Member);

        mockHistoryService.Verify(h => h.AddHistoryRecord(
            1, 
            targetUserId, 
            targetUserId, 
            HistoryNotes.InvitationAccepted, 
            null, 
            GroupRole.Member, 
            false, 
            true, 
            null, 
            null), 
        Times.Once);
    }
}