using Castle.Core.Internal;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NovaBugTracker.Controllers;
using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Models.ChartModels;
using NovaBugTracker.Models.ViewModels;
using NovaBugTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NovaBugTracker.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTTicketService _ticketService;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<BTUser> _signInManager;

        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            //Dependencies
            _logger = A.Fake<ILogger<HomeController>>();
            _userManager = A.Fake<UserManager<BTUser>>();
            _projectService = A.Fake<IBTProjectService>();
            _companyService = A.Fake<IBTCompanyService>();
            _ticketService = A.Fake<IBTTicketService>();
            _context = A.Fake<ApplicationDbContext>();
            _signInManager = A.Fake<SignInManager<BTUser>>();

            _controller = new HomeController(
                _logger,
                _userManager,
                _projectService,
                _companyService,
                _ticketService,
                _context,
                _signInManager);
        }

        [Fact]
        public void HomeController_Dashboard_Returns_Success()
        {
            //Arrange 
            var comapnyId = 1;
            var dashboardViewModel = A.Fake<DashboardViewModel>();

            A.CallTo(() => _projectService.GetAllProjectsByCompanyIdAsync(comapnyId))!.Returns(dashboardViewModel.Projects);
            A.CallTo(() => _ticketService.GetAllTicketsByCompanyIdAsync(comapnyId))!.Returns(dashboardViewModel.Tickets);
            A.CallTo(() => _companyService.GetMembersAsync(comapnyId))!.Returns(dashboardViewModel.Members);
            A.CallTo(() => _companyService.GetCompanyInfoAsync(comapnyId))!.Returns(dashboardViewModel.Company);

            //Act
            var result = _controller.Dashboard();

            //Assert
            result.Should().BeOfType<Task<IActionResult>>();
        }

        [Fact]
        public void Index_WhenUserIsSignedIn_RedirectsToDashboard_Success()
        {
            // Arrange
            var userManager = A.Fake<UserManager<BTUser>>();
            var signInManager = A.Fake<SignInManager<BTUser>>();
            var logger = A.Fake<ILogger<HomeController>>();

            A.CallTo(() => signInManager.IsSignedIn(A<System.Security.Claims.ClaimsPrincipal>.Ignored))
                .Returns(true);

            var controller = new HomeController(
                logger,
                userManager,
                A.Dummy<IBTProjectService>(),
                A.Dummy<IBTCompanyService>(),
                A.Dummy<IBTTicketService>(),
                A.Dummy<ApplicationDbContext>(),
                signInManager
            );

            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
                {
                    User = new System.Security.Claims.ClaimsPrincipal(
                        new System.Security.Claims.ClaimsIdentity(
                            new List<System.Security.Claims.Claim>()
                            {
                                new System.Security.Claims.Claim(
                                    "test_claim",
                                    "test_value"
                                )
                            },
                            "test_authentication_type"
                        )
                    )
                }
            };

            // Act
            var result = controller.Index();

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Mvc.RedirectToActionResult>()
                .Which.ActionName.Should().Be("Dashboard");
        }

        [Fact]
        public void Index_WhenUserIsNotSignedIn_ReturnsView_Success()
        {
            // Arrange
            var userManager = A.Fake<UserManager<BTUser>>();
            var signInManager = A.Fake<SignInManager<BTUser>>();
            var logger = A.Fake<ILogger<HomeController>>();

            A.CallTo(() => signInManager.IsSignedIn(A<System.Security.Claims.ClaimsPrincipal>.Ignored))
                .Returns(false);

            var controller = new HomeController(
                logger,
                userManager,
                A.Dummy<IBTProjectService>(),
                A.Dummy<IBTCompanyService>(),
                A.Dummy<IBTTicketService>(),
                A.Dummy<ApplicationDbContext>(),
                signInManager
            );

            // Act
            var result = controller.Index();

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Mvc.ViewResult>();
        }

        [Fact]
        public async Task PlotlyBarChart_Returns_JsonResult_With_BarData()
        {
            // Arrange
            var user = new BTUser
            {
                CompanyId = 123 // set a mock CompanyId
            };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._))
                .Returns(user);

            var projects = new List<Project>
        {
            new Project { Id = 1, Name = "Project 1", Tickets = new List<Ticket> { new Ticket(), new Ticket() } },
            new Project { Id = 2, Name = "Project 2", Tickets = new List<Ticket> { new Ticket() } },
            new Project { Id = 3, Name = "Project 3", Tickets = new List<Ticket> { new Ticket(), new Ticket(), new Ticket() } },
        };
            A.CallTo(() => _projectService.GetAllProjectsByCompanyIdAsync(user.CompanyId))
                .Returns(projects);

            // Act
            var result = await _controller.PlotlyBarChart() as JsonResult;

            // Assert
            result.Should().NotBeNull();

            var plotlyData = result.Value.Should().BeOfType<PlotlyBarData>().Subject;
            plotlyData.Data.Should().HaveCount(2);

            var barOne = plotlyData.Data.ElementAt(0);
            barOne.Name.Should().Be("Tickets");
            barOne.Type.Should().Be("bar");
            barOne.X.Should().BeEquivalentTo(new[] { "Project 1", "Project 2", "Project 3" });
            barOne.Y.Should().BeEquivalentTo(new[] { 2, 1, 3 });

            var barTwo = plotlyData.Data.ElementAt(1);
            barTwo.Name.Should().Be("Developers");
            barTwo.Type.Should().Be("bar");
            barTwo.X.Should().BeEquivalentTo(new[] { "Project 1", "Project 2", "Project 3" });
            barTwo.Y.Should().BeEquivalentTo(new[] { 0, 0, 0 }); // not mocking the projectService for this test
        }


        [Fact]
        public void Privacy_ReturnsView_Success()
        {
            // Arrange
            var fakeController = A.Fake<HomeController>();
            A.CallTo(() => fakeController.Privacy()).Returns(new ViewResult());

            // Act
            var result = fakeController.Privacy();

            // Assert
            result.Should().BeOfType<ViewResult>(); 
        }

    }
}
