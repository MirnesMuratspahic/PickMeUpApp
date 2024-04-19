﻿using PickMeUpApp.Models;
using PickMeUpApp.Models.DTO;

namespace PickMeUpApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<(ErrorProvider, List<User>)> GetUsers();
        Task<(ErrorProvider, List<dtoUserRoute>)> GetRoutes();
        Task<(ErrorProvider, List<dtoTheRoute>)> GetUserRoutes(string email);
        Task<(ErrorProvider, string)> UserRegistration(dtoUserRegistration user);
        Task<(ErrorProvider, User)> UserLogin(dtoUserLogin userDto, HttpContext httpContextAccessor);
        Task<(ErrorProvider, dtoTheRoute)> AddRoute(dtoUserRoute route);
        Task<(ErrorProvider, dtoRequest)> SendRequest(dtoRequest dtoRequest);
        Task<(ErrorProvider, List<dtoRequest>)> GetSentRequests(string passengerEmail);
        Task<(ErrorProvider, dtoRequest)> AcceptRequest(dtoRequest request);
        Task<(ErrorProvider, List<dtoRequest>)> GetRecivedRequests(string email);
    }
}
