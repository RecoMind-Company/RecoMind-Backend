using AutoMapper;
using Core.DTOs.PlnaTypeDtos;
using Core.Interfaces;
using Core.Models;
using Core.Service;
using Core.Service.Interface;
using FluentAssertions;
using Infrastructure.GrpcClients.Team;
using Moq;

namespace PlaneService.Tests;

public class PlanService_GetPlanEndDate_Tests
{
    [Fact]
    public async Task GetPlanEndDate_returns_failure_when_plan_type_not_found()
    {
        var planType = new Mock<IPlanType>();
        planType
            .Setup(x => x.GetPlanTypeByName(It.IsAny<string>()))
            .ReturnsAsync((GetPlanTypeDto)null!);

        var sut = CreateSut(planType: planType);

        var result = await sut.GetPlanEndDate("Monthly");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid plan type provided.");
    }

    [Fact]
    public async Task GetPlanEndDate_adds_months_based_on_plan_type()
    {
        var planType = new Mock<IPlanType>();
        planType
            .Setup(x => x.GetPlanTypeByName(It.IsAny<string>()))
            .ReturnsAsync(new GetPlanTypeDto { Name = "monthly", NumOfMonths = 2 });

        var sut = CreateSut(planType: planType);

        var before = DateTime.UtcNow;
        var result = await sut.GetPlanEndDate("MONTHLY");
        var after = DateTime.UtcNow;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOnOrAfter(before.AddMonths(2));
        result.Value.Should().BeOnOrBefore(after.AddMonths(2));
    }

    private static PlanService CreateSut(Mock<IPlanType> planType)
    {
        var unitOfWork = new Mock<IUnitOfWork<Plan>>();
        var mapper = new Mock<IMapper>();
        var status = new Mock<IStatus>();
        var teamGrpc = new Mock<ITeamGrpcClient>();

        return new PlanService(
            unitOfWork.Object,
            mapper.Object,
            status.Object,
            planType.Object,
            teamGrpc.Object
        );
    }
}
