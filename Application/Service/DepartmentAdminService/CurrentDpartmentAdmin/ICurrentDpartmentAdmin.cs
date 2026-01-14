using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.DepartmentAdminService.CurrentDpartmentAdmin
{
    public interface ICurrentDpartmentAdmin
    {
        string UserId { get; }
        int CityId { get; }
    }
}
