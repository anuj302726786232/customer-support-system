using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportDeskAPI.Context;
using SupportDeskAPI.Dto;
using SupportDeskAPI.Entity;

namespace SupportDeskAPI.Services
{
    public class SupportDeskService
    {

        private readonly SupportDbContext _supportDbContext;

        public SupportDeskService(SupportDbContext supportDbContext)
        {
            _supportDbContext = supportDbContext;
        }

        public async Task<SupportDeskResponse<bool>> CretaeUserAsync(RegisterUser registerUser)
        {
            try
            {
                if (string.IsNullOrEmpty(registerUser.Email))
                    return SupportDeskResponse<bool>.Fail("ValidationError", "Email is required");

                if (string.IsNullOrEmpty(registerUser.Password))
                    return SupportDeskResponse<bool>.Fail("ValidationError", "Password is required");

                if (await _supportDbContext.users.AnyAsync(u => u.Email == registerUser.Email))
                    return SupportDeskResponse<bool>.Fail("AlreadyUserExist", "Already User Exist");

                var passwordHasher = new PasswordHasher<Users>();

                var newUser = new Users
                {
                    UserName = registerUser.UserName,
                    Email = registerUser.Email,
                    CreatedAt = DateTime.Now,
                    UserRole = registerUser.UserRole
                };

                newUser.PasswordHash = passwordHasher.HashPassword(newUser, registerUser.Password);

                await _supportDbContext.AddAsync(newUser);
                var isCreated = await _supportDbContext.SaveChangesAsync();

                return isCreated > 0 ? SupportDeskResponse<bool>.Ok(true) : SupportDeskResponse<bool>.Fail("ServerError", "User could not be created");
            }
            catch (ArgumentNullException argEx)
            {
                return SupportDeskResponse<bool>.Fail("ValidationError", argEx.Message);
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<string> LogInUserAsync(string email, string password)
        {
            try
            {
                var User = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (User == null)
                    return null;

                var passwordHasher = new PasswordHasher<Users>();

                var result = passwordHasher.VerifyHashedPassword(User, User.PasswordHash, password);

                return result == PasswordVerificationResult.Success ? User.UserRole.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SupportDeskResponse<bool>> CreateTicketAsync(TicketDto ticketDto, string email)
        {
            try
            {
                var user = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return SupportDeskResponse<bool>.Fail("InvalidUser", "User does not exist");

                if (string.IsNullOrEmpty(ticketDto.Subject))
                    return SupportDeskResponse<bool>.Fail("InvalidCredential", "Subject is required.");

                if (string.IsNullOrEmpty(ticketDto.Description))
                    return SupportDeskResponse<bool>.Fail("InvalidCredential", "Description is required.");

                var newTicket = new Tickets
                {
                    Subject = ticketDto.Subject,
                    Description = ticketDto.Description,
                    Priority = ticketDto.TicketPriority,
                    Status = TicketStatus.Open,
                    Created = DateTime.Now,
                    UserId = user.UserId,
                    AssignedToUserId = 0
                };

                await _supportDbContext.tickets.AddAsync(newTicket);
                var ticketCreated = await _supportDbContext.SaveChangesAsync();

                return ticketCreated > 0 ? SupportDeskResponse<bool>.Ok(true) : SupportDeskResponse<bool>.Fail("ServerError", "Ticket could not be created");

            }
            catch (ArgumentNullException argEx)
            {
                return SupportDeskResponse<bool>.Fail("ValidationError", argEx.Message);
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<List<TicketDetails>>> GetAllTicketDetailsAsync(string email)
        {
            try
            {
                var user = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return SupportDeskResponse<List<TicketDetails>>.Fail("InvalidUser", "User does not exist");

                // var ticketDetailsList = new List<TicketListDto>();
                var ticketList = new List<Tickets>();

                var ticketQuery = user.UserRole == UserRole.Admin
                ? _supportDbContext.tickets.AsQueryable()
                : _supportDbContext.tickets.Where(t => t.UserId == user.UserId);

                var ticketDetailsList = await ticketQuery
                .Select(t => new TicketDetails
                {
                    TicketNumber = t.TicketId,
                    Subject = t.Subject ?? string.Empty,
                    // Description = t.Description ?? string.Empty,
                    Status = t.Status ?? TicketStatus.Open,
                    Priority = t.Priority ?? TicketPriority.Medium,
                    CreatedDate = t.Created,
                    AssignedAdmin = t.AssignedToUserId
                })
                .ToListAsync();

                if (ticketDetailsList.Count == 0)
                    return SupportDeskResponse<List<TicketDetails>>.Fail("NotFound", "No tickets found for current user");

                return SupportDeskResponse<List<TicketDetails>>.Ok(ticketDetailsList);

            }
            catch (Exception ex)
            {
                return SupportDeskResponse<List<TicketDetails>>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<bool>> AssignedTicketAsync(AssignTicketRequest assignTicketRequest, string email)
        {
            try
            {
                await using var transaction = await _supportDbContext.Database.BeginTransactionAsync(); // Transaction Start Here

                var userInfo = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (userInfo.UserRole != UserRole.Admin)
                    return SupportDeskResponse<bool>.Fail("UnauthorizeAccess", "User haven't permission to perform acion");

                var updatedTicket = await _supportDbContext.tickets.FirstOrDefaultAsync(t => t.TicketId == assignTicketRequest.TicketId);
                if (updatedTicket == null)
                    return SupportDeskResponse<bool>.Fail("InvalidTicketId", "Given Ticket Id is Not Valid");

                updatedTicket.AssignedToUserId = userInfo.UserId;
                updatedTicket.Status = assignTicketRequest.TicketStatus;


                var newTicketHist = new TicketsStatusHistory
                {
                    TicketStatus = assignTicketRequest.TicketStatus,
                    TicketId = assignTicketRequest.TicketId,
                    ChangeAt = DateTime.Now,
                    UserId = userInfo.UserId
                };

                await _supportDbContext.ticketsStatusHistory.AddAsync(newTicketHist);
                await _supportDbContext.SaveChangesAsync();

                await transaction.CommitAsync();// Commmit Transaction

                return SupportDeskResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                await _supportDbContext.Database.RollbackTransactionAsync();// Rollback When Something Unwanted Happen
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<TicketDetails>> GetTicketDetailsByIdAsync(long ticketId)
        {
            try
            {
                var ticket = await _supportDbContext.tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
                if (ticket == null)
                    return SupportDeskResponse<TicketDetails>.Fail("NotFound", "No tickets found for current user");

                var ticketDetails = new TicketDetails
                {
                    TicketNumber = ticket.TicketId,
                    Subject = ticket.Subject ?? string.Empty,
                    Description = ticket.Description ?? string.Empty,
                    Status = ticket.Status ?? TicketStatus.Open,
                    Priority = ticket.Priority ?? TicketPriority.Medium,
                    CreatedDate = ticket.Created,
                    AssignedAdmin = ticket.AssignedToUserId
                };

                return SupportDeskResponse<TicketDetails>.Ok(ticketDetails);
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<TicketDetails>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<bool>> UpdateTicketStatusByTicketIdAsync(TicketStatusUpdateRequest ticketStatusUpdateRequest)
        {
            try
            {
                /*var user = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return SupportDeskResponse<bool>.Fail("InvalidUser", "User does not exist");*/

                var ticket = await _supportDbContext.tickets.FirstOrDefaultAsync(t => t.TicketId == ticketStatusUpdateRequest.TicketId);
                if (ticket == null)
                    return SupportDeskResponse<bool>.Fail("NotFound", "No tickets found for current user");

                var newTicketHis = new TicketsStatusHistory
                {
                    TicketId = ticketStatusUpdateRequest.TicketId,
                    TicketStatus = ticketStatusUpdateRequest.TicketStatus,
                    ChangeAt = DateTime.Now,
                    UserId = ticket.UserId
                };

                await _supportDbContext.AddAsync(newTicketHis);
                var newCreated = await _supportDbContext.SaveChangesAsync();
                return newCreated > 0 ? SupportDeskResponse<bool>.Ok(true) : SupportDeskResponse<bool>.Fail("ServerError", "Ticket Status could not be change");
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }

        public async Task<SupportDeskResponse<bool>> CreateTicketCommentAsync(TicketCommentRequest ticketCommentRequest, string email)
        {
            try
            {
                var user = await _supportDbContext.users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return SupportDeskResponse<bool>.Fail("InvalidUser", "User does not exist");

                var ticket = await _supportDbContext.tickets.FirstOrDefaultAsync(t => t.TicketId == ticketCommentRequest.TicketId && t.UserId == user.UserId);
                if (ticket == null)
                    return SupportDeskResponse<bool>.Fail("NotFound", "No tickets found for current user");

                var newComment = new TicketComments
                {
                    TicketId = ticket.TicketId,
                    Comment = ticketCommentRequest.Comment,
                    CreatedAt = DateTime.Now,
                    UserId = ticket.UserId
                };

                await _supportDbContext.AddAsync(newComment);
                var commentCreate = await _supportDbContext.SaveChangesAsync();

                return commentCreate > 0 ? SupportDeskResponse<bool>.Ok(true) : SupportDeskResponse<bool>.Fail("ServerError", "Ticket Comment could not be create");
            }
            catch (Exception ex)
            {
                return SupportDeskResponse<bool>.Fail("ServerError", "Internal Server Error");
            }
        }
    }
}

