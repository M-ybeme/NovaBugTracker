using Microsoft.EntityFrameworkCore;
using NovaBugTracker.Data;
using NovaBugTracker.Models;
using NovaBugTracker.Services.Interfaces;
using NovaBugTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace NovaBugTrackerTests.Services
{
    public class BTTicketServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBTTicketService _ticketService;

        public BTTicketServiceTests()
        {
            // Create an in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _ticketService = new BTTicketService(_dbContext);
        }

        [Fact]
        public async Task BTTicketServiceTest_AddNewTicketAsync_ShouldAddTicketToDatabase_Success()
        {
            // Arrange
            var ticket = new Ticket
            {
                Description = "Ticket description",
                SubmitterUserId = "Submitter user id",
                Title = "Ticket title"
            };

            // Act
            await _ticketService.AddNewTicketAsync(ticket);

            // Assert
            _dbContext.Tickets.Should().Contain(ticket);
        }

        [Fact]
        public async Task BTTicketServiceTest_AddTicketAttachmentAsync_ShouldAddAttachmentToDatabase_Success()
        {
            // Arrange
            var attachment = new TicketAttatchment
            {
                /* initialize attachment properties */
                UserId = "some_user_id" // Set the UserId property here
            };

            // Act
            await _ticketService.AddTicketAttachmentAsync(attachment);

            // Assert
            _dbContext.TicketAttatchments.Should().Contain(attachment);
        }

        [Fact]
        public async Task BTTicketServiceTest_AddTicketCommentAsync_ShouldAddCommentToDatabase_Success()
        {
            // Arrange
            var comment = new TicketComment
            {
                Comment = "This is a test comment",
                UserId = "user123" // Replace with a valid user ID 
            };

            // Act
            await _ticketService.AddTicketCommentAsync(comment);

            // Assert
            _dbContext.TicketComments.Should().Contain(comment);
        }

        [Fact]
        public async Task BTTicketServiceTest_ArchiveTicketAsync_ShouldSetArchivedFlagToTrue_Success()
        {
            // Arrange
            var ticket = new Ticket
            {
                Title = "Test Ticket",
                Description = "Test ticket description",
                SubmitterUserId = "testuser@example.com",
                Archived = false
            };

            // Act
            await _ticketService.ArchiveTicketAsync(ticket);

            // Assert
            ticket.Archived.Should().BeTrue();
        }

        [Fact]
        public async Task BTTicketServiceTest_GetAllTicketsByCompanyIdAsync_ShouldReturnAllTicketsForCompany_Success()
        {
            // Arrange
            var companyId = 1;
            var tickets = new List<Ticket> { /* create tickets for company */ };
            _dbContext.Tickets.AddRange(tickets);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            // Assert
            result.Should().BeEquivalentTo(tickets);
        }

        [Fact]
        public async Task BTTicketServiceTest_GetArchivedTicketsAsync_ShouldReturnArchivedTicketsForCompany_Success()
        {
            // Arrange
            var companyId = 1;
            var tickets = new List<Ticket> { /* create archived tickets for company */ };
            _dbContext.Tickets.AddRange(tickets);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _ticketService.GetArchivedTicketsAsync(companyId);

            // Assert
            result.Should().BeEquivalentTo(tickets);

        }
    }
}
