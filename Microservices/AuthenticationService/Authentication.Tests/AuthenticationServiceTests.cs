using Authentication.Core.DTOs;
using Authentication.Core.Interfaces;
using Authentication.Core.Models;
using Authentication.Core.Services;
using Authentication.Core.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Security.Claims;

namespace Authentication.Tests;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserStore<AppUser>> _userStoreMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly PasswordHasher<AppUser> _passwordHasher;
    private readonly Mock<IOptions<JwtSettings>> _jwtOptions;
    private readonly Mock<IVerificationEmailService> _verficationEmailServiceMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<IGrpcInvitationService> _grpcInvitationServiceMock;
    private readonly Mock<IGrpcTeamService> _grpcTeamServiceMock;
    private readonly Mock<IGenericRepository<UsersJobTitle>> _jobTitleRepositoryMock;
    private readonly Mock<IUnitOfWork<UsersJobTitle>> _jobTitleUnitOfWorkMock;
    private readonly Core.Services.AuthenticationService _sut;
    public AuthenticationServiceTests()
    {

        _userStoreMock = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
        _passwordHasher = new PasswordHasher<AppUser>();
        _jwtOptions = new Mock<IOptions<JwtSettings>>();
        var jwtSettings = new JwtSettings { Audience = "TestAudience", Issuer = "TestIssuer", DurationInHours = 2, SecretKey = "uper-secret-key-that-is-long-enough-for-testing-123456" };
        _jwtOptions.Setup(o => o.Value).Returns(jwtSettings);
        _verficationEmailServiceMock = new Mock<IVerificationEmailService>();
        _emailSenderMock = new Mock<IEmailSender>();
        _grpcInvitationServiceMock = new Mock<IGrpcInvitationService>();
        _grpcTeamServiceMock = new Mock<IGrpcTeamService>();
        _jobTitleRepositoryMock = new Mock<IGenericRepository<UsersJobTitle>>();
        _jobTitleUnitOfWorkMock = new Mock<IUnitOfWork<UsersJobTitle>>();
        _sut = new Core.Services.AuthenticationService(_userManagerMock.Object, _passwordHasher, _jwtOptions.Object,
                                                       _verficationEmailServiceMock.Object, _emailSenderMock.Object,
                                                       _grpcInvitationServiceMock.Object,
                                                       _grpcTeamServiceMock.Object,
                                                       _jobTitleRepositoryMock.Object,
                                                       _jobTitleUnitOfWorkMock.Object);
    }
    [Fact]
    public async Task Register_WhenUserIsExist_ShouldReturnFalseWithMessage()
    {
        // arrange
        var registerDto = new RegisterDto
        {
            Email = "test@gmail.com",
            FullName = "Test user",
            Password = "Test@1234",
            Role = "admin"
        };
        _userManagerMock.Setup(um => um.FindByEmailAsync(registerDto.Email))
                        .ReturnsAsync(new AppUser { Email = registerDto.Email, FullName = registerDto.FullName });
        // act
        var result = await _sut.Register(registerDto);
        // assert
        result.IsAuthenticated.Should().BeFalse();
        result.message.Should().Contain("already registered");
        _userManagerMock.Verify(um => um.FindByEmailAsync(registerDto.Email), Times.Once);
    }
    [Fact]
    public async Task Register_WhenUserRoleIsInvalid_ShouldReturnFalseWithMessage()
    {
        // arrange
        var registerDto = new RegisterDto
        {
            Email = "test@gmail.com",
            FullName = "Test user",
            Password = "Test@1234",
            Role = "InvalidRole"
        };
        _userManagerMock.Setup(um => um.FindByEmailAsync(registerDto.Email)).ReturnsAsync((AppUser?)null);
        // act
        var result = await _sut.Register(registerDto);
        // assert
        result.IsAuthenticated.Should().BeFalse();
        result.message.Should().Contain("Role is not valid!");
        _userManagerMock.Verify(um => um.FindByEmailAsync(registerDto.Email), Times.Once);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<AppUser>()), Times.Never);
    }
    [Fact]
    public async Task Register_WhenUserRoleIsAdmin_ShouldReturnFullAuthenticationDto()
    {
        // arrange
        var registerDto = new RegisterDto
        {
            Email = "test@gmail.com",
            FullName = "Test user",
            Password = "Test@1234",
            Role = "admin"
        };
        var Roles = new List<string> { "admin" };
        var successfullyIdnetityResult = IdentityResult.Success;
        _userManagerMock.Setup(um => um.FindByEmailAsync(registerDto.Email)).ReturnsAsync((AppUser?)null);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(successfullyIdnetityResult);
        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), registerDto.Role)).ReturnsAsync(successfullyIdnetityResult);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(Roles);
        _verficationEmailServiceMock.Setup(em => em.SendVerificationCodeEmail(registerDto.Email)).Returns(Task.CompletedTask);
        // act 
        var result = await _sut.Register(registerDto);
        //assert
        // validate claims
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.Token);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "admin");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == new MailAddress(registerDto.Email).User);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == registerDto.Email);
        jsonToken.Claims.Should().NotContain(c => c.Type == "CompanyId");
        // validate AuthenticationDto props
        result.Name.Should().Be(registerDto.FullName);
        result.Email.Should().Be(registerDto.Email);
        result.IsAuthenticated.Should().BeTrue();
        result.message.Should().Contain("completed sucessfully!");
        result.ExperiesOn.Should().NotBe(null);
        result.Roles.Should().Contain(registerDto.Role);
        result.RefreshToken.Should().NotBe(null);
        result.RefreshTokenExp.Should().NotBe(null);
        // validate calling of methods inside the method
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<AppUser>(), registerDto.Role), Times.Once);
        _verficationEmailServiceMock.Verify(em => em.SendVerificationCodeEmail(registerDto.Email!), Times.Once);
        _emailSenderMock.Verify(em => em.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<AppUser>()), Times.Once);
    }
    [Fact]
    public async Task Register_WhenUserRoleIsTeamLeader_ShouldReturnFullAuthenticationDto()
    {
        // arrange
        var registerDto = new RegisterDto
        {
            Email = "test@gmail.com",
            Role = "teamleader"
        };
        var Roles = new List<string> { "teamleader" };
        var successfllyIdentityResult = IdentityResult.Success;
        _userManagerMock.Setup(um => um.FindByEmailAsync(registerDto.Email)).ReturnsAsync((AppUser?)null);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(successfllyIdentityResult);
        _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), registerDto.Role)).ReturnsAsync(successfllyIdentityResult);
        _emailSenderMock.Setup(es => es.SendEmailAsync(registerDto.Email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(Roles);
        // act
        var result = await _sut.Register(registerDto);
        //assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.Token);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == registerDto.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == registerDto.Role);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == new MailAddress(registerDto.Email).User);
        jsonToken.Claims.Should().NotContain(c => c.Type == "CompanyId");
        jsonToken.Issuer.Should().Be("TestIssuer");
        jsonToken.Audiences.Should().Contain("TestAudience");
        _verficationEmailServiceMock.Verify(v => v.SendVerificationCodeEmail(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
        _emailSenderMock.Verify(es => es.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        result.Name.Should().Be(new MailAddress(registerDto.Email).User);
        result.Email.Should().Be(registerDto.Email);
        result.IsAuthenticated.Should().BeTrue();
        result.message.Should().Contain("completed sucessfully!");
        result.ExperiesOn.Should().NotBe(null);
        result.Roles.Should().Contain(registerDto.Role);
        result.RefreshToken.Should().NotBe(null);
        result.RefreshTokenExp.Should().NotBe(null);
    }
    [Fact]
    public async Task Login_WhenPasswordOrEmailIsIncorrect_ReturnsFalseWithMessage()
    {
        // arrange
        var loginDto = new LoginDto { Email = "test@gmail.com", Password = "Test@2345" };
        var user = new AppUser { Email = loginDto.Email, UserName = "test" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), loginDto.Password)).ReturnsAsync(false);
        // act
        var result = await _sut.Login(loginDto);
        // assert
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
        result.IsAuthenticated.Should().BeFalse();
        result.message.Should().Contain("email or password is incorrect");
    }
    [Fact]
    public async Task Login_WhenUserIsNotFound_ReturnsFalseWithMessage()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@gmail.com", Password = "Test@2345" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync((AppUser?)null);
        // Act
        var result = await _sut.Login(loginDto);
        // Assert
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        result.IsAuthenticated.Should().BeFalse();
        result.message.Should().Contain("email or password is incorrect");
    }
    [Fact]
    public async Task Login_WhenUserIsInAdminRole_ContinueLoginWithoutCallInvitationService()
    {
        // arrange
        var loginDto = new LoginDto { Email = "test@gmail.com", Password = "Test@2345" };
        var user = new AppUser { Email = loginDto.Email, UserName = "test", FullName = "fulltest" };
        var roles = new List<string> { "admin" };
        var teamDto = new TeamDto { CompanyId = "testCompanyId", TeamId = "testTeamId" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), loginDto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
        _grpcTeamServiceMock.Setup(t => t.GetTeamByUserId(It.IsAny<string>())).ReturnsAsync(teamDto);
        // act
        var result = await _sut.Login(loginDto);
        // assert
        // claims
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.Token);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == loginDto.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == new MailAddress(loginDto.Email).User);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == roles.First());
        jsonToken.Claims.Should().Contain(c => c.Type == "CompanyId" && c.Value == teamDto.CompanyId);
        jsonToken.Issuer.Should().Be("TestIssuer");
        jsonToken.Audiences.Should().Contain("TestAudience");
        // 
        result.Name.Should().Be(user.FullName);
        result.Email.Should().Be(user.Email);
        result.IsAuthenticated.Should().BeTrue();
        result.ExperiesOn.Should().NotBe(null);
        result.Token.Should().NotBeNull();
        result.message.Contains("login successfully");
        result.Roles.Contains(roles.First());
        _userManagerMock.Verify(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once());
        _userManagerMock.Verify(um => um.GetRolesAsync(It.IsAny<AppUser>()), Times.Exactly(2));
        _grpcTeamServiceMock.Verify(t => t.GetTeamByUserId(It.IsAny<string>()), Times.Once);
        _grpcInvitationServiceMock.Verify(i => i.LoginAttempt(It.IsAny<string>()), Times.Never);


    }
    [Fact]
    public async Task Login_WhenUserIsInTeamLeaderRole_ContinueLoginWithCallingInvitationService()
    {
        // arrange
        var loginDto = new LoginDto { Email = "test@gmail.com", Password = "Test@1234" };
        var user = new AppUser { Email = "test@gmail.com", FullName = new MailAddress(loginDto.Email).User, UserName = new MailAddress(loginDto.Email).User };
        var roles = new List<string> { "teamleader" };
        var baseToReturn = new BaseToReturnDto { Message = "test", Success = true };
        var team = new TeamDto { CompanyId = "testCompanyId", TeamId = "testTeamId" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
        _grpcInvitationServiceMock.Setup(i => i.LoginAttempt(It.IsAny<string>())).ReturnsAsync(baseToReturn);
        _grpcTeamServiceMock.Setup(t => t.GetTeamByUserId(It.IsAny<string>())).ReturnsAsync(team);
        // act
        var result = await _sut.Login(loginDto);
        // assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.Token);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == loginDto.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == roles.First());
        jsonToken.Claims.Should().Contain(c => c.Type == "CompanyId" && c.Value == team.CompanyId);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jsonToken.Issuer.Should().Be("TestIssuer");
        jsonToken.Audiences.Should().Contain("TestAudience");
        result.Name.Should().Be(user.FullName);
        result.Email.Should().Be(user.Email);
        result.IsAuthenticated.Should().BeTrue();
        result.ExperiesOn.Should().NotBe(null);
        result.Token.Should().NotBeNull();
        result.message.Should().Contain("login successfully");
        result.Roles.Should().Contain(roles.First());
    }
    [Fact]
    public async Task ForgetPassword_WhenUserIsNotFound_ReturnFalseWithMessage()
    {
        // arrange
        var forgetDto = new ForgetPasswordDto { Email = "test@gmail.com" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(forgetDto.Email)).ReturnsAsync((AppUser?)null);
        // act
        var result = await _sut.ForgetPassword(forgetDto);
        // assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email Is NotFound");
        _userManagerMock.Verify(um => um.FindByEmailAsync(forgetDto.Email), Times.Once);
        _verficationEmailServiceMock.Verify(s => s.SendVerificationCodeEmail(forgetDto.Email), Times.Never);
    }
    [Fact]
    public async Task ForgetPassword_WhenUserIsExist_ReturnTrueWithMessage()
    {
        // arrange
        var forgetDto = new ForgetPasswordDto { Email = "test@gmail.com" };
        var user = new AppUser { Email = forgetDto.Email, UserName = "test", FullName = "fulltest" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(forgetDto.Email)).ReturnsAsync(user);
        _verficationEmailServiceMock.Setup(ve => ve.SendVerificationCodeEmail(forgetDto.Email)).Returns(Task.CompletedTask);
        // act
        var result = await _sut.ForgetPassword(forgetDto);
        // assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Email send successfully");
        _userManagerMock.Verify(um => um.FindByEmailAsync(forgetDto.Email), Times.Once);
        _verficationEmailServiceMock.Verify(ve => ve.SendVerificationCodeEmail(forgetDto.Email), Times.Once);
    }
    [Fact]
    public async Task ResetPassword_WhenPasswordIsIncorrect_ReturnsFalseWithMessage()
    {
        // arrange
        var resetDto = new ResetPasswordDto { OldPassword = "OldP@ss1234", NewPassword = "NewP@ss1234", ConfirmNewPassword = "NewP@ss1234" };
        var email = "test@gmail.com";
        var user = new AppUser { Email = email, UserName = "test", FullName = "fulltest" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);
        // act
        var result = await _sut.ResetPassword(resetDto, email);
        // assert
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.CheckPasswordAsync(user, It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Password is not correct");
    }
    [Fact]
    public async Task ResetPassword_WhenPasswordIsCorrrect_ReturnsTrueWithMessage()
    {
        // arrange
        var resetDto = new ResetPasswordDto { OldPassword = "OldP@ss1234", NewPassword = "NewP@ss1234", ConfirmNewPassword = "NewP@ss1234" };
        var email = "test@gmail.com";
        var user = new AppUser { Email = email, UserName = "test", FullName = "fulltest" };
        var identityResult = IdentityResult.Success;
        _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(identityResult);
        // act
        var result = await _sut.ResetPassword(resetDto, email);
        // assert
        _userManagerMock.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<AppUser>()), Times.Once);
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("The password Updated successfully");
    }
    [Fact]
    public async Task GetUserById_WhenUserIsNotFound_ReturnNull()
    {
        // arrange
        var id = "testUserId";
        _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync((AppUser?)null);
        // act
        var result = await _sut.GetUserById(id);
        // assert
        _userManagerMock.Verify(um => um.FindByIdAsync(It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(um => um.GetRolesAsync(It.IsAny<AppUser>()), Times.Never);
        result.Should().BeNull();
    }
    [Fact]
    public async Task GetuserById_WhenUserIsExist_ReturnUserWithJobTitle()
    {
        var id = "testUserId";
        var user = new AppUser { Id = id, Email = "test@gmail.com", UserName = "test", FullName = "fulltest" };
        var jobTitleEntity = new UsersJobTitle { UserId = id, JobTitle = "Software Developer" };

        _userManagerMock.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(user);

        _jobTitleRepositoryMock.Setup(repo =>
            repo.Find(It.IsAny<Expression<Func<UsersJobTitle, bool>>>()))
            .ReturnsAsync(jobTitleEntity);

        var result = await _sut.GetUserById(id);

        _userManagerMock.Verify(um => um.FindByIdAsync(id), Times.Once);
        _jobTitleRepositoryMock.Verify(repo =>
            repo.Find(It.IsAny<Expression<Func<UsersJobTitle, bool>>>()), Times.Once);

        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be(user.FullName);
        result.JobTitle.Should().Be(jobTitleEntity.JobTitle);
    }
}