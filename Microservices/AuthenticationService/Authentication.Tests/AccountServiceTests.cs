using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Core.Services;
using Authentication.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace Authentication.Tests;

public class AccountServiceTests
{
    private readonly Mock<IUserStore<AppUser>> _userStoreMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IGenericRepository<UsersJobTitle>> _repositoryMock;
    private readonly Mock<IUnitOfWork<UsersJobTitle>> _unitOfWorkMock;
    private readonly Mock<IOptions<PhotoSettings>> _photoOptions;
    private readonly AccountService _sut;
    public AccountServiceTests()
    {
        _userStoreMock = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
        _repositoryMock = new Mock<IGenericRepository<UsersJobTitle>>();
        _unitOfWorkMock = new Mock<IUnitOfWork<UsersJobTitle>>();
        _photoOptions = new Mock<IOptions<PhotoSettings>>();
        _sut = new AccountService(_userManagerMock.Object, _photoOptions.Object, _repositoryMock.Object, _unitOfWorkMock.Object);
    }


}
