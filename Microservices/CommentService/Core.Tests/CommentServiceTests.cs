using AutoMapper;
using Core.Interface;
using Core.MappingProfiles;
using Core.Models;
using Core.Services;
using Core.ServicesAbstraction;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Core.Tests;

public class CommentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<Comment>> _commentRepositoryMock;
    private readonly Mock<IGrpcTeamService> _grpcTeamServiceMock;
    private readonly Mock<IGrpcPlanService> _grpcPlanServiceMock;
    private readonly IMapper _mapper;
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
        _unitOfWorkMock.Setup(u => u.GetRepository<Comment>()).Returns(_commentRepositoryMock.Object);

        _grpcTeamServiceMock = new Mock<IGrpcTeamService>();
        _grpcPlanServiceMock = new Mock<IGrpcPlanService>();

        var nullLoggerFactory = new NullLoggerFactory();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CommentProfile>(), nullLoggerFactory);
        _mapper = config.CreateMapper();

        _sut = new CommentService(_unitOfWorkMock.Object, _mapper, _grpcTeamServiceMock.Object, _grpcPlanServiceMock.Object);
    }
}
