﻿using Microsoft.EntityFrameworkCore;
using PickMeUpApp.Models.DTO;
using PickMeUpApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PickMeUpApp.Context;
using PickMeUpApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace PickMeUpApp.Services
{
    public class RouteService:IRouteService
    {
        public ApplicationDbContext DbContext { get; set; }
        public IConfiguration configuration { get; set; }
        public ErrorProvider error = new ErrorProvider() { Status = false };

        public ErrorProvider defaultError = new ErrorProvider() { Status = true, Name = "Property koji ste poslali ne smije biti null!" };
        public RouteService(ApplicationDbContext dbContext, IConfiguration _configuration)
        {
            DbContext = dbContext;
            configuration = _configuration;
        }
        public async Task<(ErrorProvider, List<UserRoute>)> GetRoutes()
        {
            var routes = await DbContext.UserRoutes.Include(x=>x.User).Include(x=>x.Route).ToListAsync();

            if (routes.Count == 0)
            {
                error = new ErrorProvider()
                {
                    Status = true,
                    Name = "Nema ruta u bazi!"
                };
                return (error, null);
            }
            return (error, routes);

        }

        public async Task<(ErrorProvider, TheRoute)> AddRoute(dtoUserRoute route)
        {
            if (route == null)
                return (defaultError, null);

            var tokenValidator = new TokenValidator(configuration.GetSection("AppSettings:Token").Value!);

            if (!tokenValidator.ValidateToken(route.UserToken))
            {
                error = new ErrorProvider()
                {
                    Status = true,
                    Name = "Token nije validan!"
                };
                return (error, null);
            }

            var decodedToken = tokenValidator.DecodeToken(route.UserToken);
            var emailClaim = decodedToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;


            if (route.Route.SeatsNumber == 0)
            {
                error = new ErrorProvider()
                {
                    Status = true,
                    Name = "Broj slobodnih mjesta mora biti minimalno 1"
                };
                return (error, null);
            }

            var userFromDatabase = await DbContext.Users.FirstOrDefaultAsync(x => x.Email == emailClaim);

            if (userFromDatabase == null)
            {
                error = new ErrorProvider()
                {
                    Status = true,
                    Name = "Ne postoji user sa poslanom email adresom!"
                };
                return (error, null);
            }

            var newRoute = new TheRoute()
            {
                Name = route.Route.Name,
                Description = route.Route.Description,
                SeatsNumber = route.Route.SeatsNumber,
                DateAndTime = route.Route.DateAndTime,
                Price = route.Route.Price
            };

            await DbContext.Routes.AddAsync(newRoute);
            await DbContext.SaveChangesAsync();

            var newUserRoute = new UserRoute()
            {
                RouteId = newRoute.Id,
                UserId = userFromDatabase.Id,
                User = userFromDatabase,
                Route = newRoute
            };

            await DbContext.UserRoutes.AddAsync(newUserRoute);
            await DbContext.SaveChangesAsync();

            return (error, route.Route);
        }

    }
}
