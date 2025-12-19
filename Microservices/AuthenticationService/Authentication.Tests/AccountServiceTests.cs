using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Core.Services;
using Authentication.Core.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace Authentication.Tests;

public class AccountServiceTests
{
    private readonly Mock<IUserStore<AppUser>> _userStoreMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IGenericRepository<UsersJobTitle>> _jobRepositoryMock;
    private readonly Mock<IUnitOfWork<UsersJobTitle>> _jobUnitOfWorkMock;
    private readonly Mock<IOptions<PhotoSettings>> _photoOptions;
    private readonly AccountService _sut;
    public AccountServiceTests()
    {
        _userStoreMock = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
        _photoOptions = new Mock<IOptions<PhotoSettings>>();
        var photoSettings = new PhotoSettings { BaseUrl = "testBasePath", VirtualPathUrl = "testVirtualPath" };
        _photoOptions.Setup(o => o.Value).Returns(photoSettings);
        _jobRepositoryMock = new Mock<IGenericRepository<UsersJobTitle>>();
        _jobUnitOfWorkMock = new Mock<IUnitOfWork<UsersJobTitle>>();
        _sut = new AccountService(_userManagerMock.Object, _photoOptions.Object, _jobRepositoryMock.Object, _jobUnitOfWorkMock.Object);
    }
    [Fact]
    public async Task GetProfile_whenUserIsNotExist_ReturnNull()
    {
        // arrange
        _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);
        _jobRepositoryMock.Setup(j => j.Find(It.IsAny<Expression<Func<UsersJobTitle, bool>>>())).ReturnsAsync((UsersJobTitle?)null);
        // act
        var result = await _sut.GetProfile("test@gmail.com");
        // assert
        result.Should().BeNull();
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _jobRepositoryMock.Verify(j => j.Find(It.IsAny<Expression<Func<UsersJobTitle, bool>>>()), Times.Once);
    }
    [Fact]
    public async Task GetProfile_WhenUserIsExist_ReturnUser()
    {
        // arrange
        var user = new AppUser { Id = "userId1", Email = "test@gmail.com", FullName = "testFullName", PhotoUrl = "photourl", PhoneNumber = "0123456789" };
        var jobTitle = new UsersJobTitle { Id = "jobId1", JobTitle = "sales manager", UserId = user.Id };
        _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _jobRepositoryMock.Setup(jt => jt.Find(It.IsAny<Expression<Func<UsersJobTitle, bool>>>())).ReturnsAsync(jobTitle);
        // act
        var result = await _sut.GetProfile("test@gmail.com");
        // assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNull();
        result.Email.Should().NotBeNull();
        result.Name.Should().NotBeNull();
        result.Photo.Should().NotBeNull();
        result.Phone.Should().NotBeNull();
        result.JobTitle.Should().NotBeNull();
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _jobRepositoryMock.Verify(jt => jt.Find(It.IsAny<Expression<Func<UsersJobTitle, bool>>>()), Times.Once);
    }



}
