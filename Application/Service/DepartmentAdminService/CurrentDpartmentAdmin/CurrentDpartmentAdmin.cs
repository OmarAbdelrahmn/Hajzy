using Application.Extensions;
using Application.Service.DepartmentAdminService.CityAdminContext;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Application.Service.DepartmentAdminService.CurrentDpartmentAdmin
{
    public class CurrentDpartmentAdmin : ICurrentDpartmentAdmin
    {


        private readonly IHttpContextAccessor _httpContext;
        private readonly ApplicationDbcontext _db;

        public string UserId { get; }
        public int CityId { get; }

        public CurrentDpartmentAdmin(
            IHttpContextAccessor httpContext,
            ApplicationDbcontext db)
        {
            UserId = httpContext.HttpContext?
                .User.GetUserId()
                ?? throw new UnauthorizedAccessException();

            CityId = db.Set<DepartmentAdmin>()
                .Where(x => x.UserId == UserId && x.IsActive)
                .Select(x => x.CityId)
                .FirstOrDefault();

            if (CityId == 0)
                throw new UnauthorizedAccessException("User is not CityAdmin");
        }
    }
}


//    public string UserId { get; }
//    public int CityId { get; }

//    public CityAdminContext(
//        IHttpContextAccessor httpContext,
//        ApplicationDbcontext db)
//    {
//        UserId = httpContext.HttpContext?
//            .User.FindFirstValue(ClaimTypes.NameIdentifier)
//            ?? throw new UnauthorizedAccessException();

//        CityId = db.Set<DepartmentAdmin>()
//            .Where(x => x.UserId == UserId && x.IsActive)
//            .Select(x => x.CityId)
//            .FirstOrDefault();

//        if (CityId == 0)
//            throw new UnauthorizedAccessException("User is not CityAdmin");
//    }



//public class CityAdminContext : ICityAdminContext
//{
//    public string? UserId { get; }
//    public int? CityId { get; }
//    public bool IsAuthenticated { get; }
//    public bool IsCityAdmin => CityId.HasValue;

//    public CityAdminContext(
//        IHttpContextAccessor httpContext,
//        ApplicationDbcontext db)
//    {
//        var user = httpContext.HttpContext?.User;

//        if (user?.Identity?.IsAuthenticated != true)
//            return;

//        IsAuthenticated = true;
//        UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

//        CityId = db.Set<DepartmentAdmin>()
//            .Where(x => x.UserId == UserId && x.IsActive)
//            .Select(x => (int?)x.CityId)
//            .FirstOrDefault();
//    }
//}
